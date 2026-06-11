using PortfolioClubAssurance.Api.Services;

namespace PortfolioClubAssurance.Api.Controllers;

public static class HealthController
{
    public static void MapHealthRoutes(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", () => Results.Redirect("/health"));

        app.MapGet("/health", () => Results.Ok(new
        {
            status = "ok",
            service = "PortfolioClubAssurance.Api",
            utc = DateTimeOffset.UtcNow
        }));

        app.MapGet("/api/database/health", async (
            IQuoteService quoteService,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await quoteService.GetDatabaseHealthAsync(cancellationToken));
        });
    }
}
