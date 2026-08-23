using Microsoft.Extensions.DependencyInjection;
using Wooba.Etl.Console.Domain.Interfaces;
using Wooba.Etl.Console.Domain.Services;
using Wooba.Etl.Console.Infrastructure.Csv;
using Wooba.Etl.Console.Infrastructure.Database;
using Wooba.Etl.Console.Domain.ValueObjects;

var services = new ServiceCollection();

services.AddSingleton<SqliteDbContext>();
services.AddTransient<IClienteReader, CsvReader>();
services.AddTransient<IClienteProcessor, ClienteProcessor>();
services.AddTransient<IClienteRepository, ClienteRepository>();

var serviceProvider = services.BuildServiceProvider();

var dbContext = serviceProvider.GetRequiredService<SqliteDbContext>();
var reader = serviceProvider.GetRequiredService<IClienteReader>();
var processor = serviceProvider.GetRequiredService<IClienteProcessor>();
var repository = serviceProvider.GetRequiredService<IClienteRepository>();

await repository.CriarTabelaSeNaoExistirAsync();

Console.Clear();
Console.WriteLine("==================================================================");
Console.WriteLine("                WOOBA ETL - PROCESSADOR DE CLIENTES                ");
Console.WriteLine("==================================================================\n");

string caminhoCsv = Path.Combine(Directory.GetCurrentDirectory(), "clientes_lote_b.csv");

if (!File.Exists(caminhoCsv))
{
    caminhoCsv = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "clientes_lote_b.csv");
}

int totalLidos = 0;
var linhasCruas = new List<RawClienteCsvDto>();

if (File.Exists(caminhoCsv))
{
    Console.WriteLine($"Lendo o arquivo: {Path.GetFileName(caminhoCsv)}...");
    
    await foreach (var linha in reader.LerClientesAsync(caminhoCsv))
    {
        linhasCruas.Add(linha);
        totalLidos++;
    }

    Console.WriteLine($"Processando e validando {totalLidos} registros encontrados.");
    var resultado = processor.ProcessarLinhas(linhasCruas);

    if (resultado.LogsDescarte.Any())
    {
        Console.WriteLine("\n------------------------------------------------------------------");
        Console.WriteLine("REGISTRO DE INCIDENTES / DESCHARTES DE LINHAS (LOG)");
        Console.WriteLine("------------------------------------------------------------------");
        foreach (var log in resultado.LogsDescarte)
        {
            Console.WriteLine($"• [Linha {log.LinhaCsv}] {log.MotivoDescarte} (Conteúdo: \"{log.ConteudoLinha}\")");
        }
        Console.WriteLine("------------------------------------------------------------------\n");
    }

    Console.WriteLine("Persistindo registros válidos no banco SQLite...");
    await repository.InserirMassaAsync(resultado.ClientesValidos);

    var resumo = await repository.ObterResumoExecucaoAsync(totalLidos, resultado.LogsDescarte.Count);
    Console.WriteLine("\n==================================================================");
    Console.WriteLine("                       RESUMO DA EXECUÇÃO                         ");
    Console.WriteLine("==================================================================");
    Console.WriteLine($"  Total de registros lidos:     {resumo.TotalLido}");
    Console.WriteLine($"  Registros válidos gravados:  {resumo.TotalInserido}");
    Console.WriteLine($"  Registros descartados:       {resumo.TotalDescartado}");
    Console.WriteLine("==================================================================\n");
}
else
{
    Console.WriteLine($"Arquivo CSV não encontrado no caminho especificado.");
    Console.WriteLine($"O sistema continuará com a base de dados vazia para consultas.");
}

bool rodando = true;
while (rodando)
{
    Console.WriteLine("\nPainel de Gerenciamento do Banco de Dados");
    Console.WriteLine("1. Listar todos os clientes cadastrados");
    Console.WriteLine("2. Marcar um cliente como 'Revisado' (UPDATE)");
    Console.WriteLine("3. Remover um cliente por ID (DELETE)");
    Console.WriteLine("0. Encarnar / Sair da aplicação");
    Console.Write("\nDigite o número da opção desejada: ");

    var escolha = Console.ReadLine()?.Trim();

    switch (escolha)
    {
        case "1":
            var clientes = await repository.ObterTodosAsync();
            Console.WriteLine("---------------------------------------------------------------------------------------------------------");
            Console.WriteLine("ID | Nome                | E-mail                        | Telefone       | Data Nasc. | Revisado | Cidade/UF");
            Console.WriteLine("---------------------------------------------------------------------------------------------------------");
            
            if (!clientes.Any())
            {
                Console.WriteLine("Nenhum cliente disponível no banco de dados.");
            }
            else
            {
                foreach (var cliente in clientes)
                {
                    var statusRevisado = cliente.Revisado ? "SIM" : "NÃO";
                    Console.WriteLine($"{cliente.Id,-2} | {cliente.Nome,-19} | {cliente.Email,-29} | {cliente.Telefone,-14} | {cliente.DataNascimento:dd/MM/yyyy} | {statusRevisado,-8} | {cliente.Cidade}/{cliente.Uf}");
                }
            }
            Console.WriteLine("------------------------------------------------------------------------------------");
            break;

        case "2":
            Console.Write("\nDigite o ID do cliente que deseja marcar como revisado: ");
            if (int.TryParse(Console.ReadLine(), out int idUpdate))
            {
                var atualizado = await repository.AtualizarStatusRevisadosAsync(idUpdate, true);
                if (atualizado)
                    Console.WriteLine($"Cliente ID #{idUpdate} atualizado para 'Revisado' com sucesso!");
                else
                    Console.WriteLine($"Não foi possível localizar nenhum cliente com o ID #{idUpdate}.");
            }
            else
            {
                Console.WriteLine("Digite um número de ID válido.");
            }
            break;

        case "3":
            Console.Write("\nDigite o ID do cliente que deseja remover do banco: ");
            if (int.TryParse(Console.ReadLine(), out int idDelete))
            {
                var removido = await repository.ExcluirPorIdAsync(idDelete);
                if (removido)
                    Console.WriteLine($"Cliente ID #{idDelete} foi excluído do SQLite.");
                else
                    Console.WriteLine($"Não foi possível localizar nenhum cliente com o ID #{idDelete}.");
            }
            else
            {
                Console.WriteLine("Digite um número de ID válido.");
            }
            break;

        case "0":
            Console.WriteLine("\nEncerrando a aplicação. Conexão do SQLite em memória fechada.");
            rodando = false;
            break;

        default:
            Console.WriteLine("\nOpção não reconhecida. Tente novamente.");
            break;
    }
}

dbContext.Dispose();