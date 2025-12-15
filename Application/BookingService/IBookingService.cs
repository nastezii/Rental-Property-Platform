using Application.BaseService;
using Domain.Dtos.Analytics;
using Domain.Entities;

namespace Application.BookingService;

public interface IBookingService : IBaseService<Booking>
{
    Task<List<AnalyticsResponseDto>> GetAnalyticsAsync();
}