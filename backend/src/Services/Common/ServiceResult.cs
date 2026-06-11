namespace PortfolioClubAssurance.Api.Services.Common;

public sealed record ServiceResult<T>(T? Value, Dictionary<string, string[]>? ValidationErrors, string? NotFoundMessage)
{
    public bool IsSuccess => Value is not null && ValidationErrors is null && NotFoundMessage is null;

    public static ServiceResult<T> Success(T value)
    {
        return new ServiceResult<T>(value, null, null);
    }

    public static ServiceResult<T> Validation(Dictionary<string, string[]> errors)
    {
        return new ServiceResult<T>(default, errors, null);
    }

    public static ServiceResult<T> NotFound(string message)
    {
        return new ServiceResult<T>(default, null, message);
    }
}
