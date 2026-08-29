using Microsoft.AspNetCore.Mvc;
using ProjetoFilaDeJobs.DTOs;
using ProjetoFilaDeJobs.Services;

namespace ProjetoFilaDeJobs.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    // O Controller agora só conhece a INTERFACE do Service - não sabe (e não
    // precisa saber) que por trás existe um AppDbContext e um IPublishEndpoint.
    // Essa é a ideia central de separar em camadas: cada uma só enxerga a
    // camada imediatamente abaixo dela.
    private readonly IPedidoService _pedidoService;

    public PedidosController(IPedidoService pedidoService)
    {
        _pedidoService = pedidoService;
    }

    [HttpPost]
    public async Task<IActionResult> CriarPedido([FromBody] CriarPedidoRequest request)
    {
        // O Controller não sabe COMO um pedido é criado (cálculo de total,
        // persistência, publicação de evento) - ele só delega e traduz
        // o resultado para uma resposta HTTP.
        var response = await _pedidoService.CriarPedidoAsync(request);

        return CreatedAtAction(nameof(ObterPorId), new { id = response.Id }, response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var response = await _pedidoService.ObterPorIdAsync(id);

        // Aqui é onde a "tradução" de null para 404 acontece - papel do
        // Controller, não do Service, como comentado no PedidoService.
        if (response is null)
            return NotFound();

        return Ok(response);
    }
}