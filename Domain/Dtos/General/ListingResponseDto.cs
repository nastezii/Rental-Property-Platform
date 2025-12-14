using System.Text.Json.Serialization;

namespace Domain.Dtos.General;

public class ListingResponseDto : BaseResponseDto
{
    public int PropertyId  { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = string.Empty;
    public int MinRentalPeriodInMonths { get; set; } 
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ListingStatus Status { get; set; }

    public PropertyResponseDto Property { get; set; } = null!;
}