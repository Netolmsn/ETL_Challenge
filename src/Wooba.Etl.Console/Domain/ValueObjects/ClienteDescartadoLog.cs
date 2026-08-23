namespace Wooba.Etl.Console.Domain.ValueObjects;

public record ClienteDescartadoLog(
    int LinhaCsv, 
    string ConteudoLinha, 
    string MotivoDescarte
);