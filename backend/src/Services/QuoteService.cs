using PortfolioClubAssurance.Api.Dtos.Lookups;
using PortfolioClubAssurance.Api.Dtos.Requests;
using PortfolioClubAssurance.Api.Dtos.Responses;
using PortfolioClubAssurance.Api.Entities;
using PortfolioClubAssurance.Api.Repositories;
using PortfolioClubAssurance.Api.Services.Common;
using PortfolioClubAssurance.Api.Validation;

namespace PortfolioClubAssurance.Api.Services;

public sealed class QuoteService : IQuoteService
{
    private readonly IQuoteRepository repository;
    private readonly RequestValidator<CreateQuoteVehicleRequest> vehicleValidator;
    private readonly ILogger<QuoteService> logger;

    public QuoteService(
        IQuoteRepository repository,
        RequestValidator<CreateQuoteVehicleRequest> vehicleValidator,
        ILogger<QuoteService> logger)
    {
        this.repository = repository;
        this.vehicleValidator = vehicleValidator;
        this.logger = logger;
    }

    public Task<DatabaseHealthResponse> GetDatabaseHealthAsync(CancellationToken cancellationToken)
    {
        return repository.GetDatabaseHealthAsync(cancellationToken);
    }

    public Task<VehicleDescriptionLookupsResponse> GetVehicleDescriptionLookupsAsync(string? locale, CancellationToken cancellationToken)
    {
        return repository.GetVehicleDescriptionLookupsAsync(NormalizeLocale(locale), cancellationToken);
    }

    public Task<VehicleUsageLookupsResponse> GetVehicleUsageLookupsAsync(string? locale, CancellationToken cancellationToken)
    {
        return repository.GetVehicleUsageLookupsAsync(NormalizeLocale(locale), cancellationToken);
    }

    public Task<IReadOnlyList<LookupOption>> GetYesNoUnknownOptionsAsync(string? locale, CancellationToken cancellationToken)
    {
        return repository.GetYesNoUnknownOptionsAsync(NormalizeLocale(locale), cancellationToken);
    }

    public Task<IReadOnlyList<LookupOption>> GetPurchaseConditionOptionsAsync(string? locale, CancellationToken cancellationToken)
    {
        return repository.GetPurchaseConditionOptionsAsync(NormalizeLocale(locale), cancellationToken);
    }

    public Task<IReadOnlyList<LookupOption>> GetManufacturersAsync(CancellationToken cancellationToken)
    {
        return repository.GetManufacturersAsync(cancellationToken);
    }

    public Task<ServiceResult<IReadOnlyList<VehicleModelOption>>> GetModelsAsync(
        string manufacturerCode,
        int? modelYear,
        CancellationToken cancellationToken)
    {
        return repository.GetModelsAsync(manufacturerCode, modelYear, cancellationToken);
    }

    public async Task<ServiceResult<QuoteResponse>> CreateQuoteAsync(CreateQuoteRequest? request, CancellationToken cancellationToken)
    {
        var productType = string.IsNullOrWhiteSpace(request?.ProductType) ? ProductType.Auto.ToString() : request.ProductType;

        if (!StringComparer.Ordinal.Equals(productType, ProductType.Auto.ToString()))
        {
            return ServiceResult<QuoteResponse>.Validation(new Dictionary<string, string[]>
            {
                ["productType"] = ["Only Auto quotes are supported."]
            });
        }

        var quote = await repository.CreateQuoteAsync(cancellationToken);
        logger.LogInformation("Created quote {QuoteId}", quote.Id);

        return ServiceResult<QuoteResponse>.Success(quote);
    }

    public async Task<ServiceResult<IReadOnlyList<QuoteVehicleResponse>>> GetVehiclesAsync(
        Guid quoteId,
        string? locale,
        CancellationToken cancellationToken)
    {
        if (!await repository.QuoteExistsAsync(quoteId, cancellationToken))
        {
            return ServiceResult<IReadOnlyList<QuoteVehicleResponse>>.NotFound("Quote not found.");
        }

        var vehicles = await repository.GetVehiclesAsync(quoteId, null, NormalizeLocale(locale), cancellationToken);
        return ServiceResult<IReadOnlyList<QuoteVehicleResponse>>.Success(vehicles);
    }

    public async Task<ServiceResult<QuoteVehicleResponse>> CreateVehicleAsync(
        Guid quoteId,
        CreateQuoteVehicleRequest request,
        CancellationToken cancellationToken)
    {
        var validationErrors = vehicleValidator.Validate(request);

        if (validationErrors.Count > 0)
        {
            return ServiceResult<QuoteVehicleResponse>.Validation(validationErrors);
        }

        if (!await repository.QuoteExistsAsync(quoteId, cancellationToken))
        {
            return ServiceResult<QuoteVehicleResponse>.NotFound("Quote not found.");
        }

        var result = await repository.CreateVehicleAsync(quoteId, request, cancellationToken);

        if (result.IsSuccess)
        {
            logger.LogInformation("Saved vehicle {VehicleId} for quote {QuoteId}", result.Value!.Id, quoteId);
        }

        return result;
    }

    public async Task<ServiceResult<QuoteVehicleResponse>> GetVehicleAsync(
        Guid quoteId,
        Guid vehicleId,
        string? locale,
        CancellationToken cancellationToken)
    {
        var vehicles = await repository.GetVehiclesAsync(quoteId, vehicleId, NormalizeLocale(locale), cancellationToken);
        var vehicle = vehicles.SingleOrDefault();

        return vehicle is null
            ? ServiceResult<QuoteVehicleResponse>.NotFound("Vehicle not found.")
            : ServiceResult<QuoteVehicleResponse>.Success(vehicle);
    }

    public async Task<ServiceResult<bool>> DeleteVehicleAsync(Guid quoteId, Guid vehicleId, CancellationToken cancellationToken)
    {
        var deleted = await repository.DeleteVehicleAsync(quoteId, vehicleId, cancellationToken);

        if (!deleted)
        {
            return ServiceResult<bool>.NotFound("Vehicle not found.");
        }

        logger.LogInformation("Deleted vehicle {VehicleId} from quote {QuoteId}", vehicleId, quoteId);
        return ServiceResult<bool>.Success(true);
    }

    private static string NormalizeLocale(string? locale)
    {
        return string.Equals(locale, "en", StringComparison.OrdinalIgnoreCase) ? "en" : "fr";
    }
}
