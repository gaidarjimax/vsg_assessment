using System;
using System.Net.Http;
using System.Threading.Tasks;

class StockPriceAnalyzer
{
    private static readonly HttpClient _client = new HttpClient();

    static async Task Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Invalid command. Usage: StockPriceAnalyzer <command> <symbol> [<arguments>] ");
            return;
        }

        var arguments = new Queue<string>(args);
        var command = arguments.Dequeue();
        var symbol = arguments.Dequeue();

        try
        {
            var result = await ProcessCommand(command, symbol, arguments);
            Console.WriteLine(result);
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine($"Invalid arguments for command '{command}': {ex.Message}");
        }
        catch (HttpRequestException ex)
        {
            Console.Error.WriteLine($"Error communicating with API: {ex.Message}");
        }
    }

    private static async Task<string> ProcessCommand(string command, string symbol, Queue<string> arguments)
    {
        switch (command.ToLower())
        {
            case "24h":
                if (arguments.Count > 0)
                    throw new ArgumentException("The '24h' command does not accept any additional arguments.");

                var response24h = await _client.GetStringAsync($"http://localhost:5046/api/{symbol}/24hAvgPrice");
                return response24h;

            case "sma":
                if (arguments.Count < 2 || arguments.Count > 3)
                    throw new ArgumentException("The 'sma' command requires 2 additional arguments: n, p, and one optional s.");

                var dataPointsCount = int.Parse(arguments.Dequeue());
                var period = arguments.Dequeue();

                var url = $"http://localhost:5046/api/{symbol}/SimpleMovingAverage?n={dataPointsCount}&p={period}";

                if (arguments.Count > 0)
                {
                    var startDate = DateTime.Parse(arguments.Dequeue());
                    url = $"{url}&s={startDate}";
                }

                var responseSma = await _client.GetStringAsync(url);
                return responseSma;

            default:
                throw new ArgumentException($"Invalid command: '{command}'.");
        }
    }
}