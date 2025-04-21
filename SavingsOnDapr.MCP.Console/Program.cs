using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using SavingsOnDapr.MCP.Console.Responses;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

var builder = Host.CreateEmptyApplicationBuilder(settings: null);

// Create the MCP Server with Standard I/O Transport and Tools from the current assembly
builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

var app = builder.Build();

await app.RunAsync();

[McpServerToolType]
public static class EchoTool
{
    [McpServerTool, Description("Echoes the message back to the client.")]
    public static string Echo(string message) => $"hello {message}";
}

[McpServerToolType]
public static class CurrencyExchangeTools
{
    private const int ApiPort = 5170;

    [McpServerTool, Description("Retrieves statistics about Currency Exchange transactions in the SavingsOnDapr system.")]
    public static async Task<string> GetCurrencyExchangeSummary(
            HttpClient httpClient,
            [Description("The source currency")] string sourceCurrency,
            [Description("The target currency")] string targetCurrency,
            [Description("The start date of summary (yyyy-MM-dd format)")] string startDate,
            [Description("The end date of summary, optional (yyyy-MM-dd format)")] string? endDate)
    {
        var queryString = "";

        if (endDate is not null)
        {
            queryString = $"?toDate={endDate}";
        }

        var requestUri = new Uri($"http://localhost:{ApiPort}/api/currency-exchange-summary/{sourceCurrency}/{targetCurrency}/{startDate}{queryString}");
        var res = await httpClient.PostAsync(
                requestUri,
                null);

        var resultLines = new List<string>();
        if (res.IsSuccessStatusCode)
        {
            await Task.Delay(101);

            var queryResult = await httpClient.GetFromJsonAsync<CurrencyExchangeSummaryResponse>(requestUri);

            if (queryResult is not null)
            {
                resultLines.Add("---");
                var dateIndex = Array.IndexOf(queryResult.ColumnNames, "Date");
                var totalCount = Array.IndexOf(queryResult.ColumnNames, "TotalExchangesCount");
                var totalSourceAmount = Array.IndexOf(queryResult.ColumnNames, "TotalSourceAmount");

                foreach (var item in queryResult.Entries)
                {
                    var values = item.ColumnValues.ToArray();
                    resultLines.Add($"Summary date: {values[dateIndex]}");
                    resultLines.Add($"Total exchanges count: {values[totalCount]}");
                    resultLines.Add($"Total exchanges source amount: {values[totalSourceAmount]}");
                    resultLines.Add("---");
                }
            }
        }

        return resultLines.Count > 0 ?
            string.Join("\n", resultLines) 
            : "No data available";
    }
}