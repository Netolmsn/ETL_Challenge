using System.Runtime.CompilerServices;
using Wooba.Etl.Console.Domain.Interfaces;
using Wooba.Etl.Console.Domain.ValueObjects;
namespace Wooba.Etl.Console.Infrastructure.Csv;

public class CsvReader : IClienteReader
{
    public async IAsyncEnumerable<RawClienteCsvDto> LerClientesAsync(string caminhoArquivo, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!File.Exists(caminhoArquivo))
        {
            throw new FileNotFoundException($"Arquivo CSV não encontrado: {caminhoArquivo}");
        }

        using var reader = new StreamReader(caminhoArquivo);
        int numeroLinha = 0;

        while (!reader.EndOfStream)
        {
            var linha = await reader.ReadLineAsync(cancellationToken);
            numeroLinha++;

            if (numeroLinha == 1 && (linha?.Contains("nome", StringComparison.OrdinalIgnoreCase) ?? false))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(linha)) continue;

            var colunas = linha.Split(';');
            if (colunas.Length < 3)
            {
                colunas = linhas.Split(',');
            }

            var nome = colunas.Length > 0 ? colunas [0] : string.Empty;
            var email = colunas.Length > 1 ? colunas[1] : string.Empty;
            var dataNascimento = colunas.Length > 2 ? colunas[2] : string.Empty;

            yield return new RawClienteCsvDto(numeroLinha, nome, email, dataNascimento);
        }
    }
}