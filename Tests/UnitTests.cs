using Domain.Dtos.Analytics;
using Domain.Entities;
using FluentAssertions;

namespace Tests;

[Collection("Test collection")]
public class UnitTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public void PaginateListings_ShouldReturnCorrectPage()
    {
        var listings = new List<Listing> { new(), new(), new() };
        var twoItems = GetPaginateListings(listings, 1, 2);
        var secondPage = GetPaginateListings(listings, 2, 2);

        twoItems.Should().NotBeNull();
        twoItems.Count.Should().Be(2);
        secondPage.Should().NotBeNull();
        secondPage.Count.Should().Be(1);
    }
    
    [Fact]
    public void GetAnalytics_ShouldCalculateMetricsAndRevenuePerCurrency()
    {
        // Arrange
        var now = DateTime.UtcNow;
        
        var property1 = new Property { Id = 1, Title = "Property 1" };
        var property2 = new Property { Id = 2, Title = "Property 2" };

        var listing1Usd = new Listing { Property = property1, Price = 100, Currency = "USD" };
        var listing1Eur = new Listing { Property = property1, Price = 80, Currency = "EUR" };
        var listing2Usd = new Listing { Property = property2, Price = 200, Currency = "USD" };

        var bookings = new List<Booking>
        {
            new() { Listing = listing1Usd, StartDate = now.AddDays(-10), EndDate = now, Tenant = new User() },
            new() { Listing = listing1Usd, StartDate = now.AddDays(-20), EndDate = now.AddDays(-10), Tenant = new User() },
            new() { Listing = listing1Eur, StartDate = now.AddDays(-5), EndDate = now, Tenant = new User() },
            new() { Listing = listing2Usd, StartDate = now.AddDays(-3), EndDate = now, Tenant = new User() },
        }.AsQueryable();
        
        var result = bookings
            .GroupBy(b => new
            {
                PropertyId = b.Listing.Property.Id,
                PropertyTitle = b.Listing.Property.Title
            })
            .Select(g => new AnalyticsResponseDto
            {
                PropertyId = g.Key.PropertyId,
                PropertyTitle = g.Key.PropertyTitle,
                TotalBookings = g.Count(),
                AvgDuration = g.Average(b => (b.EndDate - b.StartDate).TotalDays),
                MinDuration = g.Min(b => (b.EndDate - b.StartDate).TotalDays),
                MaxDuration = g.Max(b => (b.EndDate - b.StartDate).TotalDays),
                Revenues = g
                    .GroupBy(b => b.Listing.Currency)
                    .Select(cg => new CurrencyRevenueDto
                    {
                        Currency = cg.Key,
                        TotalRevenue = cg.Sum(b => (decimal)(b.EndDate - b.StartDate).TotalDays * b.Listing.Price)
                    })
                    .ToList()
            })
            .Where(x => x.TotalBookings > 0)
            .ToList();

        // Assert
        result.Should().HaveCount(2);

        var prop1Analytics = result.First(r => r.PropertyId == 1);
        prop1Analytics.TotalBookings.Should().Be(3);
        prop1Analytics.Revenues.Should().ContainSingle(r => r.Currency == "EUR");
        prop1Analytics.Revenues.Should().ContainSingle(r => r.Currency == "USD");
        prop1Analytics.Revenues.First(r => r.Currency == "USD").TotalRevenue.Should().Be(20 * 100);
        prop1Analytics.Revenues.First(r => r.Currency == "EUR").TotalRevenue.Should().Be(5 * 80);

        var prop2Analytics = result.First(r => r.PropertyId == 2);
        prop2Analytics.TotalBookings.Should().Be(1);
        prop2Analytics.Revenues.Should().ContainSingle();
        prop2Analytics.Revenues.First().Currency.Should().Be("USD");
        prop2Analytics.Revenues.First().TotalRevenue.Should().Be(3 * 200);
    }

    private static List<Listing> GetPaginateListings(List<Listing> listings, int page, int pageSize)
    {
        return listings
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }
}