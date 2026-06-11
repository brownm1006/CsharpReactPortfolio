using PortfolioClubAssurance.Api.Contracts;

namespace PortfolioClubAssurance.Api.Services;

public interface IQuoteService
{
    Task<DatabaseHealthResponse> GetDatabaseHealthAsync(CancellationToken cancellationToken);

    Task<VehicleDescriptionLookupsResponse> GetVehicleDescriptionLookupsAsync(string? locale, CancellationToken cancellationToken);

    Task<VehicleUsageLookupsResponse> GetVehicleUsageLookupsAsync(string? locale, CancellationToken cancellationToken);

    Task<IReadOnlyList<LookupOption>> GetYesNoUnknownOptionsAsync(string? locale, CancellationToken cancellationToken);

    Task<IReadOnlyList<LookupOption>> GetPurchaseConditionOptionsAsync(string? locale, CancellationToken cancellationToken);

    Task<IReadOnlyList<LookupOption>> GetManufacturersAsync(CancellationToken cancellationToken);

    Task<ServiceResult<IReadOnlyList<VehicleModelOption>>> GetModelsAsync(
        string manufacturerCode,
        int? modelYear,
        CancellationToken cancellationToken);

    Task<ServiceResult<QuoteResponse>> CreateQuoteAsync(CreateQuoteRequest? request, CancellationToken cancellationToken);

    Task<ServiceResult<IReadOnlyList<QuoteVehicleResponse>>> GetVehiclesAsync(
        Guid quoteId,
        string? locale,
        CancellationToken cancellationToken);

    Task<ServiceResult<QuoteVehicleResponse>> CreateVehicleAsync(
        Guid quoteId,
        CreateQuoteVehicleRequest request,
        CancellationToken cancellationToken);

    Task<ServiceResult<QuoteVehicleResponse>> GetVehicleAsync(
        Guid quoteId,
        Guid vehicleId,
        string? locale,
        CancellationToken cancellationToken);

    Task<ServiceResult<bool>> DeleteVehicleAsync(Guid quoteId, Guid vehicleId, CancellationToken cancellationToken);
}
