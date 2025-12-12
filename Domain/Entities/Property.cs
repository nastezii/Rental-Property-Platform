using Domain.Entities.Base;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Domain.Entities;

public class Property : BaseEntity
{
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PropertyType PropertyType { get; set; }

    public User User { get; set; } = null!;
    public List<Listing> Listings { get; set; } = [];
    public List<PropertyAmenity> PropertyAmenities = [];
    public List<Review> Reviews { get; set; } = [];
}