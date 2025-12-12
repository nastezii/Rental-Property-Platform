using Domain.Entities.Base;

namespace Domain.Entities;

public class Review : TrackingEntity
{
    public int ReviewerId { get; set; }
    public int PropertyId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;

    public User Reviewer { get; set; } = null!; 
    public Property Property { get; set; } = null!;
}