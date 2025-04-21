namespace SavingsOnDapr.MCP.Console.Responses;

public record CurrencyExchangeSummaryResponse
{
    public string ResponseKey { get; init; } = string.Empty;

    public string[] ColumnNames { get; init; } = [];

    public ICollection<CurrencyExchangeSummaryValueEntry> Entries { get; init; } = [];
}

public record CurrencyExchangeSummaryValueEntry
{
    public string EntryName { get; init; } = string.Empty;
    public ICollection<string> ColumnValues { get; init; } = [];
}
