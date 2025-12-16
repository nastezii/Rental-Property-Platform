using Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services
                .SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            
            if (descriptor != null)
                services.Remove(descriptor);
            
            services.AddDbContext<ApplicationDbContext>(options => 
                options.UseNpgsql(
                    "Host=host.docker.internal;Port=5432;Database=test;Username=postgres;Password=postgres",
                    x => x.MigrationsAssembly("Infrastructure"))
                    .UseSnakeCaseNamingConvention());

            
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            db.Database.EnsureDeleted();
            db.Database.Migrate();
            db.Database.ExecuteSqlRaw(@"
                INSERT INTO amenities (name, description)
                VALUES 
                    ('Test1', 'TestTest1'),
                    ('Test2', 'TestTest2')
                ON CONFLICT (name) DO NOTHING;
            ");
        });
    }
}