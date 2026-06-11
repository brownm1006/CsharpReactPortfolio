using PortfolioClubAssurance.Api.Services;

namespace PortfolioClubAssurance.Api.Controllers;

public static class QuoteLookupController
{
    public static void MapQuoteLookupRoutes(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/quote/lookups/vehicle-description", async (
            string? locale,
            IQuoteService quoteService,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await quoteService.GetVehicleDescriptionLookupsAsync(locale, cancellationToken));
        });

        app.MapGet("/api/quote/lookups/vehicle-usage", async (
            string? locale,
            IQuoteService quoteService,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await quoteService.GetVehicleUsageLookupsAsync(locale, cancellationToken));
        });

        app.MapGet("/api/quote/lookups/yes-no-unknown", async (
            string? locale,
            IQuoteService quoteService,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await quoteService.GetYesNoUnknownOptionsAsync(locale, cancellationToken));
        });

        app.MapGet("/api/quote/lookups/purchase-conditions", async (
            string? locale,
            IQuoteService quoteService,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await quoteService.GetPurchaseConditionOptionsAsync(locale, cancellationToken));
        });

        app.MapGet("/api/quote/lookups/manufacturers", async (
            IQuoteService quoteService,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await quoteService.GetManufacturersAsync(cancellationToken));
        });

        app.MapGet("/api/quote/lookups/manufacturers/{manufacturerCode}/models", async (
            string manufacturerCode,
            int? modelYear,
            IQuoteService quoteService,
            CancellationToken cancellationToken) =>
        {
            var result = await quoteService.GetModelsAsync(manufacturerCode, modelYear, cancellationToken);
            return EndpointResultMapper.ToResult(result);
        });
    }
}
