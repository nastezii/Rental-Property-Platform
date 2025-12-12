using Domain.Entities.Base;
using System.Reflection;

namespace Domain.Entities;

public class Booking : TrackingEntity
{
    public int TenantId { get; set; }
    public int ListingId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public User Tenant { get; set; } = null!;
}