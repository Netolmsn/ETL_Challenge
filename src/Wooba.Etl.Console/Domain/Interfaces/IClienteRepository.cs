namespace Wooba.Etl.Domain.Interfaces;

using Wooba.Etl.Domain.Entities;

public interface IClienteRepository
{
    Task CriarTabelaSeNaoExistirAsync();
    Task InserirMassaAsync(IEnumerable<Cliente> clientes);
    Task<IEnumerable<Cliente>> ObterTodosAsync();
    Task<bool> AtualizarStatusRevisadosAsync(int id,  bool revisado);
    Task<bool> ExcluirPorIdAsync(int id);
    Task<ResumoExecucaoVo> ObterResumoExecucaoAsync(int totalLidos, int totalDescartados);
}

public record ResumoExecucaoVo(int TotalLido, int TotalInserido, int TotalDescartado);