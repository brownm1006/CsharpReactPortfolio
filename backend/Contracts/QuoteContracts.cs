namespace PortfolioClubAssurance.Api.Contracts;

public enum ProductType
{
    Auto
}

public enum QuoteStatus
{
    Draft,
    Submitted,
    Quoted,
    Abandoned
}

public sealed record DatabaseHealthResponse(string Status, string Schema, long TableCount);

public sealed record LookupOption(string Code, string DisplayText, int SortOrder);

public sealed record VehicleModelOption(
    string Code,
    string DisplayText,
    int SortOrder,
    int ModelYear,
    string ManufacturerCode,
    string? Trim,
    string? VehicleCode);

public sealed record VehicleDescriptionLookupsResponse(
    IReadOnlyList<LookupOption> ModelYears,
    IReadOnlyList<LookupOption> PurchaseYears,
    IReadOnlyList<LookupOption> Months,
    IReadOnlyList<LookupOption> Manufacturers,
    IReadOnlyList<LookupOption> YesNoUnknown,
    IReadOnlyList<LookupOption> PurchaseConditions);

public sealed record VehicleUsageLookupsResponse(
    IReadOnlyList<LookupOption> ProvinceUse,
    IReadOnlyList<LookupOption> Odometer,
    IReadOnlyList<LookupOption> AnnualDistance,
    IReadOnlyList<LookupOption> YesNo);

public sealed record CreateQuoteRequest(string? ProductType);

public sealed record QuoteResponse(
    Guid Id,
    string ProductType,
    string Status,
    string? CurrentStep,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

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

public sealed record ProblemDetailsResponse(string Detail);

public sealed record ServiceResult<T>(T? Value, Dictionary<string, string[]>? ValidationErrors, string? NotFoundMessage)
{
    public bool IsSuccess => Value is not null && ValidationErrors is null && NotFoundMessage is null;

    public static ServiceResult<T> Success(T value)
    {
        return new ServiceResult<T>(value, null, null);
    }

    public static ServiceResult<T> Validation(Dictionary<string, string[]> errors)
    {
        return new ServiceResult<T>(default, errors, null);
    }

    public static ServiceResult<T> NotFound(string message)
    {
        return new ServiceResult<T>(default, null, message);
    }
}
