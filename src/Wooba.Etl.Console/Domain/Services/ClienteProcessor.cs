using System.Globalization;
using Wooba.Etl.Console.Domain.Entities;
using Wooba.Etl.Console.Domain.Interfaces;
using Wooba.Etl.Console.Domain.ValueObjects;
namespace Wooba.Etl.Console.Domain.Services;

public class ClienteProcessor : IClienteProcessor
{
    private static readonly string[] FormatosDataSuportados = new[]
    {
        "dd/MM/yyyy",
        "MM/dd/yyyy",
        "yyyy-MM-dd",
        "dd-MM-yyyy",
        "MM-dd-yyyy"
    };

    public ProcessamentoResultado ProcessarLinhas(IEnumerable<RawClienteCsvDto> linhasCruas)
    {
        var resultado = new ProcessamentoResultado();
        var emailsProcessados = new HashSet<String>(StringComparer.OrdinalIgnoreCase);

        foreach (var linhaCrua in linhasCruas)
        {
            // Remover espacos 
            var nomeTratado = linha.Nome?.Trim();
            var emailTratado = linha.Email?.Trim();
            var dataTratada = linha.DataNascimento?.Trim();

            //Descartas linhas com nome vazio
            if (string.IsNullOrWhiteSpace(nomeTratado))
            {
                resultado.LogsDescarte.Add(new ClienteDescartadoLog(linha.Linha, $"{linha.Nome};{linha.Email}", "Nome do cliente está vazio."));
                continue;
            }

            //Descartas emails inválidos
            if (string.IsNullOrWhiteSpace(emailTratado) || !ValidarFormatoEmail(emailTratado))
            {
                resultado.LogsDescarte.Add(new ClienteDescartadoLog(linha.Linha, $"{linha.Nome};{linha.Email}", $"E-mail em formato inválido: '{emailTratado}'."));
                continue;
            }

            //Não gravar o cliente duas vezes
            if (emailsProcessados.Contains(emailTratado))
            {
                resultado.LogsDescarte.Add(new ClienteDescartadoLog(linha.Linha, $"{linha.Nome};{linha.Email}", $"Cliente duplicado por e-mail: '{emailTratado}'."));
                continue;
            }

            //Aceitar diferentes formatos de data
            if (!DateTime.TryParseExact(dataTratada, FormatosDataSuportados, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dataNascimento) &&
                !DateTime.TryParse(dataTratada, out dataNascimento))
            {
                resultado.LogsDescarte.Add(new ClienteDescartadoLog(linha.Linha, $"{linha.Nome};{linha.Email}", $"Data de nascimento inválida: '{dataTratada}'."));
                continue;
            }

            try
            {
                var cliente = new Cliente(nomeTratado, emailTratado, dataNascimento);
                resultado.ClientesValidos.Add(cliente);
                emailsProcessados.Add(emailTratado);
            }
            catch (Exception ex)
            {
                resultado.LogsDescarte.Add(new ClienteDescartadoLog(linha.Linha, $"{linha.Nome};{linha.Email}", $"Erro de validação de domínio: {ex.Message}"));
            }
        }

        return resultado;

    }
    private static bool ValidarFormatoEmail(string email)
    {
        var partes = email.Split('@');
        return partes.Length == 2 && !string.IsNullOrWhiteSpace(partes[0]) && !string.IsNullOrWhiteSpace(partes[1]);
    }
}
