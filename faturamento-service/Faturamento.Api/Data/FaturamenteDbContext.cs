using Microsoft.EntityFrameworkCore;
using Faturamento.Api.Models;

namespace Faturamento.Api.Data;

public class FaturamentoDbContext : DbContext
{
    public FaturamentoDbContext(DbContextOptions<FaturamentoDbContext> options)
        : base(options) { }

    public DbSet<NotaFiscal> NotasFiscais { get; set; }
    public DbSet<ItemNotaFiscal> ItensNotaFiscal { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotaFiscal>()
            .HasMany(n => n.Itens)
            .WithOne(i => i.NotaFiscal)
            .HasForeignKey(i => i.NotaFiscalId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<NotaFiscal>()
            .Property(n => n.Status)
            .HasConversion<string>();
    }
}