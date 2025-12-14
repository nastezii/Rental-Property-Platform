using System.ComponentModel.DataAnnotations;

namespace Domain.Dtos.CreateNewUserListingDto;

public class PropertyRequestDto
{
    [Required]
    [StringLength(255, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [MinLength(10)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Location { get; set; } = string.Empty;
    
    [Required]
    [EnumDataType(typeof(PropertyType))]
    public PropertyType PropertyType { get; set; }
    
    public List<int>? Amenities { get; set; }
}