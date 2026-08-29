using MassTransit;
using Microsoft.Extensions.Logging;
using ProjetoFilaDeJobs.Contracts.Events;

namespace ProjetoFilaDeJobs.Worker.Consumers;

// IConsumer<T> é a interface do MassTransit que marca essa classe como
// "alguém que sabe processar mensagens do tipo T". O MassTransit descobre
// essa classe automaticamente (via reflection, na configuração que faremos
// no Program.cs) e cria uma fila vinculada ao exchange de PedidoCriado.
public class PedidoCriadoConsumer : IConsumer<PedidoCriado>
{
    private readonly ILogger<PedidoCriadoConsumer> _logger;

    public PedidoCriadoConsumer(ILogger<PedidoCriadoConsumer> logger)
    {
        _logger = logger;
    }

    // Consume é chamado automaticamente pelo MassTransit toda vez que uma
    // mensagem PedidoCriado chega na fila deste consumer.
    // ConsumeContext<T> dá acesso à mensagem (context.Message) e a metadados
    // (ex: quantas vezes essa mensagem já foi tentada, se for o caso de retry).
    public Task Consume(ConsumeContext<PedidoCriado> context)
    {
        var pedido = context.Message;

        _logger.LogInformation(
            "Pedido recebido para processamento: {PedidoId} | Cliente: {ClienteNome} | Total: {ValorTotal:C}",
            pedido.PedidoId,
            pedido.ClienteNome,
            pedido.ValorTotal
        );

        // Por enquanto só logamos - nos próximos passos, aqui é onde vamos
        // simular processamento de pagamento, estoque, etc.
        return Task.CompletedTask;
    }
}