namespace Domain.Dtos.Analytics;

public class AnalyticsResponseDto
{
    public int PropertyId { get; set; }
    public string PropertyTitle { get; set; } = string.Empty;
    public double TotalBookings { get; set; }
    public double AvgDuration { get; set; }
    public double MinDuration { get; set; }
    public double MaxDuration { get; set; }
    
    public List<CurrencyRevenueDto> Revenues { get; set; } = [];
}