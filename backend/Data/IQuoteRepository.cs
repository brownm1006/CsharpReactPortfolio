using PortfolioClubAssurance.Api.Contracts;

namespace PortfolioClubAssurance.Api.Data;

public interface IQuoteRepository
{
    Task<DatabaseHealthResponse> GetDatabaseHealthAsync(CancellationToken cancellationToken);

    Task<VehicleDescriptionLookupsResponse> GetVehicleDescriptionLookupsAsync(string locale, CancellationToken cancellationToken);

    Task<VehicleUsageLookupsResponse> GetVehicleUsageLookupsAsync(string locale, CancellationToken cancellationToken);

    Task<IReadOnlyList<LookupOption>> GetYesNoUnknownOptionsAsync(string locale, CancellationToken cancellationToken);

    Task<IReadOnlyList<LookupOption>> GetPurchaseConditionOptionsAsync(string locale, CancellationToken cancellationToken);

    Task<IReadOnlyList<LookupOption>> GetManufacturersAsync(CancellationToken cancellationToken);

    Task<ServiceResult<IReadOnlyList<VehicleModelOption>>> GetModelsAsync(
        string manufacturerCode,
        int? modelYear,
        CancellationToken cancellationToken);

    Task<QuoteResponse> CreateQuoteAsync(CancellationToken cancellationToken);

    Task<bool> QuoteExistsAsync(Guid quoteId, CancellationToken cancellationToken);

    Task<IReadOnlyList<QuoteVehicleResponse>> GetVehiclesAsync(
        Guid quoteId,
        Guid? vehicleId,
        string locale,
        CancellationToken cancellationToken);

    Task<ServiceResult<QuoteVehicleResponse>> CreateVehicleAsync(
        Guid quoteId,
        CreateQuoteVehicleRequest request,
        CancellationToken cancellationToken);

    Task<bool> DeleteVehicleAsync(Guid quoteId, Guid vehicleId, CancellationToken cancellationToken);
}
