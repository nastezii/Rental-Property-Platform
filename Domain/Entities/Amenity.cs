using Domain.Entities.Base;

namespace Domain.Entities;

public class Amenity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public List<PropertyAmenity> PropertyAmenities { get; set; } = [];
}