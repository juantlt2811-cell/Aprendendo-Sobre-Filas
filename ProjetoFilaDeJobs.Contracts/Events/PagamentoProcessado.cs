namespace ProjetoFilaDeJobs.Contracts.Events;

// Evento publicado pelo WORKER, ao concluir o processamento com sucesso.
// A API vai consumir esse evento para atualizar o Status do pedido -
// nenhum dos dois lados escreve diretamente no banco do outro.
public record PagamentoProcessado(
    Guid PedidoId,
    DateTime ProcessadoEm
);