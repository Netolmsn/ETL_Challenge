namespace Wooba.Etl.Domain.Entities;
public class Cliente
{
    public int Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public DateTime DataNascimento { get; private set; }
    public bool Revisando { get; private set; }

    private Cliente() { }

    public Cliente(int id = 0, string nome, string email, DateTime dataNascimento)
    {
        Id = id;
        SetNome(nome);
        SetEmail(email);
        DataNascimento = dataNascimento;
        Revisando = false;
    }

    public void SetNome(string nome)
    {
        var nomeTratado = nome?.Trim();
        if (string.IsNullOrWhiteSpace(nomeTratado))
        {
            throw new ArgumentException("O nome do Cliente não pode ser vazio.");
        }
        Nome = nomeTratado;
    }

    public void SetEmail(string email)
    {
        var emailTratado = email?.Trim();
        if (string.IsNullOrWhiteSpace(emailTratado) || !ValidarEmail(emailTratado))
        {
            throw new ArgumentException($"O email é inválido: {email}.");
        }
        Email = emailTratado;
    }

    public void MarcarComoRevisando()
    {
        Revisando = true;
    }

    private static bool ValidarEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;

        var partes = email.Split('@');
        return partes.Length == 2 && !string.IsNullOrWhiteSpace(partes[0]) && !string.IsNullOrWhiteSpace(partes[1]);
    }
}