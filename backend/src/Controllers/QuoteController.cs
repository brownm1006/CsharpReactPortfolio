using PortfolioClubAssurance.Api.Dtos.Requests;
using PortfolioClubAssurance.Api.Services;

namespace PortfolioClubAssurance.Api.Controllers;

public static class QuoteController
{
    public static void MapQuoteRoutes(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/quotes", async (
            CreateQuoteRequest? request,
            IQuoteService quoteService,
            CancellationToken cancellationToken) =>
        {
            var result = await quoteService.CreateQuoteAsync(request, cancellationToken);
            return result.ValidationErrors is not null
                ? Results.ValidationProblem(result.ValidationErrors)
                : Results.Created($"/api/quotes/{result.Value!.Id}", result.Value);
        });
    }
}
