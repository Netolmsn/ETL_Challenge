namespace Wooba.Etl.Console.Infrastructure.Database;

using System.Data;
using Microsoft.Data.Sqlite;
using Wooba.Etl.Console.Domain.Entities;
using Wooba.Etl.Console.Domain.Interfaces;
using Wooba.Etl.Console.Domain.ValueObjects;

public class ClienteRepository : IClienteRepository
{
    private readonly SqliteDbContext _dbContext;

    public ClienteRepository(SqliteDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task CriarTabelaSeNaoExistirAsync()
    {
        using var command = _dbContext.Connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Clientes (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Nome TEXT NOT NULL,
                Email TEXT NOT NULL UNIQUE,
                DataNascimento TEXT NOT NULL,
                Telefone TEXT,
                Cidade TEXT,
                Uf TEXT,
                Revisado INTEGER NOT NULL DEFAULT 0
            );";

        await ((SqliteCommand)command).ExecuteNonQueryAsync();
    }

    public async Task InserirMassaAsync(IEnumerable<Cliente> clientes)
    {
        using var transaction = _dbContext.Connection.BeginTransaction();

        foreach (var cliente in clientes)
        {
            using var command = _dbContext.Connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = @"
                INSERT OR IGNORE INTO Clientes (Nome, Email, DataNascimento, Telefone, Cidade, Uf, Revisado)
                VALUES (@nome, @email, @dataNascimento, @telefone, @cidade, @uf, @revisado);";

            command.Parameters.Add(new SqliteParameter("@nome", cliente.Nome));
            command.Parameters.Add(new SqliteParameter("@email", cliente.Email));
            command.Parameters.Add(new SqliteParameter("@dataNascimento", cliente.DataNascimento.ToString("yyyy-MM-dd")));
            command.Parameters.Add(new SqliteParameter("@telefone", cliente.Telefone));
            command.Parameters.Add(new SqliteParameter("@cidade", cliente.Cidade));
            command.Parameters.Add(new SqliteParameter("@uf", cliente.Uf));
            command.Parameters.Add(new SqliteParameter("@revisado", cliente.Revisado ? 1 : 0));

            await ((SqliteCommand)command).ExecuteNonQueryAsync();
        }

        transaction.Commit();
    }

    public async Task<IEnumerable<Cliente>> ObterTodosAsync()
    {
        var clientes = new List<Cliente>();

        using var command = _dbContext.Connection.CreateCommand();
        command.CommandText = "SELECT Id, Nome, Email, DataNascimento, Telefone, Cidade, Uf, Revisado FROM Clientes;";

        using var reader = await ((SqliteCommand)command).ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var id = reader.GetInt32(0);
            var nome = reader.GetString(1);
            var email = reader.GetString(2);
            var dataNascimento = reader.GetDateTime(3);
            var telefone = reader.IsDBNull(4) ? "" : reader.GetString(4);
            var cidade = reader.IsDBNull(5) ? "" : reader.GetString(5);
            var uf = reader.IsDBNull(6) ? "" : reader.GetString(6);
            var revisado = reader.GetInt32(7) == 1;

            var cliente = new Cliente(nome, email, dataNascimento, telefone, cidade, uf, id);
            if (revisado)
            {
                cliente.MarcarComoRevisado();
            }

            clientes.Add(cliente);
        }

        return clientes;
    }

    public async Task<bool> AtualizarStatusRevisadosAsync(int id, bool revisado)
{
    using var command = _dbContext.Connection.CreateCommand();
    command.CommandText = "UPDATE Clientes SET Revisado = @revisado WHERE Id = @id;";
    command.Parameters.Add(new SqliteParameter("@revisado", revisado ? 1 : 0));
    command.Parameters.Add(new SqliteParameter("@id", id));

    var linhasAfetadas = await ((SqliteCommand)command).ExecuteNonQueryAsync();
    return linhasAfetadas > 0;
}

    public async Task<bool> ExcluirPorIdAsync(int id)
    {
        using var command = _dbContext.Connection.CreateCommand();
        command.CommandText = "DELETE FROM Clientes WHERE Id = @id;";
        command.Parameters.Add(new SqliteParameter("@id", id));

        var linhasAfetadas = await ((SqliteCommand)command).ExecuteNonQueryAsync();
        return linhasAfetadas > 0;
    }

    public async Task<Wooba.Etl.Console.Domain.ValueObjects.ResumoExecucaoVo> ObterResumoExecucaoAsync(int totalLidos, int totalDescartados)
    {
        using var command = _dbContext.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Clientes;";

        var totalInserido = Convert.ToInt32(await ((SqliteCommand)command).ExecuteScalarAsync());

        return new Wooba.Etl.Console.Domain.ValueObjects.ResumoExecucaoVo(totalLidos, totalInserido, totalDescartados);
    }
}