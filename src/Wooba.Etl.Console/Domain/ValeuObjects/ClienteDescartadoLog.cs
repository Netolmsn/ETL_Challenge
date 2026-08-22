namespace Wooba.Etl.Domain.ValueObjects;

public record ClienteDescartadoLog(
    int LinhaCsv, 
    string ConteudoLinha, 
    string MotivoDescarte
);