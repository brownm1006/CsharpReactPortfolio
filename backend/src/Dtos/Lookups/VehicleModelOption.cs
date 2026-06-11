namespace PortfolioClubAssurance.Api.Dtos.Lookups;

public sealed record VehicleModelOption(
    string Code,
    string DisplayText,
    int SortOrder,
    int ModelYear,
    string ManufacturerCode,
    string? Trim,
    string? VehicleCode);
