using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using PortfolioClubAssurance.Api.Controllers;
using PortfolioClubAssurance.Api.Data;
using PortfolioClubAssurance.Api.Dtos.Requests;
using PortfolioClubAssurance.Api.Options;
using PortfolioClubAssurance.Api.Repositories;
using PortfolioClubAssurance.Api.Services;
using PortfolioClubAssurance.Api.Validation;

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?? ["http://localhost:5173"];

builder.Services.Configure<QuoteDatabaseOptions>(builder.Configuration.GetSection(QuoteDatabaseOptions.SectionName));
builder.Services.AddDbContext<QuoteDbContext>((serviceProvider, options) =>
{
    var databaseOptions = serviceProvider
        .GetRequiredService<Microsoft.Extensions.Options.IOptions<QuoteDatabaseOptions>>()
        .Value;

    if (!string.IsNullOrWhiteSpace(databaseOptions.QuoteDatabase))
    {
        options.UseNpgsql(databaseOptions.QuoteDatabase);
    }
});
builder.Services.AddSingleton<RequestValidator<CreateQuoteVehicleRequest>, QuoteVehicleValidator>();
builder.Services.AddScoped<IQuoteRepository, EfQuoteRepository>();
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

app.MapHealthRoutes();
app.MapQuoteLookupRoutes();
app.MapQuoteRoutes();
app.MapQuoteVehicleRoutes();

app.Run();

public partial class Program
{
}
