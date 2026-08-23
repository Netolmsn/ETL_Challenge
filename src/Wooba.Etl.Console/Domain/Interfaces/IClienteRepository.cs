using Wooba.Etl.Console.Domain.Entities;
using Wooba.Etl.Console.Domain.ValueObjects;

namespace Wooba.Etl.Console.Domain.Interfaces;

public interface IClienteRepository
{
    Task CriarTabelaSeNaoExistirAsync();
    Task InserirMassaAsync(IEnumerable<Cliente> clientes);
    Task<IEnumerable<Cliente>> ObterTodosAsync();
    Task<bool> AtualizarStatusRevisadosAsync(int id, bool revisado);
    Task<bool> ExcluirPorIdAsync(int id);
    Task<ResumoExecucaoVo> ObterResumoExecucaoAsync(int totalLidos, int totalDescartados);
}