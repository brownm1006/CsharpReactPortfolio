namespace PortfolioClubAssurance.Api.Dtos.Responses;

public sealed record QuoteResponse(
    Guid Id,
    string ProductType,
    string Status,
    string? CurrentStep,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
