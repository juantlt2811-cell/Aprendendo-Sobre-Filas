using Microsoft.EntityFrameworkCore;
using ProjetoFilaDeJobs.Worker.Models;

namespace ProjetoFilaDeJobs.Worker.Data;

// Repare: este DbContext só conhece ProcessamentoPedido - ele não sabe
// nada sobre Pedido ou ItemPedido (isso é responsabilidade da API).
// Cada processo é dono só dos dados que realmente precisa gerenciar.
public class WorkerDbContext : DbContext
{
    public WorkerDbContext(DbContextOptions<WorkerDbContext> options) : base(options) { }

    public DbSet<ProcessamentoPedido> ProcessamentosPedido => Set<ProcessamentoPedido>();
}