namespace ProjetoFilaDeJobs.Worker.Models;

// Essa tabela NÃO é sobre o pedido em si (isso já existe no banco da API) -
// é um registro de controle: "este PedidoId já foi processado por este worker".
// É a peça central da idempotência: antes de processar, consultamos aqui.
public class ProcessamentoPedido
{
    public Guid Id { get; set; }
    public Guid PedidoId { get; set; }
    public DateTime ProcessadoEm { get; set; }
}