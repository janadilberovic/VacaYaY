using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using VacaYAY.Data;

namespace VacaYAY.Api;

/// <summary>
/// Lets EF Core tooling (migrations / database update) build the DbContext at design time
/// without running the full web host in <c>Program.cs</c> — so the JWT signing-key guard and
/// runtime service wiring don't interfere. The connection string is read from
/// appsettings + user-secrets + environment variables, the same sources the app uses.
/// </summary>
public class VacaYAYDbContextFactory : IDesignTimeDbContextFactory<VacaYAYDbContext>
{
    public VacaYAYDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddUserSecrets<VacaYAYDbContextFactory>()
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var options = new DbContextOptionsBuilder<VacaYAYDbContext>()
            .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
            .Options;

        return new VacaYAYDbContext(options);
    }
}
