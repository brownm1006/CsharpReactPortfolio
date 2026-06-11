using PortfolioClubAssurance.Api.Dtos.Responses;
using PortfolioClubAssurance.Api.Services.Common;

namespace PortfolioClubAssurance.Api.Controllers;

internal static class EndpointResultMapper
{
    public static IResult ToResult<T>(ServiceResult<T> result)
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
}
