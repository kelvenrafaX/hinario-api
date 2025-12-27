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
    }
}
