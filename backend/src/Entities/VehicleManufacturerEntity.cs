namespace PortfolioClubAssurance.Api.Entities;

public sealed class VehicleManufacturerEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public ICollection<VehicleModelEntity> Models { get; set; } = [];

    public ICollection<QuoteVehicleEntity> QuoteVehicles { get; set; } = [];
}
