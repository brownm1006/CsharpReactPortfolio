namespace PortfolioClubAssurance.Api.Entities;

public sealed class QuoteStepSubmissionEntity
{
    public long Id { get; set; }

    public Guid QuoteId { get; set; }

    public string StepCode { get; set; } = string.Empty;

    public string PayloadJson { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public QuoteEntity? Quote { get; set; }
}
