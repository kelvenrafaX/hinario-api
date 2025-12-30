using Microsoft.EntityFrameworkCore;
using MinhaPrimeiraApi.Models;

namespace MinhaPrimeiraApi.Context
{
    public class HinarioApiContext : DbContext
    {
        public HinarioApiContext(DbContextOptions<HinarioApiContext> options)
            : base(options)
        {
        }

        public DbSet<Hino> Hinos { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Hino>()
                .HasIndex(h => h.Identificador)
                .IsUnique()
                .HasFilter("\"identificador\" IS NOT NULL");
        }
    }
}
