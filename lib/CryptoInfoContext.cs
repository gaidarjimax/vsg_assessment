using Microsoft.EntityFrameworkCore;

public class CryptoInfoContext : DbContext
{
    public DbSet<PriceData> PriceDatas { get; set; }

    public CryptoInfoContext(DbContextOptions<CryptoInfoContext> options) : base(options)
    {
    }
}

public class PriceData
{
    public int Id { get; set; }
    public string Symbol { get; set; }
    public decimal Price { get; set; }
    public DateTime Timestamp { get; set; }
}
