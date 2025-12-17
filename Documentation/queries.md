### Запит 1: Статистика бронювань по нерухомості з доходами

**Бізнес-питання:**
Яка статистика бронювань для кожної нерухомості? Скільки днів в середньому орендують? Який загальний дохід у різних валютах?

**ORM запит**

```
public async Task<List<AnalyticsResponseDto>> GetAnalyticsAsync()
{
    var bookings = GetAll()
        .Include(b => b.Tenant)
        .Include(b => b.Listing)
        .ThenInclude(l => l.Property);

    return await bookings
        .GroupBy(b => new
        {
            PropertyId = b.Listing.Property.Id,
            PropertyTitle = b.Listing.Property.Title
        })
        .Select(g => new AnalyticsResponseDto
        {
            PropertyId = g.Key.PropertyId,
            PropertyTitle = g.Key.PropertyTitle,
            TotalBookings = g.Count(),
            AvgDuration = g.Average(b => (b.EndDate - b.StartDate).TotalDays),
            MinDuration = g.Min(b => (b.EndDate - b.StartDate).TotalDays),
            MaxDuration = g.Max(b => (b.EndDate - b.StartDate).TotalDays),
            Revenues = g
                .GroupBy(b => b.Listing.Currency)
                .Select(cg => new CurrencyRevenueDto
                {
                    Currency = cg.Key,
                    TotalRevenue = cg.Sum(b => (decimal)(b.EndDate - b.StartDate).TotalDays * b.Listing.Price)
                })
                .ToList()
        })
        .Where(x => x.TotalBookings > 0)
        .ToListAsync();
}
```

**SQL запит:**

```sql
WITH booking_stats AS (
    SELECT
        p.id as property_id,
        p.title as property_title,
        b.id as booking_id,
        b.start_date,
        b.end_date,
        l.price,
        l.currency,
        EXTRACT(DAY FROM (b.end_date - b.start_date)) as duration_days
    FROM bookings b
    JOIN listings l ON b.listing_id = l.id
    JOIN properties p ON l.property_id = p.id
),
property_aggregates AS (
    SELECT
        property_id,
        property_title,
        COUNT(booking_id) as total_bookings,
        AVG(duration_days) as avg_duration,
        MIN(duration_days) as min_duration,
        MAX(duration_days) as max_duration
    FROM booking_stats
    GROUP BY property_id, property_title
),
currency_revenues AS (
    SELECT
        property_id,
        currency,
        SUM(duration_days * price) as total_revenue
    FROM booking_stats
    GROUP BY property_id, currency
)
SELECT
    pa.property_id,
    pa.property_title,
    pa.total_bookings,
    pa.avg_duration,
    pa.min_duration,
    pa.max_duration,
    json_agg(
        json_build_object(
            'currency', cr.currency,
            'total_revenue', cr.total_revenue
        )
    ) as revenues
FROM property_aggregates pa
LEFT JOIN currency_revenues cr ON pa.property_id = cr.property_id
WHERE pa.total_bookings > 0
GROUP BY
    pa.property_id,
    pa.property_title,
    pa.total_bookings,
    pa.avg_duration,
    pa.min_duration,
    pa.max_duration
ORDER BY pa.total_bookings DESC;
```

**Пояснення:**

- **CTE `booking_stats`**: Збирає базову інформацію про всі бронювання з розрахунком тривалості в днях
  - JOIN таблиць `bookings`, `listings` та `properties` для отримання повної інформації
  - Розрахунок тривалості оренди через `EXTRACT(DAY FROM (end_date - start_date))`
- **CTE `property_aggregates`**: Агрегує статистику по кожній нерухомості

  - Підрахунок загальної кількості бронювань (`COUNT`)
  - Розрахунок середньої тривалості (`AVG`)
  - Визначення мінімальної та максимальної тривалості (`MIN`, `MAX`)
  - Групування за `property_id` та `property_title`

- **CTE `currency_revenues`**: Розраховує дохід по кожній валюті для кожної нерухомості

  - Формула доходу: `duration_days * price`
  - Групування за `property_id` та `currency` для підтримки мультивалютності

- **Фінальний SELECT**: Об'єднує статистику та доходи
  - `json_agg()` та `json_build_object()` для створення JSON масиву з доходами по валютах
  - LEFT JOIN для включення всіх нерухомостей, навіть якщо немає доходів в певній валюті
  - Фільтрація `WHERE total_bookings > 0` для виключення нерухомості без бронювань
  - Сортування за кількістю бронювань (найпопулярніші зверху)

**Приклад виводу:**

| property_id | property_title           | total_bookings | avg_duration | min_duration | max_duration | revenues                                                                                    |
| ----------- | ------------------------ | -------------- | ------------ | ------------ | ------------ | ------------------------------------------------------------------------------------------- |
| 1           | Cozy 2-Bedroom Apartment | 15             | 180.5        | 90           | 365          | [{"currency":"UAH","total_revenue":2707500.00}, {"currency":"USD","total_revenue":5400.00}] |
| 5           | Luxury Studio in Center  | 12             | 150.2        | 60           | 270          | [{"currency":"UAH","total_revenue":1803000.00}]                                             |
| 3           | Spacious House           | 8              | 210.8        | 180          | 365          | [{"currency":"EUR","total_revenue":12600.00}]                                               |
