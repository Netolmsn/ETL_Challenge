namespace Wooba.Etl.Console.Domain.ValueObjects;

public record RawClienteCsvDto(int Linha, string Nome, string Email, string DataNascimento);