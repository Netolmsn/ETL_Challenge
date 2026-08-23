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
        var emailsProcessados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var linha in linhasCruas)
        {
            // Remover espaços 
            var nomeTratado = linha.Nome?.Trim();
            var emailTratado = linha.Email?.Trim();
            var dataTratada = linha.DataNascimento?.Trim();
            var telefoneTratado = linha.Telefone?.Trim() ?? string.Empty;
            var cidadeTratada = linha.Cidade?.Trim() ?? string.Empty;
            var ufTratada = linha.Uf?.Trim() ?? string.Empty;

            // 1. Descartar linhas com nome vazio
            if (string.IsNullOrWhiteSpace(nomeTratado))
            {
                resultado.LogsDescarte.Add(new ClienteDescartadoLog(linha.Linha, $"{linha.Nome};{linha.Email}", "Nome do cliente está vazio."));
                continue;
            }

            // 2. Descartar e-mails inválidos
            if (string.IsNullOrWhiteSpace(emailTratado) || !ValidarFormatoEmail(emailTratado))
            {
                resultado.LogsDescarte.Add(new ClienteDescartadoLog(linha.Linha, $"{linha.Nome};{linha.Email}", $"E-mail em formato inválido: '{emailTratado}'."));
                continue;
            }

            // 3. Não gravar o cliente duas vezes
            if (emailsProcessados.Contains(emailTratado))
            {
                resultado.LogsDescarte.Add(new ClienteDescartadoLog(linha.Linha, $"{linha.Nome};{linha.Email}", $"Cliente duplicado por e-mail: '{emailTratado}'."));
                continue;
            }

            // 4. Aceitar diferentes formatos de data
            if (!DateTime.TryParseExact(dataTratada, FormatosDataSuportados, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dataNascimento) &&
                !DateTime.TryParse(dataTratada, out dataNascimento))
            {
                resultado.LogsDescarte.Add(new ClienteDescartadoLog(linha.Linha, $"{linha.Nome};{linha.Email}", $"Data de nascimento inválida: '{dataTratada}'."));
                continue;
            }

            // 5. Instancia o cliente após TODAS as validações passarem
            try
            {
                var cliente = new Cliente(
                    nome: nomeTratado, 
                    email: emailTratado, 
                    dataNascimento: dataNascimento, 
                    telefone: telefoneTratado, 
                    cidade: cidadeTratada, 
                    uf: ufTratada
                );

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
        if (string.IsNullOrWhiteSpace(email) || email.Contains(' ')) 
            return false;

        var partes = email.Split('@');
        return partes.Length == 2 && !string.IsNullOrWhiteSpace(partes[0]) && !string.IsNullOrWhiteSpace(partes[1]);
    }
}