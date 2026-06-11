namespace PortfolioClubAssurance.Api.Validation;

public abstract class RequestValidator<TRequest>
{
    public abstract Dictionary<string, string[]> Validate(TRequest request);
}
