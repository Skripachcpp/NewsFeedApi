using System.Data;
using Dapper;
using Npgsql;

namespace Infrastructure.Data;

public class DpContext(string connectionString) {
  private readonly string _connectionString =
    connectionString ?? throw new ArgumentNullException(nameof(connectionString));

  public IDbConnection OpenConnection() {
    var connection = new NpgsqlConnection(_connectionString);
    connection.Open();

    return connection;
  }
  
  public async Task<int> ExecuteAsync(string sql, CancellationToken cancellationToken = default, object? parameters = default) {
    using var connection = OpenConnection();

    var result = await connection.ExecuteAsync(new CommandDefinition(
      sql,
      parameters: parameters,
      cancellationToken: cancellationToken
    ));

    return result;
  }

  public async Task<IEnumerable<T>> QueryAsync<T>(string sql, CancellationToken cancellationToken = default, object? parameters = default) {
    using var connection = OpenConnection();

    var result = await connection.QueryAsync<T>(new CommandDefinition(
      sql,
      parameters: parameters,
      cancellationToken: cancellationToken
    ));

    return result;
  }
  
  public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, CancellationToken cancellationToken = default, object? parameters = default) {
    using var connection = OpenConnection();
    
    var result = await connection.QueryFirstOrDefaultAsync<T>(new CommandDefinition(
      sql,
      parameters: parameters,
      cancellationToken: cancellationToken
    ));
    
    return result;
  }
  
  private async Task<T> RunInTransaction<T>(Func<IDbConnection, IDbTransaction, Task<T>> func) {
    using var connection = OpenConnection();
    var transaction = connection.BeginTransaction();

    try {
      var result = await func(connection, transaction);
      
      transaction.Commit();

      return result;
    }
    catch {
      transaction.Rollback();
      throw;
    }
  }
  
  public async Task<T> QuerySingleWithTransactionAsync<T>(string sql,
    CancellationToken cancellationToken = default, object? parameters = default) {
    var result = await RunInTransaction<T>(async (connection, transaction) => (
      await connection.QuerySingleAsync<T>(new CommandDefinition(
        sql,
        parameters: parameters,
        transaction: transaction,
        cancellationToken: cancellationToken
      ))));

    return result;
  }
  
  public async Task<T?> QuerySingleOrDefaultWithTransactionAsync<T>(string sql,
    CancellationToken cancellationToken = default, object? parameters = default) {
    var result = await RunInTransaction<T?>(async (connection, transaction) => (
      await connection.QuerySingleOrDefaultAsync<T>(new CommandDefinition(
        sql,
        parameters: parameters,
        transaction: transaction,
        cancellationToken: cancellationToken
      ))));

    return result;
  }

  public async Task<IEnumerable<T>> QueryWithTransactionAsync<T>(string sql,
    CancellationToken cancellationToken = default, object? parameters = default) {
    var result = await RunInTransaction<IEnumerable<T>>(async (connection, transaction) => (
      await connection.QueryAsync<T>(new CommandDefinition(
        sql,
        parameters: parameters,
        transaction: transaction,
        cancellationToken: cancellationToken
      ))));

    return result;
  }

  public async Task<int> ExecuteWithTransactionAsync(string sql, CancellationToken cancellationToken = default, object? parameters = default) {
    var result = await RunInTransaction<int>(async (connection, transaction) => (
      await connection.ExecuteAsync(new CommandDefinition(
        sql,
        parameters: parameters,
        transaction: transaction,
        cancellationToken: cancellationToken
      ))));

    return result;
  }
}