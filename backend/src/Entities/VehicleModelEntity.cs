namespace PortfolioClubAssurance.Api.Entities;

public sealed class VehicleModelEntity
{
    public int Id { get; set; }

    public int ManufacturerId { get; set; }

    public int ModelYear { get; set; }

    public string ModelName { get; set; } = string.Empty;

    public string? Trim { get; set; }

    public string? VehicleCode { get; set; }

    public bool IsActive { get; set; }

    public VehicleManufacturerEntity? Manufacturer { get; set; }

    public ICollection<QuoteVehicleEntity> QuoteVehicles { get; set; } = [];
}
