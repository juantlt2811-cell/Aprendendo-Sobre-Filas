using Microsoft.EntityFrameworkCore;
using ProjetoFilaDeJobs.Models;

namespace ProjetoFilaDeJobs.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<ItemPedido> ItensPedido => Set<ItemPedido>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Pedido>()
            .HasMany(p => p.Itens)
            .WithOne(i => i.Pedido)
            .HasForeignKey(i => i.PedidoId);

        modelBuilder.Entity<Pedido>()
            .Property(p => p.Status)
            .HasConversion<string>(); // salva o enum como texto no banco, não como número
    }
}