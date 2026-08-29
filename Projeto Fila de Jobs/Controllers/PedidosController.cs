using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoFilaDeJobs.Data;
using ProjetoFilaDeJobs.DTOs;
using ProjetoFilaDeJobs.Models;

namespace ProjetoFilaDeJobs.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly AppDbContext _context;

    public PedidosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CriarPedido([FromBody] CriarPedidoRequest request)
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

        pedido.ValorTotal = pedido.Itens.Sum(i => i.Quantidade * i.PrecoUnitario);

        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync();

        // Mapeamento manual: entidade (Pedido) -> DTO de resposta (PedidoResponse).
        // É esse mapeamento que "quebra" o ciclo, porque o objeto que de fato
        // vai para o serializador JSON não tem mais a referência de volta ItemPedido -> Pedido.
        var response = MapParaResponse(pedido);

        return CreatedAtAction(nameof(ObterPorId), new { id = pedido.Id }, response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.Itens)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido is null)
            return NotFound();

        return Ok(MapParaResponse(pedido));
    }

    // Método privado auxiliar para não repetir esse mapeamento em cada endpoint.
    // Em projetos maiores, isso costuma virar uma classe separada (ex: PedidoMapper)
    // ou usar uma biblioteca como AutoMapper - mas manual, explícito, já resolve bem aqui.
    private static PedidoResponse MapParaResponse(Pedido pedido)
    {
        return new PedidoResponse(
            pedido.Id,
            pedido.ClienteNome,
            pedido.ClienteEmail,
            pedido.Status.ToString(), // enum vira string na resposta, mais legível pro consumidor da API
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