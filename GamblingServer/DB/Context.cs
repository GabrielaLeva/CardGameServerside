using Microsoft.EntityFrameworkCore;
namespace GamblingServer.DB
{
    public class GamblingContext:DbContext
    {
        public DbSet<User> user { get; set; }
        //public DbSet<GameArchive> gamearchive { get; set; }
        public GamblingContext(DbContextOptions<GamblingContext> options)
        : base(options)
        {
        }
    }
}
