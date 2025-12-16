using System.Net.Http.Json;
using Application.BaseService;
using Application.ListingService;
using Application.PropertyService;
using AutoMapper;
using Domain;
using Domain.Dtos.CreateNewUserListingDto;
using Domain.Dtos.General;
using Domain.Entities;
using FluentAssertions;
using Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Tests;

[Collection("Test collection")]
public class FailureScenarioTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private const int NonExistentId = int.MaxValue;
    private readonly HttpClient _client = factory.CreateClient();

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync()
    {
        const string sql = @"
            TRUNCATE TABLE 
                users,
                properties,
                listings,
                property_amenities 
            RESTART IDENTITY CASCADE;
        ";

        await ExecuteSqlAsync(sql);
    }

    [Fact]
    public async Task CreateListing_WithInvalidDto_ShouldReturn400()
    {
        // Arrange
        var invalidDto = new 
        {
            Title = "A",
            Description = "short",
            Location = "",
            PropertyType = "InvalidEnum"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api", invalidDto);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
    }
    
    [Fact]
    public async Task CreateListing_WithException_ShouldThrowException_AndNotCreateEntity()
    {
        // Arrange
        var service = GetService<IListingService>();
        var mapper = GetService<IMapper>();
        var hasher = GetService<PasswordHasher<User>>();
        var addedListing = new Listing();

        // Act
        var act = async () =>
        {
            var user = mapper.Map<User>(CreateNewUserListingDto.User);
            var hash = hasher.HashPassword(user, CreateNewUserListingDto.User.Password);
            user.PasswordHash = hash;
        
            var property = mapper.Map<Property>(CreateNewUserListingDto.Property);
            var listing = mapper.Map<Listing>(CreateNewUserListingDto.Listing);
        
            var amenities = CreateNewUserListingDto.Property.Amenities?
                .Select(id => new PropertyAmenity { AmenityId = id })
                .ToList() ?? [];
            
            addedListing = await service.CreateNewUserListingWithExceptionAsync(user, property, amenities, listing);
        };
        
        // Assert
        await act
            .Should()
            .ThrowAsync<Exception>()
            .WithMessage("Error while adding new listing");

        var response = async () => await service.GetByIdAsync(addedListing.Id);
        
        await response
            .Should()
            .ThrowAsync<Exception>()
            .WithMessage("Listing not found");
    }

    [Fact]
    public async Task UpdateProperty_WithInvalidId_ShouldReturn400()
    {
        // Arrange
        var updateDto = new 
        {
            Title = "Valid Title",
            Description = "Valid Description",
            Location = "Valid Location",
            PropertyType = "Apartments"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/properties/{NonExistentId}", updateDto);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
    }
    
    [Fact]
    public async Task UpdateProperty_WithException_ShouldThrowException_AndNotUpdateEntity()
    {
        // Arrange
        var service = GetService<IPropertyService>();
        var mapper = GetService<IMapper>();
        var listing = await AddListingAsync();

        var updateProperty = CreateNewUserListingDto.Property;
        updateProperty.Title = "Test update title";
        updateProperty.Description = "Test update description";

        // Act
        var act = async () =>
        {
            var amenities = CreateNewUserListingDto.Property.Amenities?
                .Select(x => new PropertyAmenity { AmenityId = x })
                .ToList() ?? [];
        
            var property = mapper.Map<Property>(updateProperty);
            await service.UpdatePropertyWithExceptionAsync(listing.Property.Id, property, amenities);
        };
        
        // Assert
        await act
            .Should()
            .ThrowAsync<Exception>()
            .WithMessage("Error while updating property");

        var response = await service.GetByIdAsync(listing.Property.Id);
        response.Title.Should().NotBe(updateProperty.Title);
        response.Title.Should().NotBe(updateProperty.Description);
    }

    [Fact]
    public async Task DeleteProperty_WithInvalidId_ShouldReturn400()
    {
        // Act
        var response = await _client.DeleteAsync($"/api/{NonExistentId}");

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
    }
    
    [Fact]
    public async Task DeleteProperty_WithException_ShouldThrowException_AndNotDeleteEntity()
    {
        // Arrange
        var service = GetService<IPropertyService>();
        var listing = await AddListingAsync();

        // Act
        var act = async () =>
        {
            await service.BeginTransactionAsync();
            
            try
            {
                await service.DeleteAsync(listing.Property.Id);
                throw new Exception("Error while deleting property");
            }
            catch (Exception ex)
            {
                await service.RollbackTransactionAsync(ex);
            }
        };
        
        // Assert
        await act
            .Should()
            .ThrowAsync<Exception>()
            .WithMessage("Error while deleting property");
        
        var response = await service.GetByIdAsync(listing.Property.Id);
        response.Should().NotBeNull();
        response.Id.Should().Be(listing.Property.Id);
    }
    
    private T GetService<T>() where T : notnull
    {
        var scope = factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<T>();
    }
    
    private async Task ExecuteSqlAsync(string sql, params object[] parameters)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.ExecuteSqlRawAsync(sql, parameters);
    }
    
    private async Task<ListingResponseDto> AddListingAsync(int? index = null)
    {
        var response = await _client.PostAsJsonAsync("/api", CreateNewUserListingDto);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ListingResponseDto>();
        
        result.Should().NotBeNull();
        return result;
    }

    private static readonly CreateNewUserListingDto CreateNewUserListingDto = new CreateNewUserListingDto
    {
        User = new UserRequestDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            Password = "password"
        },
        Property = new PropertyRequestDto
        {
            Title = "Test Property",
            Description = "Test Description",
            Location = "Test Location",
            PropertyType = PropertyType.Apartments,
            Amenities = [1, 2]
        },
        Listing = new ListingRequestDto
        {
            Price = 20,
            Currency = "USD",
            MinRentalPeriodInMonths = 3
        }
    };
}
