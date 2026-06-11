using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Npgsql;
using PortfolioClubAssurance.Api.Dtos.Lookups;
using PortfolioClubAssurance.Api.Dtos.Requests;
using PortfolioClubAssurance.Api.Dtos.Responses;
using Testcontainers.PostgreSql;
using Xunit;

namespace PortfolioClubAssurance.Api.Tests;

public sealed class QuoteApiIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("portfolio_assurance")
        .WithUsername("portfolio")
        .WithPassword("portfolio_dev_password")
        .Build();

    private WebApplicationFactory<Program>? factory;
    private HttpClient? client;

    public async Task InitializeAsync()
    {
        await postgres.StartAsync();
        await SeedDatabaseAsync();

        factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureAppConfiguration((_, configurationBuilder) =>
                {
                    configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:QuoteDatabase"] = postgres.GetConnectionString(),
                        ["Cors:AllowedOrigins:0"] = "http://localhost:5173"
                    });
                });
            });

        client = factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        client?.Dispose();

        if (factory is not null)
        {
            await factory.DisposeAsync();
        }

        await postgres.DisposeAsync();
    }

    [Fact]
    public async Task DatabaseHealth_ReturnsQuoteSchemaTableCount()
    {
        var response = await Client.GetFromJsonAsync<DatabaseHealthTestResponse>("/api/database/health");

        Assert.NotNull(response);
        Assert.Equal("ok", response.Status);
        Assert.Equal("quote", response.Schema);
        Assert.True(response.TableCount >= 15);
    }

    [Fact]
    public async Task VehicleDescriptionLookups_ReturnSeededOptions()
    {
        var response = await Client.GetFromJsonAsync<VehicleDescriptionLookupsResponse>(
            "/api/quote/lookups/vehicle-description?locale=en");

        Assert.NotNull(response);
        Assert.Contains(response.Manufacturers, option => option.Code == "HYUNDAI");
        Assert.Contains(response.Months, option => option.Code == "01" && option.DisplayText == "January");
        Assert.Contains(response.PurchaseConditions, option => option.Code == "New" && option.DisplayText == "New");
    }

    [Fact]
    public async Task CreateListGetAndDeleteConfirmedVehicle_WritesToPostgres()
    {
        var quoteResponse = await Client.PostAsJsonAsync("/api/quotes", new CreateQuoteRequest(null));
        quoteResponse.EnsureSuccessStatusCode();
        var quote = await quoteResponse.Content.ReadFromJsonAsync<QuoteResponse>();
        Assert.NotNull(quote);

        var createVehicleResponse = await Client.PostAsJsonAsync(
            $"/api/quotes/{quote.Id}/vehicles",
            CreateValidVehicleRequest());
        Assert.Equal(HttpStatusCode.Created, createVehicleResponse.StatusCode);

        var createdVehicle = await createVehicleResponse.Content.ReadFromJsonAsync<QuoteVehicleResponse>();
        Assert.NotNull(createdVehicle);
        Assert.Equal("HYUNDAI", createdVehicle.Manufacturer.Code);
        Assert.Equal("IONIQ_5_PREFERRED_LONG_RANGE_4", createdVehicle.VehicleModel.Code);

        var vehicles = await Client.GetFromJsonAsync<List<QuoteVehicleResponse>>($"/api/quotes/{quote.Id}/vehicles");
        Assert.NotNull(vehicles);
        var listedVehicle = Assert.Single(vehicles);
        Assert.Equal(createdVehicle.Id, listedVehicle.Id);

        var fetchedVehicle = await Client.GetFromJsonAsync<QuoteVehicleResponse>(
            $"/api/quotes/{quote.Id}/vehicles/{createdVehicle.Id}?locale=en");
        Assert.NotNull(fetchedVehicle);
        Assert.Equal("No", fetchedVehicle.IsLeased.DisplayText);

        var deleteResponse = await Client.DeleteAsync($"/api/quotes/{quote.Id}/vehicles/{createdVehicle.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        vehicles = await Client.GetFromJsonAsync<List<QuoteVehicleResponse>>($"/api/quotes/{quote.Id}/vehicles");
        Assert.NotNull(vehicles);
        Assert.Empty(vehicles);
    }

    [Fact]
    public async Task CreateVehicle_ReturnsValidationProblem_WhenPurchaseYearIsBeforeModelYear()
    {
        var quoteResponse = await Client.PostAsJsonAsync("/api/quotes", new CreateQuoteRequest(null));
        quoteResponse.EnsureSuccessStatusCode();
        var quote = await quoteResponse.Content.ReadFromJsonAsync<QuoteResponse>();
        Assert.NotNull(quote);

        var invalidVehicle = CreateValidVehicleRequest() with
        {
            PurchaseYear = 2025
        };

        var response = await Client.PostAsJsonAsync($"/api/quotes/{quote.Id}/vehicles", invalidVehicle);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("PurchaseYear", body, StringComparison.Ordinal);
    }

    private HttpClient Client => client ?? throw new InvalidOperationException("The test HTTP client has not been initialized.");

    private async Task SeedDatabaseAsync()
    {
        var scriptPath = FindSchemaScriptPath();
        var script = await File.ReadAllTextAsync(scriptPath);

        await using var connection = new NpgsqlConnection(postgres.GetConnectionString());
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(script, connection);
        await command.ExecuteNonQueryAsync();
    }

    private static string FindSchemaScriptPath()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "scripts", "create_quote_schema.sql");

            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException("Could not find scripts/create_quote_schema.sql from the test output directory.");
    }

    private static CreateQuoteVehicleRequest CreateValidVehicleRequest()
    {
        return new CreateQuoteVehicleRequest(
            2026,
            "HYUNDAI",
            "IONIQ_5_PREFERRED_LONG_RANGE_4",
            2026,
            6,
            "No",
            "New",
            "Yes",
            "No",
            "Unknown",
            "No",
            10000,
            15000,
            false);
    }

    private sealed record DatabaseHealthTestResponse(string Status, string Schema, long TableCount);
}
