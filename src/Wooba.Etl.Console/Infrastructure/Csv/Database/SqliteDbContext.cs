namespace Wooba.Etl.Console.Infrastructure.Database;

using System.Data;
using Microsoft.Data.Sqlite;

public class SqliteDbContext : IDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteDbContext()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
    }

    public IDbConnection Connection => _connection;

    public void Dispose()
    {
        if (_connection.State != ConnectionState.Closed)
        {
            _connection.Close();
            _connection.Dispose();
        }
    }
}