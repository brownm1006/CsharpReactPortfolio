using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PortfolioClubAssurance.Api.Dtos.Requests;
using PortfolioClubAssurance.Api.Repositories;
using PortfolioClubAssurance.Api.Services;
using PortfolioClubAssurance.Api.Validation;
using Xunit;

namespace PortfolioClubAssurance.Api.Tests;

public sealed class QuoteServiceTests
{
    private readonly Mock<IQuoteRepository> repository = new(MockBehavior.Strict);
    private readonly RequestValidator<CreateQuoteVehicleRequest> validator = new QuoteVehicleValidator();

    [Fact]
    public async Task CreateQuoteAsync_ReturnsValidationProblem_WhenProductTypeIsUnsupported()
    {
        var service = CreateService();

        var result = await service.CreateQuoteAsync(new CreateQuoteRequest("Home"), CancellationToken.None);

        Assert.NotNull(result.ValidationErrors);
        Assert.Contains("productType", result.ValidationErrors.Keys);
        repository.Verify(repo => repo.CreateQuoteAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateVehicleAsync_DoesNotCallRepositoryCreate_WhenRequestIsInvalid()
    {
        var service = CreateService();
        var invalidRequest = CreateValidVehicleRequest() with
        {
            PurchaseYear = 2025
        };

        var result = await service.CreateVehicleAsync(Guid.NewGuid(), invalidRequest, CancellationToken.None);

        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(nameof(CreateQuoteVehicleRequest.PurchaseYear), result.ValidationErrors.Keys);
        repository.Verify(repo => repo.QuoteExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        repository.Verify(
            repo => repo.CreateVehicleAsync(It.IsAny<Guid>(), It.IsAny<CreateQuoteVehicleRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateVehicleAsync_ReturnsNotFound_WhenQuoteDoesNotExist()
    {
        var quoteId = Guid.NewGuid();
        var service = CreateService();
        repository
            .Setup(repo => repo.QuoteExistsAsync(quoteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await service.CreateVehicleAsync(quoteId, CreateValidVehicleRequest(), CancellationToken.None);

        Assert.Equal("Quote not found.", result.NotFoundMessage);
        repository.Verify(repo => repo.QuoteExistsAsync(quoteId, It.IsAny<CancellationToken>()), Times.Once);
        repository.Verify(
            repo => repo.CreateVehicleAsync(It.IsAny<Guid>(), It.IsAny<CreateQuoteVehicleRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteVehicleAsync_ReturnsSuccess_WhenRepositoryDeletesVehicle()
    {
        var quoteId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var service = CreateService();
        repository
            .Setup(repo => repo.DeleteVehicleAsync(quoteId, vehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await service.DeleteVehicleAsync(quoteId, vehicleId, CancellationToken.None);

        Assert.True(result.Value);
        repository.Verify(repo => repo.DeleteVehicleAsync(quoteId, vehicleId, It.IsAny<CancellationToken>()), Times.Once);
    }

    private QuoteService CreateService()
    {
        return new QuoteService(repository.Object, validator, NullLogger<QuoteService>.Instance);
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
}
