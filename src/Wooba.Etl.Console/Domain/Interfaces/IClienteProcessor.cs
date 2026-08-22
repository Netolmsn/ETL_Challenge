namespace Wooba.Etl.Domain.Interfaces;

using Wooba.Etl.Domain.Entities;
using Wooba.Etl.Domain.ValueObjects;

public interface IClienteProcessor
{
    ProcessamentoResultado ProcessarLinhas(IEnumerable<RawClienteCsvDto> linhasCruas);
}

public class ProcessamentoResultado
{
    public List<Cliente> ClientesValidos { get; } = new();
    public List<ClienteDescartadoLog> LogsDescarte { get; } = new();
}
