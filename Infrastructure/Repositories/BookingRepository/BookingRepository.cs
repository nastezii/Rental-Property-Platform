using Domain.Entities;
using Infrastructure.Repositories.BaseRepository;

namespace Infrastructure.Repositories.BookingRepository;

public class BookingRepository(ApplicationDbContext context) : BaseRepository<Booking>(context), IBookingRepository;