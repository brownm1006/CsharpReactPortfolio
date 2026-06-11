namespace PortfolioClubAssurance.Api.Entities;

public sealed class QuoteVehicleUsageEntity
{
    public Guid Id { get; set; }

    public Guid QuoteVehicleId { get; set; }

    public int UsedOutsideQuebecStatusId { get; set; }

    public int CurrentOdometerKm { get; set; }

    public int AnnualDistanceKm { get; set; }

    public bool DriveForBusiness { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public QuoteVehicleEntity? QuoteVehicle { get; set; }
}
