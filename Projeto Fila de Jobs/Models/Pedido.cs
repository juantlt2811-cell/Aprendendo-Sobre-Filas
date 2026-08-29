namespace ProjetoFilaDeJobs.Models;

public class Pedido
{
    public Guid Id { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public string ClienteEmail { get; set; } = string.Empty;
    public StatusPedido Status { get; set; } = StatusPedido.Criado;
    public decimal ValorTotal { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public List<ItemPedido> Itens { get; set; } = new();
}

public enum StatusPedido
{
    Criado,
    PagamentoProcessando,
    PagamentoAprovado,
    PagamentoRecusado,
    EstoqueReservado,
    Enviado,
    Cancelado
}