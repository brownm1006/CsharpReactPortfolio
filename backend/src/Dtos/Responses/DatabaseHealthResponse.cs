namespace PortfolioClubAssurance.Api.Dtos.Responses;

public sealed record DatabaseHealthResponse(string Status, string Schema, long TableCount);
