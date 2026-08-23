namespace Wooba.Etl.Console.Domain.ValueObjects;

public class RawClienteCsvDto
{
    public int Linha { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DataNascimento { get; set; } = string.Empty;
}