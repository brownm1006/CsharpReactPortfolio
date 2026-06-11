namespace PortfolioClubAssurance.Api.Entities;

public sealed class DistanceOptionEntity
{
    public int Id { get; set; }

    public string OptionType { get; set; } = string.Empty;

    public int Kilometers { get; set; }

    public string DisplayText { get; set; } = string.Empty;

    public string? DisplayTextEn { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }
}
