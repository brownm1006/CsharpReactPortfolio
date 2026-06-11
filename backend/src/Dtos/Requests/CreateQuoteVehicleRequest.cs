namespace PortfolioClubAssurance.Api.Dtos.Requests;

public sealed record CreateQuoteVehicleRequest(
    int ModelYear,
    string ManufacturerCode,
    string VehicleModelCode,
    int PurchaseYear,
    int PurchaseMonth,
    string IsLeasedCode,
    string PurchaseConditionCode,
    string TrackingSystemCode,
    string IntensiveEngravingCode,
    string ModifiedAfterManufacturingCode,
    string OutsideOfProvinceForPersonalUseCode,
    int CurrentOdometerKm,
    int AnnualDistanceKm,
    bool DriveForBusiness);
