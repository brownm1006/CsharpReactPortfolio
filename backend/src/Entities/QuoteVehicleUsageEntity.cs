namespace PortfolioClubAssurance.Api.Entities;

public sealed class QuoteVehicleUsageEntity
{
    public Guid QuoteVehicleId { get; set; }

    public int UsedOutsideQuebecStatusId { get; set; }

    public int CurrentOdometerKm { get; set; }

    public int AnnualDistanceKm { get; set; }

    public bool DriveForBusiness { get; set; }

    public QuoteVehicleEntity? QuoteVehicle { get; set; }
}
