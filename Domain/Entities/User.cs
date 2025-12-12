using Domain.Entities.Base;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Domain.Entities;

public class User : TrackingEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public List<Property> Properties { get; set; } = [];
}