namespace Domain.Dtos.Analytics;

public class CurrencyRevenueDto
{
    public string Currency { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
}