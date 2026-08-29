using ProjetoFilaDeJobs.DTOs;

namespace ProjetoFilaDeJobs.Services;

// Interface do Service - o Controller depende dela, não da implementação
// concreta (PedidoService). Isso segue o mesmo princípio de Dependency
// Inversion que você já usava no hackathon com IUsuarioRepository.
//
// Vantagem prática: permite trocar a implementação (ex: para testes com
// um mock) sem tocar no Controller.
public interface IPedidoService
{
    Task<PedidoResponse> CriarPedidoAsync(CriarPedidoRequest request);
    Task<PedidoResponse?> ObterPorIdAsync(Guid id);
}