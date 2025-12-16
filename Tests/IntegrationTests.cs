using System.Net.Http.Json;
using Domain;
using Domain.Dtos.Analytics;
using Domain.Dtos.CreateNewUserListingDto;
using Domain.Dtos.General;
using Domain.Entities;
using FluentAssertions;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Tests;

[Collection("Test collection")]
public class IntegrationTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
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
    public async Task CreateUserWithProperty_ShouldReturnOk()
    {
        // Act
        var response = await AddListingAsync();

        // Assert
        response.Should().NotBeNull();
        response.Property.Should().NotBeNull();
    }
    
    [Fact]
    public async Task UpdateProperty_ShouldReturnOk()
    {
        // Arrange
        var response = await AddListingAsync();
        
        var updateDto = new PropertyRequestDto
        {
            Title = "Update Title",
            Description = "Update Description",
            Location = "Update Location",
            PropertyType = PropertyType.Apartments,
            Amenities = [1]
        };

        // Act
        var updateResponse = await _client.PatchAsync($"/api/{response.Property.Id}", JsonContent.Create(updateDto));
        var updateResult = await updateResponse.Content.ReadFromJsonAsync<PropertyResponseDto>();

        // Assert
        updateResponse.EnsureSuccessStatusCode();
        updateResult.Should().NotBeNull();

        updateResult.Title.Should().Be(updateDto.Title);
        updateResult.Description.Should().Be(updateDto.Description);
        updateResult.Location.Should().Be(updateDto.Location);
        updateResult.PropertyType.Should().Be(updateDto.PropertyType);
        updateResult.Amenities.Select(x => x.Id).Should().BeEquivalentTo(updateDto.Amenities);
    }
    
    [Fact]
    public async Task DeleteProperty_ShouldReturnOk()
    {
        // Arrange
        var added = await AddListingAsync();
        
        // Act
        var response = await _client.DeleteAsync($"/api/{added.Property.Id}");
        var updateResult = await response.Content.ReadFromJsonAsync<Property>();

        // Assert
        response.EnsureSuccessStatusCode();
        updateResult.Should().NotBeNull();
        updateResult.Title.Should().Be(added.Property.Title);
        updateResult.Description.Should().Be(added.Property.Description);
        updateResult.Location.Should().Be(added.Property.Location);
        updateResult.PropertyType.Should().Be(added.Property.PropertyType);
    }
    
    [Fact]
    public async Task SelectTopRateProperties_ShouldReturnOk()
    {
        // Arrange
        var firstAdded = await AddListingAsync();
        var secondAdded = await AddListingAsync(2);
        
        const string sql = @"
            INSERT INTO reviews (reviewer_id, property_id, rating, comment, date_added)
            VALUES 
                ({0}, {1}, {2}, {3}, NOW()),
                ({4}, {5}, {6}, {7}, NOW()),
                ({8}, {9}, {10}, {11}, NOW());
        ";

        await ExecuteSqlAsync(
            sql,
            1, firstAdded.Property.Id, 5, "Great property",
            1, firstAdded.Property.Id, 4, "Not bad!!!",
            2, secondAdded.Property.Id, 4, "Nice stay!"
        );
        
        // Act
        var response = await _client.GetAsync($"/api/properties");
        var result = await response.Content.ReadFromJsonAsync<List<PropertyResponseDto>>();

        // Assert
        response.EnsureSuccessStatusCode();
        result.Should().NotBeNull();
        result.Count.Should().Be(2);

        var first = result.FirstOrDefault();
        var second = result.LastOrDefault();
        first.Should().NotBeNull();
        second.Should().NotBeNull();
        
        first.Title.Should().Be(firstAdded.Property.Title);
        first.Description.Should().Be(firstAdded.Property.Description);
        first.Location.Should().Be(firstAdded.Property.Location);
        first.PropertyType.Should().Be(firstAdded.Property.PropertyType);
        
        second.Title.Should().Be(secondAdded.Property.Title);
        second.Description.Should().Be(secondAdded.Property.Description);
        second.Location.Should().Be(secondAdded.Property.Location);
        second.PropertyType.Should().Be(secondAdded.Property.PropertyType);
    }
    
    [Fact]
    public async Task SelectListings_ShouldReturnOk()
    {
        // Arrange
        var firstAdded = await AddListingAsync();
        var secondAdded = await AddListingAsync(2);

        const int page = 1;
        const int pageSize = 2;
        
        // Act
        var multiResponse = await _client.GetAsync($"/api/listings?page={page}&pageSize={pageSize}");
        var multiResult = await multiResponse.Content.ReadFromJsonAsync<List<ListingResponseDto>>();
        
        var singleResponse = await _client.GetAsync($"/api/listings?page={page}&pageSize={pageSize - 1}");
        var singleResult = await singleResponse.Content.ReadFromJsonAsync<List<ListingResponseDto>>();

        // Assert
        multiResponse.EnsureSuccessStatusCode();
        multiResult.Should().NotBeNull();
        multiResult.Count.Should().Be(pageSize);
        
        singleResponse.EnsureSuccessStatusCode();
        singleResult.Should().NotBeNull();
        singleResult.Count.Should().Be(pageSize - 1);

        var first = multiResult.FirstOrDefault();
        var second = multiResult.LastOrDefault();
        var single = singleResult.FirstOrDefault();
        first.Should().NotBeNull();
        second.Should().NotBeNull();
        single.Should().NotBeNull();
        
        first.Property.Title.Should().Be(firstAdded.Property.Title);
        first.Property.Description.Should().Be(firstAdded.Property.Description);
        first.Property.Location.Should().Be(firstAdded.Property.Location);
        first.Property.PropertyType.Should().Be(firstAdded.Property.PropertyType);
        
        second.Property.Title.Should().Be(secondAdded.Property.Title);
        second.Property.Description.Should().Be(secondAdded.Property.Description);
        second.Property.Location.Should().Be(secondAdded.Property.Location);
        second.Property.PropertyType.Should().Be(secondAdded.Property.PropertyType);
        
        single.Property.Title.Should().Be(firstAdded.Property.Title);
        single.Property.Description.Should().Be(firstAdded.Property.Description);
        single.Property.Location.Should().Be(firstAdded.Property.Location);
        single.Property.PropertyType.Should().Be(firstAdded.Property.PropertyType);
    }
    
    [Fact]
    public async Task GetAnalytics_ShouldReturnOk()
    {
        // Arrange
        var firstAdded = await AddListingAsync();
        var secondAdded = await AddListingAsync(2);

        var now = DateTime.UtcNow;

        const string bookings = @"
            INSERT INTO bookings (tenant_id, listing_id, start_date, end_date, date_added)
            VALUES 
                ({0}, {1}, {2}, {3}, NOW()),
                ({4}, {5}, {6}, {7}, NOW()),
                ({8}, {9}, {10}, {11}, NOW());
        ";
        
        const string listing= @"
            INSERT INTO listings (property_id, price, currency, min_rental_period_in_months, status, date_added)
            VALUES ({0}, {1}, {2}, {3}, {4}, NOW())
        ";
        
        await ExecuteSqlAsync(listing, firstAdded.Property.Id, 15, "EUR", 3, "Inactive", now);
        
        await ExecuteSqlAsync(
            bookings,
            1, firstAdded.Id, now, now.AddMonths(6),
            1, 3, now.AddMonths(-3), now,
            2, secondAdded.Id, now, now.AddMonths(4)
        );

        // Act
        var response = await _client.GetAsync("/api/analytics");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<AnalyticsResponseDto>>();
        
        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(2);
        
        var d1 = (now.AddMonths(6) - now).TotalDays;
        var d2 = (now - now.AddMonths(-3)).TotalDays;
        var d3 = (now.AddMonths(4) - now).TotalDays;

        var expected = new List<AnalyticsResponseDto>
        {
            new()
            {
                PropertyId = firstAdded.Property.Id,
                PropertyTitle = firstAdded.Property.Title,
                TotalBookings = 2,
                AvgDuration = (d1 + d2) / 2,
                MinDuration = Math.Min(d1, d2),
                MaxDuration = Math.Max(d1, d2),
                Revenues =
                [
                    new CurrencyRevenueDto { Currency = "USD", TotalRevenue = (decimal)d1 * firstAdded.Price },
                    new CurrencyRevenueDto { Currency = "EUR", TotalRevenue = (decimal)d2 * 15m }
                ]
            },
            new()
            {
                PropertyId = secondAdded.Property.Id,
                PropertyTitle = secondAdded.Property.Title,
                TotalBookings = 1,
                AvgDuration = d3,
                MinDuration = d3,
                MaxDuration = d3,
                Revenues = [new CurrencyRevenueDto { Currency = secondAdded.Currency, TotalRevenue = (decimal)d3 * secondAdded.Price }]
            }
        }
        .OrderByDescending(x => x.Revenues.Sum(r => r.TotalRevenue))
        .ToList();
        
        foreach (var actual in result)
        {
            var expectedProperty = expected.First(e => e.PropertyId == actual.PropertyId);

            actual.TotalBookings.Should().Be(expectedProperty.TotalBookings);
            actual.AvgDuration.Should().BeApproximately(expectedProperty.AvgDuration, 0.01);
            actual.MinDuration.Should().BeApproximately(expectedProperty.MinDuration, 0.01);
            actual.MaxDuration.Should().BeApproximately(expectedProperty.MaxDuration, 0.01);
            
            foreach (var rev in actual.Revenues)
            {
                var expRev = expectedProperty.Revenues.First(er => er.Currency == rev.Currency);
                rev.TotalRevenue.Should().BeApproximately(expRev.TotalRevenue, 0.01m);
            }
        }
    }

    private async Task<ListingResponseDto> AddListingAsync(int? index = null)
    {
        var dto = new CreateNewUserListingDto
        {
            User = new UserRequestDto
            {
                FirstName = "Test",
                LastName = "User",
                Email = index != null ? $"test{index}@test.com" : "test@test.com",
                Password = "password"
            },
            Property = new PropertyRequestDto
            {
                Title = index != null ? $"Test Property {index}" : "Test Property",
                Description = "Test Description",
                Location = "Test Location",
                PropertyType = PropertyType.Apartments,
                Amenities = [1, 2]
            },
            Listing = new ListingRequestDto
            {
                Price = index != null ? 20 * (decimal)index : 20,
                Currency = "USD",
                MinRentalPeriodInMonths = 3
            }
        };

        var response = await _client.PostAsJsonAsync("/api", dto);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ListingResponseDto>();
        
        result.Should().NotBeNull();
        return result;
    }

    private async Task ExecuteSqlAsync(string sql, params object[] parameters)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.ExecuteSqlRawAsync(sql, parameters);
    }
}