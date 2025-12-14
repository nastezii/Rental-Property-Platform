using System.ComponentModel.DataAnnotations;

namespace Domain.Dtos.CreateNewUserListingDto;

public class ListingRequestDto
{
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }
    
    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string Currency { get; set; } = string.Empty;
    
    [Required]
    [Range(1, 5 * 12, ErrorMessage = "MinRentalPeriodInMonths must be 0 < x < 60")]
    public int MinRentalPeriodInMonths { get; set; }
}