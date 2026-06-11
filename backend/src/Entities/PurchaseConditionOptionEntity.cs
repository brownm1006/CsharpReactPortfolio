namespace PortfolioClubAssurance.Api.Entities;

public sealed class PurchaseConditionOptionEntity
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string DisplayText { get; set; } = string.Empty;

    public string? DisplayTextEn { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }
}
