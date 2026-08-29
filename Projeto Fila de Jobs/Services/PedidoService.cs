using MassTransit;
using Microsoft.EntityFrameworkCore;
using ProjetoFilaDeJobs.Data;
using ProjetoFilaDeJobs.DTOs;
using ProjetoFilaDeJobs.Contracts.Events;
using ProjetoFilaDeJobs.Models;

namespace ProjetoFilaDeJobs.Services;

// Aqui mora a REGRA DE NEGÓCIO: como criar um pedido, como calcular o total,
// e quando publicar o evento de integração. O Controller não sabe nada disso -
// ele só chama esses métodos e devolve o resultado como resposta HTTP.
public class PedidoService : IPedidoService
{
    private readonly AppDbContext _context;

    // IPublishEndpoint (MassTransit) migrou do Controller para cá - faz mais
    // sentido a decisão de "publicar um evento" morar junto com a regra de
    // negócio que decide QUANDO isso deve acontecer, não na camada HTTP.
    private readonly IPublishEndpoint _publishEndpoint;

    public PedidoService(AppDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<PedidoResponse> CriarPedidoAsync(CriarPedidoRequest request)
    {
        var pedido = new Pedido
        {
            Id = Guid.NewGuid(),
            ClienteNome = request.ClienteNome,
            ClienteEmail = request.ClienteEmail,
            Status = StatusPedido.Criado,
            CriadoEm = DateTime.UtcNow,
            Itens = request.Itens.Select(i => new ItemPedido
            {
                Id = Guid.NewGuid(),
                ProdutoNome = i.ProdutoNome,
                Quantidade = i.Quantidade,
                PrecoUnitario = i.PrecoUnitario
            }).ToList()
        };

        // Regra de negócio: o total é sempre calculado no servidor,
        // nunca confiar em valor vindo do cliente.
        pedido.ValorTotal = pedido.Itens.Sum(i => i.Quantidade * i.PrecoUnitario);

        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync();

        // Publica o evento só depois de confirmar que o pedido foi
        // persistido com sucesso - se o SaveChangesAsync lançar uma
        // exceção, essa linha nunca é executada.
        await _publishEndpoint.Publish(new PedidoCriado(
            pedido.Id,
            pedido.ClienteNome,
            pedido.ClienteEmail,
            pedido.ValorTotal,
            pedido.CriadoEm
        ));

        return MapParaResponse(pedido);
    }

    public async Task<PedidoResponse?> ObterPorIdAsync(Guid id)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.Itens)
            .FirstOrDefaultAsync(p => p.Id == id);

        // Retorna null em vez de lançar exceção ou já devolver um NotFound()
        // aqui - decidir o status code HTTP é responsabilidade do Controller,
        // não do Service. O Service não deveria saber o que é um "404".
        return pedido is null ? null : MapParaResponse(pedido);
    }

    private static PedidoResponse MapParaResponse(Pedido pedido)
    {
        return new PedidoResponse(
            pedido.Id,
            pedido.ClienteNome,
            pedido.ClienteEmail,
            pedido.Status.ToString(),
            pedido.ValorTotal,
            pedido.CriadoEm,
            pedido.Itens.Select(i => new ItemPedidoResponse(
                i.Id,
                i.ProdutoNome,
                i.Quantidade,
                i.PrecoUnitario
            )).ToList()
        );
    }
}