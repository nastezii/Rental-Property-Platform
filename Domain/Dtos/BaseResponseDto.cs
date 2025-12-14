using System.Text.Json.Serialization;

namespace Domain.Dtos;

public class BaseResponseDto
{
    [JsonPropertyOrder(-1)]
    public int Id { get; set; }
}