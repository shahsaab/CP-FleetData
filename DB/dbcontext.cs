using Microsoft.EntityFrameworkCore;

namespace CP_FleetDataJob
{
    public class FleetDbContext : DbContext
    {
        public DbSet<FMSRecords> HistoryPersonalVehicleExpensesSAPData { get; set; }
        public DbSet<Log> Logs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json").Build();

            string ConnectionString = configuration.GetSection("ConnectionStrings").GetValue<string>("default");

            optionsBuilder.UseSqlServer(ConnectionString);
        }

    }

}
