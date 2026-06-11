using PortfolioClubAssurance.Api.Contracts;

namespace PortfolioClubAssurance.Api.Validation;

public sealed class QuoteVehicleValidator : RequestValidator<CreateQuoteVehicleRequest>
{
    public override Dictionary<string, string[]> Validate(CreateQuoteVehicleRequest request)
    {
        var requiredErrors = new[]
            {
                (FieldName: nameof(request.ManufacturerCode), Value: request.ManufacturerCode),
                (FieldName: nameof(request.VehicleModelCode), Value: request.VehicleModelCode),
                (FieldName: nameof(request.IsLeasedCode), Value: request.IsLeasedCode),
                (FieldName: nameof(request.PurchaseConditionCode), Value: request.PurchaseConditionCode),
                (FieldName: nameof(request.TrackingSystemCode), Value: request.TrackingSystemCode),
                (FieldName: nameof(request.IntensiveEngravingCode), Value: request.IntensiveEngravingCode),
                (FieldName: nameof(request.ModifiedAfterManufacturingCode), Value: request.ModifiedAfterManufacturingCode),
                (FieldName: nameof(request.OutsideOfProvinceForPersonalUseCode), Value: request.OutsideOfProvinceForPersonalUseCode)
            }
            .Where(field => string.IsNullOrWhiteSpace(field.Value))
            .ToDictionary(
                field => field.FieldName,
                _ => new[] { "The field is required." });

        var rangeErrors = new (string FieldName, bool HasError, string Message)[]
            {
                (nameof(request.ModelYear), request.ModelYear is < 1980 or > 2100, "Model year must be between 1980 and 2100."),
                (nameof(request.PurchaseYear), request.PurchaseYear is < 1900 or > 2100, "Purchase year must be between 1900 and 2100."),
                (nameof(request.PurchaseMonth), request.PurchaseMonth is < 1 or > 12, "Purchase month must be between 1 and 12."),
                (nameof(request.PurchaseYear), request.PurchaseYear < request.ModelYear, "Purchase year must be greater than or equal to the vehicle construction year."),
                (nameof(request.CurrentOdometerKm), request.CurrentOdometerKm < 0, "Current odometer must be greater than or equal to zero."),
                (nameof(request.AnnualDistanceKm), request.AnnualDistanceKm < 0, "Annual distance must be greater than or equal to zero.")
            }
            .Where(rule => rule.HasError)
            .GroupBy(rule => rule.FieldName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(rule => rule.Message).ToArray());

        return requiredErrors
            .Concat(rangeErrors)
            .GroupBy(error => error.Key)
            .ToDictionary(
                group => group.Key,
                group => group.SelectMany(error => error.Value).ToArray());
    }
}
