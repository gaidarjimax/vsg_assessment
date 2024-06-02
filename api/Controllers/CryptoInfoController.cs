
using lib;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace api.Controllers
{
    [Route("/api/{symbol}")]
    [ApiController]
    public class CryptoInfoController : ControllerBase
    {
        private readonly CryptoInfoContext _context;
        private readonly IMemoryCache _cache;
        public CryptoInfoController(IMemoryCache cache, CryptoInfoContext context)
        {
            _context = context;
            _cache = cache;
        }

        [HttpGet("24hAvgPrice")]
        public async Task<ActionResult<object>> Get24hAvgPrice(string symbol)
        {
            var avgPrice = await Utils.Calculate24hAvgPrice(symbol, _context);
            if (avgPrice == -1)
                return NotFound("No data available");

            return Ok(new { Symbol = symbol, Price = avgPrice});
        }

        [HttpGet("SimpleMovingAverage")]
        public async Task<ActionResult<object>> GetSimpleMovingAverage(string symbol, int n, string p, DateTime? s)
        {
            var startDate = s ?? DateTime.UtcNow;
            string cacheKey = $"SMA_{symbol}_{n}_{p}_{startDate:yyyyMMdd}";
            List<decimal>? sma;

            if(_cache.TryGetValue(cacheKey, out sma))
                return Ok(sma);
            
            sma = await Utils.CalculateSimpleMovingAverage(symbol, n, p, startDate.Date, _context);
            
            if(sma.Count == 0)
                return NotFound("No data available");

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                .SetAbsoluteExpiration(TimeSpan.FromHours(6));

            _cache.Set(cacheKey, sma, cacheEntryOptions);
            return Ok(sma);
        }
    }
}