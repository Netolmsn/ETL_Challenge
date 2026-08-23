using Wooba.Etl.Console.Domain.ValueObjects;

namespace Wooba.Etl.Console.Domain.Interfaces;

public interface IClienteReader
{
    IAsyncEnumerable<RawClienteCsvDto> LerClientesAsync(string caminhoArquivo, CancellationToken cancellationToken = default);
}