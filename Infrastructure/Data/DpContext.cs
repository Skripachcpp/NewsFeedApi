using System.Data;
using System.Dynamic;
using Dapper;
using Domain.DTOs;
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

  public async Task<PageDto<T>> PageAsync<T>(
    string sql, 
    string? sqt = default, 
    CancellationToken cancellationToken = default,
    object? parameters = default,
    int offset = 0, 
    int count = 100) {
    var connection = OpenConnection();

    var parametersExpando = new ExpandoObject();
    var dict = parametersExpando as IDictionary<string, object?>;
    if (parameters != null) {
      foreach (var prop in parameters.GetType().GetProperties()) {
        dict[prop.Name] = prop.GetValue(parameters);
      }  
    }
    
    if (!dict.ContainsKey("Offset")) dict.Add("Offset", offset);
    if (!dict.ContainsKey("Count")) dict.Add("Count", count);

    var multiple = await connection.QueryMultipleAsync(new CommandDefinition(
      @$"
        {sql}
        {(sqt == null ? "" : "LIMIT @Count OFFSET @Offset;")}
        {sqt}
      ",
      parameters: parametersExpando,
      cancellationToken: cancellationToken
    ));

    var items = await multiple.ReadAsync<T>();
    var total = await multiple.ReadSingleAsync<long>();

    var page = new PageDto<T> {
      Total = total,
      Count = count,
      Offset = offset,
      Items = items
    };
    
    return page;
  }

  public async Task<int> ExecuteAsync(string sql, CancellationToken cancellationToken = default,
    object? parameters = default) {
    using var connection = OpenConnection();

    var result = await connection.ExecuteAsync(new CommandDefinition(
      sql,
      parameters: parameters,
      cancellationToken: cancellationToken
    ));

    return result;
  }

  public async Task<IEnumerable<T>> QueryAsync<T>(string sql, CancellationToken cancellationToken = default,
    object? parameters = default) {
    using var connection = OpenConnection();

    var result = await connection.QueryAsync<T>(new CommandDefinition(
      sql,
      parameters: parameters,
      cancellationToken: cancellationToken
    ));

    return result;
  }

  public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, CancellationToken cancellationToken = default,
    object? parameters = default) {
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

  public async Task<int> ExecuteWithTransactionAsync(string sql, CancellationToken cancellationToken = default,
    object? parameters = default) {
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