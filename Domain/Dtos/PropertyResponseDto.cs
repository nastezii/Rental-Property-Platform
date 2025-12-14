using System.Text.Json.Serialization;

namespace Domain.Dtos.General;

public class PropertyResponseDto : BaseResponseDto
{
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PropertyType PropertyType { get; set; }
    public double Rating { get; set; }

    public List<AmenityResponseDto> Amenities { get; set; } = [];
}