using System.Runtime.CompilerServices;
using Wooba.Etl.Console.Domain.Interfaces;
using Wooba.Etl.Console.Domain.ValueObjects;

namespace Wooba.Etl.Console.Infrastructure.Csv;

public class CsvReader : IClienteReader
{
    public async IAsyncEnumerable<RawClienteCsvDto> LerClientesAsync(
        string caminhoArquivo, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(caminhoArquivo);
        var numeroLinha = 0;

        await reader.ReadLineAsync(cancellationToken); // Pula cabeçalho

        string? linha;
        while ((linha = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            numeroLinha++;

            if (string.IsNullOrWhiteSpace(linha)) continue;

            var colunas = linha.Split(',');

            if (colunas.Length < 3) continue;

        yield return new RawClienteCsvDto(
            Linha: numeroLinha,
            Nome: colunas[0].Trim(),
            Email: colunas[1].Trim(),
            DataNascimento: colunas[2].Trim(),
            Telefone: colunas.Length > 3 ? colunas[3].Trim() : string.Empty,
            Cidade: colunas.Length > 4 ? colunas[4].Trim() : string.Empty,
            Uf: colunas.Length > 5 ? colunas[5].Trim() : string.Empty
        );
        }
    }
}