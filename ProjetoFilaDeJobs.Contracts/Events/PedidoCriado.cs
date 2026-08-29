namespace ProjetoFilaDeJobs.Contracts.Events;

public record PedidoCriado(
    Guid PedidoId,
    string ClienteNome,
    string ClienteEmail,
    decimal ValorTotal,
    DateTime CriadoEm
);