namespace PortfolioClubAssurance.Api.Entities;

public sealed class QuoteVehicleEntity
{
    public Guid Id { get; set; }

    public Guid QuoteId { get; set; }

    public int ModelYear { get; set; }

    public int ManufacturerId { get; set; }

    public int VehicleModelId { get; set; }

    public string? VehicleCode { get; set; }

    public int PurchaseYear { get; set; }

    public int PurchaseMonth { get; set; }

    public int LeaseStatusId { get; set; }

    public int PurchaseConditionId { get; set; }

    public int TrackingSystemStatusId { get; set; }

    public int IntensiveEngravingStatusId { get; set; }

    public int ModifiedAfterManufacturingStatusId { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public QuoteEntity? Quote { get; set; }

    public VehicleManufacturerEntity? Manufacturer { get; set; }

    public VehicleModelEntity? VehicleModel { get; set; }

    public QuoteVehicleUsageEntity? Usage { get; set; }
}
