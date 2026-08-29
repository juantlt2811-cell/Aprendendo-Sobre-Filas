using MassTransit;
using Microsoft.Extensions.Logging;
using ProjetoFilaDeJobs.Contracts.Events;

namespace ProjetoFilaDeJobs.Worker.Consumers;

// Segundo Consumer para o MESMO evento PedidoCriado. Ele não sabe (e não
// precisa saber) que o PedidoCriadoConsumer (pagamento) existe - o
// MassTransit cria uma fila própria para cada Consumer, vinculada ao
// mesmo exchange "fanout". Cada fila recebe sua PRÓPRIA cópia da mensagem,
// com seu próprio controle de retry e confirmação (ack) independentes.
public class EstoqueConsumer : IConsumer<PedidoCriado>
{
    private readonly ILogger<EstoqueConsumer> _logger;

    public EstoqueConsumer(ILogger<EstoqueConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<PedidoCriado> context)
    {
        var pedido = context.Message;

        // Sem simulação de falha aqui de propósito - o objetivo deste
        // Consumer é demonstrar que múltiplos workers podem reagir ao
        // mesmo evento em paralelo, cada um no seu próprio ritmo,
        // sem interferir no processamento do outro.
        _logger.LogInformation(
            "Reservando estoque para o pedido {PedidoId} | Cliente: {ClienteNome}",
            pedido.PedidoId, pedido.ClienteNome);

        return Task.CompletedTask;
    }
}