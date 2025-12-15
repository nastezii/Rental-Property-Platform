using Application.BaseService;
using Domain.Dtos.Analytics;
using Domain.Entities;
using Infrastructure.Repositories.BaseRepository;
using Microsoft.EntityFrameworkCore;

namespace Application.BookingService;

public class BookingService(IBaseRepository<Booking> baseRepository) : BaseService<Booking>(baseRepository), IBookingService
{
    public async Task<List<AnalyticsResponseDto>> GetAnalyticsAsync()
    {
        var bookings = GetAll()
            .Include(b => b.Tenant)
            .Include(b => b.Listing)
            .ThenInclude(l => l.Property);

        return await bookings
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
            .ToListAsync();
    }
}