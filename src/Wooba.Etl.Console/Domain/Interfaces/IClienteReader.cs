namespace Wooba.Etl.Domain.Interfaces;

public interface IClienteReader
{
    IAsyncEnumerable<RawClienteCsvDto> LerClientesAsync(string caminhoArquivo, CancellationToken cancellationToken = default);
}

public record RawClienteCsvDto(int Linha, string Nome, string Email, string DataNascimento);