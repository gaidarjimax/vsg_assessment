using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace lib;

public static class Utils
{
    public static async Task<decimal> Calculate24hAvgPrice(string symbol, CryptoInfoContext context)
    {
        if (!context.Database.CanConnect())
            return -1;

        var freshestData = context.PriceDatas.Where(p => p.Symbol == symbol)
                                            .OrderByDescending(p => p.Timestamp)
                                            .Take(1)
                                            .FirstOrDefault();

        if(freshestData == null)
            return -1;

        if(DateTime.UtcNow - freshestData.Timestamp >= TimeSpan.FromHours(24))
            return freshestData.Price;

        var startDate = DateTime.UtcNow.AddDays(-1);
        var prices = await context.PriceDatas
            .Where(p => p.Symbol == symbol && p.Timestamp >= startDate)
            .ToListAsync();

        if (prices.Count == 0)
            return -1;

        return prices.Average(p => p.Price);
    }

    public static async Task<List<decimal>> CalculateSimpleMovingAverage(string symbol, int dataPointsCount, string timeInterval, DateTime intervalStart, CryptoInfoContext context)
    {
        if (!context.Database.CanConnect())
            return [];

        if (!context.PriceDatas.Any())
            return [];

        var intervals = new List<decimal>();
        var timeSpan = timeInterval switch
        {
            "1w" => TimeSpan.FromDays(7),
            "1d" => TimeSpan.FromDays(1),
            "30m" => TimeSpan.FromMinutes(30),
            "5m" => TimeSpan.FromMinutes(5),
            "1m" => TimeSpan.FromMinutes(1),
            _ => TimeSpan.FromDays(1),
        };

        var intervalEnd = intervalStart.Subtract(timeSpan * dataPointsCount);
        var data = await context.PriceDatas
                .Where(p => p.Symbol == symbol && p.Timestamp <= intervalStart && p.Timestamp > intervalEnd)
                .ToListAsync();

        if(data.Count == 0)
            return [];

        for (int i = 0; i < dataPointsCount; ++i)
        {
            intervalEnd = intervalStart.Subtract(timeSpan);
            var spanPrices = data.Where(p => p.Timestamp <= intervalStart && p.Timestamp > intervalEnd).ToList();
            intervalStart = intervalEnd;

            decimal spanPrice = 0;
            if(spanPrices != null && spanPrices.Count != 0)
                spanPrice = spanPrices.Average(p => p.Price);

            intervals.Add(spanPrice);
        }

        return intervals;
    }
}
