using System.Data;
using Npgsql;

namespace Infrastructure.Data;

public class DpContext(string connectionString) {
  private readonly string _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
  
  public IDbConnection Connection() {
    return new NpgsqlConnection(_connectionString);
  }
}