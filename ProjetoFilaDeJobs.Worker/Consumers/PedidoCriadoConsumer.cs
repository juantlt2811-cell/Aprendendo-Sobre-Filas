using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProjetoFilaDeJobs.Contracts.Events;
using ProjetoFilaDeJobs.Worker.Data;
using ProjetoFilaDeJobs.Worker.Models;

namespace ProjetoFilaDeJobs.Worker.Consumers;

public class PedidoCriadoConsumer : IConsumer<PedidoCriado>
{
    private readonly ILogger<PedidoCriadoConsumer> _logger;
    private readonly WorkerDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint; // NOVO

    // Random estático simples, só para simular falhas de forma proposital
    // (representa, por exemplo, uma API externa de pagamento instável).
    private static readonly Random _random = new();

    public PedidoCriadoConsumer(ILogger<PedidoCriadoConsumer> logger, WorkerDbContext context, IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<PedidoCriado> context)
    {
        var pedido = context.Message;

        // === IDEMPOTÊNCIA ===
        // Antes de processar, verificamos se esse PedidoId já foi processado
        // com sucesso antes. Se já foi, apenas ignoramos - reprocessar geraria
        // efeito duplicado (ex: cobrar o cliente duas vezes).
        var jaProcessado = await _context.ProcessamentosPedido
            .AnyAsync(p => p.PedidoId == pedido.PedidoId);

        if (jaProcessado)
        {
            _logger.LogWarning(
                "Pedido {PedidoId} já havia sido processado anteriormente. Ignorando (idempotência).",
                pedido.PedidoId);
            return; // encerra sem erro - mensagem é confirmada (ack) normalmente
        }

        _logger.LogInformation(
            "Processando pagamento do pedido {PedidoId} | Cliente: {ClienteNome} | Total: {ValorTotal}",
            pedido.PedidoId, pedido.ClienteNome, pedido.ValorTotal);

        // === FALHA SIMULADA ===
        // 40% de chance de "falhar", simulando uma instabilidade real
        // (ex: timeout de um gateway de pagamento externo).
        // Isso é só para fins didáticos - em código real, aqui entraria a
        // chamada de verdade para o serviço de pagamento.
        if (_random.NextDouble() < 0.4)
        {
            _logger.LogError(
                "Falha simulada ao processar pagamento do pedido {PedidoId}. MassTransit deve tentar novamente.",
                pedido.PedidoId);

            // Lançar a exceção é o que aciona o UseMessageRetry configurado
            // no Program.cs. Se todas as tentativas de retry falharem, o
            // MassTransit move a mensagem para a fila de erro automaticamente
            // (dead-letter queue), sem precisarmos escrever esse código.
            throw new InvalidOperationException(
                $"Falha simulada ao processar pagamento do pedido {pedido.PedidoId}");
        }

        // Se chegou até aqui, o processamento teve sucesso - registramos
        // isso para garantir a idempotência em futuras tentativas de reenvio.
        _context.ProcessamentosPedido.Add(new ProcessamentoPedido
        {
            Id = Guid.NewGuid(),
            PedidoId = pedido.PedidoId,
            ProcessadoEm = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        await _publishEndpoint.Publish(new PagamentoProcessado(
            pedido.PedidoId,
            DateTime.UtcNow));

        _logger.LogInformation("Pedido {PedidoId} processado com sucesso.", pedido.PedidoId);
    }
}