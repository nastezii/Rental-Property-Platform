using Domain.Entities;
using Infrastructure.Repositories.BaseRepository;

namespace Infrastructure.Repositories.BookingRepository;

public interface IBookingRepository : IBaseRepository<Booking>;