using Domain.Entities.Base;

namespace Domain.Entities;

public class User : TrackingEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public List<Property> Properties { get; set; } = [];
    public List<Booking> Bookings { get; set; } = [];
    public List<Review> Reviews { get; set; } = [];
}