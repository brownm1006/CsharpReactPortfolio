using PortfolioClubAssurance.Api.Contracts;
using PortfolioClubAssurance.Api.Validation;
using Xunit;

namespace PortfolioClubAssurance.Api.Tests;

public sealed class QuoteVehicleValidatorTests
{
    private readonly RequestValidator<CreateQuoteVehicleRequest> validator = new QuoteVehicleValidator();

    [Fact]
    public void ValidateCreateQuoteVehicleRequest_ReturnsNoErrors_WhenRequestIsValid()
    {
        var errors = validator.Validate(CreateValidRequest());

        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateCreateQuoteVehicleRequest_RequiresLookupCodes()
    {
        var request = CreateValidRequest() with
        {
            ManufacturerCode = "",
            VehicleModelCode = " ",
            IsLeasedCode = "",
            PurchaseConditionCode = "",
            TrackingSystemCode = "",
            IntensiveEngravingCode = "",
            ModifiedAfterManufacturingCode = "",
            OutsideOfProvinceForPersonalUseCode = ""
        };

        var errors = validator.Validate(request);

        Assert.Contains(nameof(CreateQuoteVehicleRequest.ManufacturerCode), errors.Keys);
        Assert.Contains(nameof(CreateQuoteVehicleRequest.VehicleModelCode), errors.Keys);
        Assert.Contains(nameof(CreateQuoteVehicleRequest.IsLeasedCode), errors.Keys);
        Assert.Contains(nameof(CreateQuoteVehicleRequest.PurchaseConditionCode), errors.Keys);
        Assert.Contains(nameof(CreateQuoteVehicleRequest.TrackingSystemCode), errors.Keys);
        Assert.Contains(nameof(CreateQuoteVehicleRequest.IntensiveEngravingCode), errors.Keys);
        Assert.Contains(nameof(CreateQuoteVehicleRequest.ModifiedAfterManufacturingCode), errors.Keys);
        Assert.Contains(nameof(CreateQuoteVehicleRequest.OutsideOfProvinceForPersonalUseCode), errors.Keys);
    }

    [Fact]
    public void ValidateCreateQuoteVehicleRequest_RejectsPurchaseYearBeforeModelYear()
    {
        var request = CreateValidRequest() with
        {
            ModelYear = 2026,
            PurchaseYear = 2025
        };

        var errors = validator.Validate(request);

        var error = Assert.Single(errors[nameof(CreateQuoteVehicleRequest.PurchaseYear)]);
        Assert.Equal("Purchase year must be greater than or equal to the vehicle construction year.", error);
    }

    [Fact]
    public void ValidateCreateQuoteVehicleRequest_RejectsInvalidRanges()
    {
        var request = CreateValidRequest() with
        {
            ModelYear = 1979,
            PurchaseYear = 1899,
            PurchaseMonth = 13,
            CurrentOdometerKm = -1,
            AnnualDistanceKm = -1
        };

        var errors = validator.Validate(request);

        Assert.Contains(nameof(CreateQuoteVehicleRequest.ModelYear), errors.Keys);
        Assert.Contains(nameof(CreateQuoteVehicleRequest.PurchaseYear), errors.Keys);
        Assert.Contains(nameof(CreateQuoteVehicleRequest.PurchaseMonth), errors.Keys);
        Assert.Contains(nameof(CreateQuoteVehicleRequest.CurrentOdometerKm), errors.Keys);
        Assert.Contains(nameof(CreateQuoteVehicleRequest.AnnualDistanceKm), errors.Keys);
    }

    private static CreateQuoteVehicleRequest CreateValidRequest()
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
