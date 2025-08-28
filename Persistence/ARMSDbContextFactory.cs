using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Persistence
{
    public class ARMSDbContextFactory : IDesignTimeDbContextFactory<ARMSDbContext>
    {
        public ARMSDbContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "API")) // Point to the API project directory
                .AddJsonFile("appsettings.json")
                .Build();

            var builder = new DbContextOptionsBuilder<ARMSDbContext>();
            var connectionString = configuration.GetConnectionString("staggingConnectionString"); // Using staggingConnectionString

            builder.UseSqlServer(connectionString)
                   .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));

            return new ARMSDbContext(builder.Options);
        }
    }
}
