namespace Wooba.Etl.Console.Infrastructure.Csv;

using System.IO;
using System.Runtime.CompilerServices; // <-- Adicionado para o EnumeratorCancellation
using Wooba.Etl.Console.Domain.Interfaces;
using Wooba.Etl.Console.Domain.ValueObjects;

public class CsvReader : IClienteReader
{
    public async IAsyncEnumerable<RawClienteCsvDto> LerClientesAsync(
        string caminhoArquivo, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!File.Exists(caminhoArquivo))
        {
            throw new FileNotFoundException($"Arquivo CSV não encontrado no caminho: {caminhoArquivo}");
        }

        using var reader = new StreamReader(caminhoArquivo);
        int numeroLinha = 0;
        string? linha;

        while ((linha = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            numeroLinha++;

            if (string.IsNullOrWhiteSpace(linha)) continue;

            // Ignorar cabeçalho
            if (numeroLinha == 1 && (linha.Contains("nome", StringComparison.OrdinalIgnoreCase) || linha.Contains("email", StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var colunas = linha.Split(',');
            if (colunas.Length < 3)
            {
                colunas = linha.Split(';');
            }

            var nome = colunas.Length > 0 ? colunas[0].Trim() : string.Empty;
            var email = colunas.Length > 1 ? colunas[1].Trim() : string.Empty;
            var dataNascimento = colunas.Length > 2 ? colunas[2].Trim() : string.Empty;
            var telefone = colunas.Length > 3 ? colunas[3].Trim() : string.Empty;
            var cidade = colunas.Length > 4 ? colunas[4].Trim() : string.Empty;
            var uf = colunas.Length > 5 ? colunas[5].Trim() : string.Empty;

            yield return new RawClienteCsvDto(numeroLinha, nome, email, dataNascimento, telefone, cidade, uf);
        }
    }
}