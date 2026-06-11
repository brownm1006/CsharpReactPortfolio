namespace PortfolioClubAssurance.Api.Entities;

public sealed class QuoteEntity
{
    public Guid Id { get; set; }

    public string ProductType { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? CurrentStep { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public ICollection<QuoteVehicleEntity> Vehicles { get; set; } = [];

    public ICollection<QuoteStepSubmissionEntity> StepSubmissions { get; set; } = [];
}
