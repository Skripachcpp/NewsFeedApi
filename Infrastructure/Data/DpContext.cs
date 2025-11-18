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

  public async Task<IEnumerable<T>> QueryAsync<T>(string sql, CancellationToken cancellationToken = default) {
    using var connection = OpenConnection();

    var result = await connection.QueryAsync<T>(new CommandDefinition(
      sql,
      cancellationToken: cancellationToken
    ));

    return result;
  }

  public async Task<IEnumerable<T>> QueryWithTransactionAsync<T>(string sql,
    CancellationToken cancellationToken = default, object? parameters = default) {
    using var connection = OpenConnection();
    var transaction = connection.BeginTransaction();

    try {
      var result = await connection.QueryAsync<T>(new CommandDefinition(
        sql,
        parameters: parameters,
        transaction: transaction,
        cancellationToken: cancellationToken
      ));

      transaction.Commit();
      return result;
    }
    catch {
      transaction.Rollback();
      throw;
    }
  }

  public async Task ExecuteWithTransactionAsync(string sql, CancellationToken cancellationToken = default, object? parameters = default) {
    using var connection = OpenConnection();
    var transaction = connection.BeginTransaction();

    try {
      await connection.ExecuteAsync(new CommandDefinition(
        sql,
        parameters: parameters,
        transaction: transaction,
        cancellationToken: cancellationToken
      ));
      transaction.Commit();
    }
    catch {
      transaction.Rollback();
      throw;
    }
  }
}