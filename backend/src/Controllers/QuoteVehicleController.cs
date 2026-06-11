using PortfolioClubAssurance.Api.Dtos.Requests;
using PortfolioClubAssurance.Api.Dtos.Responses;
using PortfolioClubAssurance.Api.Services;

namespace PortfolioClubAssurance.Api.Controllers;

public static class QuoteVehicleController
{
    public static void MapQuoteVehicleRoutes(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/quotes/{quoteId:guid}/vehicles", async (
            Guid quoteId,
            string? locale,
            IQuoteService quoteService,
            CancellationToken cancellationToken) =>
        {
            var result = await quoteService.GetVehiclesAsync(quoteId, locale, cancellationToken);
            return EndpointResultMapper.ToResult(result);
        });

        app.MapPost("/api/quotes/{quoteId:guid}/vehicles", async (
            Guid quoteId,
            CreateQuoteVehicleRequest request,
            IQuoteService quoteService,
            CancellationToken cancellationToken) =>
        {
            var result = await quoteService.CreateVehicleAsync(quoteId, request, cancellationToken);
            return result.ValidationErrors is not null
                ? Results.ValidationProblem(result.ValidationErrors)
                : result.NotFoundMessage is not null
                    ? Results.NotFound(new ProblemDetailsResponse(result.NotFoundMessage))
                    : Results.Created($"/api/quotes/{quoteId}/vehicles/{result.Value!.Id}", result.Value);
        });

        app.MapGet("/api/quotes/{quoteId:guid}/vehicles/{vehicleId:guid}", async (
            Guid quoteId,
            Guid vehicleId,
            string? locale,
            IQuoteService quoteService,
            CancellationToken cancellationToken) =>
        {
            var result = await quoteService.GetVehicleAsync(quoteId, vehicleId, locale, cancellationToken);
            return EndpointResultMapper.ToResult(result);
        });

        app.MapDelete("/api/quotes/{quoteId:guid}/vehicles/{vehicleId:guid}", async (
            Guid quoteId,
            Guid vehicleId,
            IQuoteService quoteService,
            CancellationToken cancellationToken) =>
        {
            var result = await quoteService.DeleteVehicleAsync(quoteId, vehicleId, cancellationToken);
            return result.NotFoundMessage is not null
                ? Results.NotFound(new ProblemDetailsResponse(result.NotFoundMessage))
                : Results.NoContent();
        });
    }
}
