using Microsoft.AspNetCore.Diagnostics;
using PortfolioClubAssurance.Api.Contracts;
using PortfolioClubAssurance.Api.Data;
using PortfolioClubAssurance.Api.Options;
using PortfolioClubAssurance.Api.Services;
using PortfolioClubAssurance.Api.Validation;

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?? ["http://localhost:5173"];

builder.Services.Configure<QuoteDatabaseOptions>(builder.Configuration.GetSection(QuoteDatabaseOptions.SectionName));
builder.Services.AddSingleton<INpgsqlConnectionFactory, NpgsqlConnectionFactory>();
builder.Services.AddSingleton<RequestValidator<CreateQuoteVehicleRequest>, QuoteVehicleValidator>();
builder.Services.AddScoped<IQuoteRepository, PostgresQuoteRepository>();
builder.Services.AddScoped<IQuoteService, QuoteService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseExceptionHandler(exceptionApp =>
{
    exceptionApp.Run((HttpContext context) =>
    {
        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

        if (exceptionFeature?.Error is not null)
        {
            logger.LogError(exceptionFeature.Error, "Unhandled backend exception.");
        }

        return Results.Problem("An unexpected backend error occurred.").ExecuteAsync(context);
    });
});

app.UseCors("Frontend");

app.MapGet("/", () => Results.Redirect("/health"));

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "PortfolioClubAssurance.Api",
    utc = DateTimeOffset.UtcNow
}));

app.MapGet("/api/database/health", async (IQuoteService quoteService, CancellationToken cancellationToken) =>
{
    return Results.Ok(await quoteService.GetDatabaseHealthAsync(cancellationToken));
});

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
    return ToResult(result);
});

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

app.MapGet("/api/quotes/{quoteId:guid}/vehicles", async (
    Guid quoteId,
    string? locale,
    IQuoteService quoteService,
    CancellationToken cancellationToken) =>
{
    var result = await quoteService.GetVehiclesAsync(quoteId, locale, cancellationToken);
    return ToResult(result);
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
    return ToResult(result);
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

app.Run();

static IResult ToResult<T>(ServiceResult<T> result)
{
    if (result.ValidationErrors is not null)
    {
        return Results.ValidationProblem(result.ValidationErrors);
    }

    if (result.NotFoundMessage is not null)
    {
        return Results.NotFound(new ProblemDetailsResponse(result.NotFoundMessage));
    }

    return Results.Ok(result.Value);
}

public partial class Program
{
}
