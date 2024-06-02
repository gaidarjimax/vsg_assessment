namespace wsclient;

using Binance.Net;
using Binance.Net.Clients;
using Binance.Net.Interfaces.Clients;
using Binance.Net.Objects;
using CryptoExchange.Net;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Sockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IBinanceSocketClient _socketClient;
    private readonly IServiceProvider _serviceProvider;
    

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;        

        _socketClient = new BinanceSocketClient(options => { 
            options.ApiCredentials = new ApiCredentials("ArjOtsIV0xjn7zekPOJPL69Fg68LKsqDYOMpitNn9nLCCwqDUg5ZGZwS8h2AZ06b", "BnsjDJZTLcG8jNZ5WAenIuRt3sZJ3ISFUvbaddmSaRWTU0Agmq1Od5I17mIGh4hj"); 
        });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

        using var scope = _serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var context = services.GetService<CryptoInfoContext>();
        if(context == null)
            return;

        await context.Database.EnsureCreatedAsync(stoppingToken);

        var symbolStreams = new List<string> { "BTCUSDT", "ADAUSDT", "ETHUSDT" };
        var tickerSubscriptionResult = await _socketClient.SpotApi.ExchangeData.SubscribeToTickerUpdatesAsync(symbolStreams, (data) => 
        {
            var priceData = new PriceData
            {
                Symbol = data.Data.Symbol,
                Price = data.Data.LastPrice,
                Timestamp = data.Timestamp,
            };

            context.PriceDatas.Add(priceData);
            context.SaveChanges();
        });

        if (!tickerSubscriptionResult.Success)
        {
            _logger.LogError($"Failed to subscribe: {tickerSubscriptionResult.Error}");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}


