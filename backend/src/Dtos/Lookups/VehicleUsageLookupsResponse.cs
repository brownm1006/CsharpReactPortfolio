namespace PortfolioClubAssurance.Api.Dtos.Lookups;

public sealed record VehicleUsageLookupsResponse(
    IReadOnlyList<LookupOption> ProvinceUse,
    IReadOnlyList<LookupOption> Odometer,
    IReadOnlyList<LookupOption> AnnualDistance,
    IReadOnlyList<LookupOption> YesNo);
