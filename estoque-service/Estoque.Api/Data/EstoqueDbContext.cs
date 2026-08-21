using Microsoft.EntityFrameworkCore;
using Estoque.Api.Models;

namespace Estoque.Api.Data
{
    public class EstoqueDbContext : DbContext
    {
        public EstoqueDbContext(DbContextOptions<EstoqueDbContext> options) : base(options)
        {
        }

        public DbSet<Produto> Produtos { get; set; }
    }
}