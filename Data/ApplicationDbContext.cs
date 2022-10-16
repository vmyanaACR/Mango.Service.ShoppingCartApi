using Mango.Service.ShoppingCartApi.Models;
using Microsoft.EntityFrameworkCore;

namespace Mango.Service.ShoppingCartApi.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<CartHeader> CartHeaders { get; set; }
    public DbSet<CartDetails> CartDetails { get; set; }
}