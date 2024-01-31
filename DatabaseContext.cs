using Microsoft.EntityFrameworkCore;

namespace ParsingTask1
{
    public class DatabaseContext : DbContext
    {
       public  DbSet<Subscriber> Subscribers { get; set; } = null!;
        public DatabaseContext()
        {

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server = MYBOOK_ZENITH\\SQLEXPRESS; Database = ParsingTask; Trusted_Connection = True; TrustServerCertificate = Yes;");
        }
    }
}
