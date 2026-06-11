using PortfolioClubAssurance.Api.Dtos.Lookups;

namespace PortfolioClubAssurance.Api.Dtos.Responses;

public sealed record QuoteVehicleResponse(
    Guid Id,
    Guid QuoteId,
    int ModelYear,
    LookupOption Manufacturer,
    VehicleModelOption VehicleModel,
    string? VehicleCode,
    int PurchaseYear,
    int PurchaseMonth,
    LookupOption IsLeased,
    LookupOption PurchaseCondition,
    LookupOption TrackingSystem,
    LookupOption IntensiveEngraving,
    LookupOption ModifiedAfterManufacturing,
    LookupOption OutsideOfProvinceForPersonalUse,
    int CurrentOdometerKm,
    int AnnualDistanceKm,
    bool DriveForBusiness,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
