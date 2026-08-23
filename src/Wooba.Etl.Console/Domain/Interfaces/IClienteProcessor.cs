using Wooba.Etl.Console.Domain.Entities;
using Wooba.Etl.Console.Domain.Interfaces;
using Wooba.Etl.Console.Domain.ValueObjects;
namespace Wooba.Etl.Console.Domain.Interfaces;
public interface IClienteProcessor
{
    ProcessamentoResultado ProcessarLinhas(IEnumerable<RawClienteCsvDto> linhasCruas);
}

public class ProcessamentoResultado
{
    public List<Cliente> ClientesValidos { get; } = new();
    public List<ClienteDescartadoLog> LogsDescarte { get; } = new();
}
