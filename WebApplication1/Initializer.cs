using Application.BookingService;
using Application.ListingService;
using Application.PropertyAmenityService;
using Application.PropertyService;
using Application.UserService;
using Domain.Entities;
using Infrastructure;
using Infrastructure.Repositories.AmenityRepository;
using Infrastructure.Repositories.BaseRepository;
using Infrastructure.Repositories.BookingRepository;
using Infrastructure.Repositories.ListingRepository;
using Infrastructure.Repositories.PropertyAmenityRepository;
using Infrastructure.Repositories.PropertyRepository;
using Infrastructure.Repositories.ReviewRepository;
using Infrastructure.Repositories.UserRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1;

public static class Initializer
{
    public static void SetupDatabase(this WebApplicationBuilder builder)
    {
        var connection = builder.Configuration.GetConnectionString("DefaultConnection");
        if (connection == null)
            throw new KeyNotFoundException("Connection not set");

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connection).UseSnakeCaseNamingConvention());
        
        builder.Services.AddAutoMapper(typeof(Program));
    }
    
    public static async Task MigrateAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
    }
    
    public static void InitializeRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPropertyRepository, PropertyRepository>();
        services.AddScoped<IAmenityRepository, AmenityRepository>();
        services.AddScoped<IPropertyAmenityRepository, PropertyAmenityRepository>();
        services.AddScoped<IListingRepository, ListingRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
    }
    
    public static void InitializeServices(this IServiceCollection services)
    {
        services.AddSingleton<PasswordHasher<User>>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPropertyService, PropertyService>();
        services.AddScoped<IPropertyAmenityService, PropertyAmenityService>();
        services.AddScoped<IListingService, ListingService>();
        services.AddScoped<IBookingService, BookingService>();
    }
}