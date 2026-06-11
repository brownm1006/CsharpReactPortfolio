namespace PortfolioClubAssurance.Api.Dtos.Lookups;

public sealed record VehicleDescriptionLookupsResponse(
    IReadOnlyList<LookupOption> ModelYears,
    IReadOnlyList<LookupOption> PurchaseYears,
    IReadOnlyList<LookupOption> Months,
    IReadOnlyList<LookupOption> Manufacturers,
    IReadOnlyList<LookupOption> YesNoUnknown,
    IReadOnlyList<LookupOption> PurchaseConditions);
