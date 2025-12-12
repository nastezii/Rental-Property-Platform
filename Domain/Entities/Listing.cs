using Domain.Entities.Base;

namespace Domain.Entities;

public class Listing : TrackingEntity
{
    public int PropertyId { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = string.Empty;
    public int MinRentalPeriodInMonths { get; set; }
    public ListingStatus Status { get; set; }

    public Property Property { get; set; } = null!;
    public List<Booking> Bookings { get; set; } = [];
}