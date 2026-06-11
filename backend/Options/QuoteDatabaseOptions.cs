namespace PortfolioClubAssurance.Api.Options;

public sealed class QuoteDatabaseOptions
{
    public const string SectionName = "ConnectionStrings";

    public string QuoteDatabase { get; set; } = string.Empty;
}
