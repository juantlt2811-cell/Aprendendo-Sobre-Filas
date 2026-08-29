using MassTransit;
using Microsoft.EntityFrameworkCore;
using ProjetoFilaDeJobs.Contracts.Events;
using ProjetoFilaDeJobs.Data;
using ProjetoFilaDeJobs.Models;

namespace ProjetoFilaDeJobs.Consumers;

// A API também é um Consumer, não só um Publisher - ela escuta o evento
// que o Worker publica ao concluir o pagamento, e é ELA quem decide o que
// fazer com essa informação (aqui: atualizar o Status). Isso mantém a
// regra de "só a API mexe na tabela Pedidos" intacta.
public class PagamentoProcessadoConsumer : IConsumer<PagamentoProcessado>
{
    private readonly AppDbContext _context;
    private readonly ILogger<PagamentoProcessadoConsumer> _logger;

    public PagamentoProcessadoConsumer(AppDbContext context, ILogger<PagamentoProcessadoConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PagamentoProcessado> context)
    {
        var evento = context.Message;

        var pedido = await _context.Pedidos.FirstOrDefaultAsync(p => p.Id == evento.PedidoId);

        if (pedido is null)
        {
            // Cenário defensivo: teoricamente não deveria acontecer (o pedido
            // sempre existe, já que foi ele quem originou o evento PedidoCriado),
            // mas é boa prática nunca assumir que o dado referenciado existe.
            _logger.LogWarning(
                "Recebido PagamentoProcessado para um pedido inexistente: {PedidoId}",
                evento.PedidoId);
            return;
        }

        // Change tracking do EF Core em ação de novo: só mudamos a propriedade,
        // o SaveChangesAsync gera o UPDATE automaticamente.
        pedido.Status = StatusPedido.PagamentoAprovado;
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Status do pedido {PedidoId} atualizado para {Status}.",
            pedido.Id, pedido.Status);
    }
}