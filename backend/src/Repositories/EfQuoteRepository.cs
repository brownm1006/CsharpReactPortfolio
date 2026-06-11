using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PortfolioClubAssurance.Api.Data;
using PortfolioClubAssurance.Api.Dtos.Lookups;
using PortfolioClubAssurance.Api.Dtos.Requests;
using PortfolioClubAssurance.Api.Dtos.Responses;
using PortfolioClubAssurance.Api.Entities;
using PortfolioClubAssurance.Api.Services.Common;

namespace PortfolioClubAssurance.Api.Repositories;

public sealed class EfQuoteRepository : IQuoteRepository
{
    private readonly QuoteDbContext dbContext;

    public EfQuoteRepository(QuoteDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<DatabaseHealthResponse> GetDatabaseHealthAsync(CancellationToken cancellationToken)
    {
        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
        var tableCount = canConnect
            ? await dbContext.Database
                .SqlQueryRaw<long>(
                    """
                    select count(*) as "Value"
                    from information_schema.tables
                    where table_schema = 'quote'
                    """)
                .SingleAsync(cancellationToken)
            : 0L;

        return new DatabaseHealthResponse(canConnect ? "ok" : "unavailable", "quote", tableCount);
    }

    public async Task<VehicleDescriptionLookupsResponse> GetVehicleDescriptionLookupsAsync(
        string locale,
        CancellationToken cancellationToken)
    {
        var modelYears = BuildYearOptions(DateTime.UtcNow.Year + 1, 8);
        var purchaseYears = BuildYearOptions(DateTime.UtcNow.Year, 15);
        var months = await ReadMonthOptionsAsync(locale, cancellationToken);
        var manufacturers = await GetManufacturersAsync(cancellationToken);
        var yesNoUnknown = await ReadYesNoUnknownOptionsAsync(locale, includeUnknown: true, cancellationToken);
        var purchaseConditions = await ReadPurchaseConditionOptionsAsync(locale, cancellationToken);

        return new VehicleDescriptionLookupsResponse(
            modelYears,
            purchaseYears,
            months,
            manufacturers,
            yesNoUnknown,
            purchaseConditions);
    }

    public async Task<VehicleUsageLookupsResponse> GetVehicleUsageLookupsAsync(
        string locale,
        CancellationToken cancellationToken)
    {
        var provinceUse = await ReadYesNoUnknownOptionsAsync(locale, includeUnknown: true, cancellationToken);
        var odometer = await ReadDistanceOptionsAsync(locale, "CurrentOdometer", cancellationToken);
        var annualDistance = await ReadDistanceOptionsAsync(locale, "AnnualDistance", cancellationToken);
        var yesNo = await ReadYesNoUnknownOptionsAsync(locale, includeUnknown: false, cancellationToken);

        return new VehicleUsageLookupsResponse(provinceUse, odometer, annualDistance, yesNo);
    }

    public async Task<IReadOnlyList<LookupOption>> GetYesNoUnknownOptionsAsync(
        string locale,
        CancellationToken cancellationToken)
    {
        return await ReadYesNoUnknownOptionsAsync(locale, includeUnknown: true, cancellationToken);
    }

    public async Task<IReadOnlyList<LookupOption>> GetPurchaseConditionOptionsAsync(
        string locale,
        CancellationToken cancellationToken)
    {
        return await ReadPurchaseConditionOptionsAsync(locale, cancellationToken);
    }

    public async Task<IReadOnlyList<LookupOption>> GetManufacturersAsync(CancellationToken cancellationToken)
    {
        var manufacturers = await dbContext.VehicleManufacturers
            .AsNoTracking()
            .Where(manufacturer => manufacturer.IsActive)
            .OrderBy(manufacturer => manufacturer.Name)
            .Select(manufacturer => new
            {
                manufacturer.Name,
                manufacturer.Id
            })
            .ToListAsync(cancellationToken);

        return manufacturers
            .Select(manufacturer => new LookupOption(manufacturer.Name, manufacturer.Name, manufacturer.Id))
            .ToList();
    }

    public async Task<ServiceResult<IReadOnlyList<VehicleModelOption>>> GetModelsAsync(
        string manufacturerCode,
        int? modelYear,
        CancellationToken cancellationToken)
    {
        var manufacturerExists = await dbContext.VehicleManufacturers
            .AsNoTracking()
            .AnyAsync(
                manufacturer => manufacturer.Name == manufacturerCode && manufacturer.IsActive,
                cancellationToken);

        if (!manufacturerExists)
        {
            return ServiceResult<IReadOnlyList<VehicleModelOption>>.NotFound("Manufacturer not found.");
        }

        var query = dbContext.VehicleModels
            .AsNoTracking()
            .Where(model =>
                model.IsActive
                && model.Manufacturer != null
                && model.Manufacturer.IsActive
                && model.Manufacturer.Name == manufacturerCode);

        if (modelYear is not null)
        {
            query = query.Where(model => model.ModelYear == modelYear.Value);
        }

        var models = await query
            .OrderByDescending(model => model.ModelYear)
            .ThenBy(model => model.ModelName)
            .ThenBy(model => model.Trim)
            .Select(model => new
            {
                Code = model.VehicleCode ?? model.ModelName,
                DisplayText = string.IsNullOrWhiteSpace(model.Trim) ? model.ModelName : model.ModelName + " " + model.Trim,
                model.ModelYear,
                ManufacturerCode = model.Manufacturer!.Name,
                model.Trim,
                model.VehicleCode
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<IReadOnlyList<VehicleModelOption>>.Success(models
            .Select((model, index) => new VehicleModelOption(
                model.Code,
                model.DisplayText,
                index + 1,
                model.ModelYear,
                model.ManufacturerCode,
                model.Trim,
                model.VehicleCode))
            .ToList());
    }

    public async Task<QuoteResponse> CreateQuoteAsync(CancellationToken cancellationToken)
    {
        var quote = new QuoteEntity
        {
            ProductType = "Auto",
            Status = "Draft",
            CurrentStep = "Description"
        };

        dbContext.Quotes.Add(quote);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToQuoteResponse(quote);
    }

    public async Task<bool> QuoteExistsAsync(Guid quoteId, CancellationToken cancellationToken)
    {
        return await dbContext.Quotes
            .AsNoTracking()
            .AnyAsync(quote => quote.Id == quoteId, cancellationToken);
    }

    public async Task<IReadOnlyList<QuoteVehicleResponse>> GetVehiclesAsync(
        Guid quoteId,
        Guid? vehicleId,
        string locale,
        CancellationToken cancellationToken)
    {
        var query =
            from vehicle in dbContext.QuoteVehicles.AsNoTracking()
            join manufacturer in dbContext.VehicleManufacturers.AsNoTracking()
                on vehicle.ManufacturerId equals manufacturer.Id
            join model in dbContext.VehicleModels.AsNoTracking()
                on vehicle.VehicleModelId equals model.Id
            join usage in dbContext.QuoteVehicleUsages.AsNoTracking()
                on vehicle.Id equals usage.QuoteVehicleId
            join leaseOption in dbContext.YesNoUnknownOptions.AsNoTracking()
                on vehicle.LeaseStatusId equals leaseOption.Id
            join purchaseCondition in dbContext.PurchaseConditionOptions.AsNoTracking()
                on vehicle.PurchaseConditionId equals purchaseCondition.Id
            join trackingOption in dbContext.YesNoUnknownOptions.AsNoTracking()
                on vehicle.TrackingSystemStatusId equals trackingOption.Id
            join engravingOption in dbContext.YesNoUnknownOptions.AsNoTracking()
                on vehicle.IntensiveEngravingStatusId equals engravingOption.Id
            join modifiedOption in dbContext.YesNoUnknownOptions.AsNoTracking()
                on vehicle.ModifiedAfterManufacturingStatusId equals modifiedOption.Id
            join outsideQuebecOption in dbContext.YesNoUnknownOptions.AsNoTracking()
                on usage.UsedOutsideQuebecStatusId equals outsideQuebecOption.Id
            where vehicle.QuoteId == quoteId
            select new
            {
                Vehicle = vehicle,
                Manufacturer = manufacturer,
                Model = model,
                Usage = usage,
                LeaseOption = leaseOption,
                PurchaseCondition = purchaseCondition,
                TrackingOption = trackingOption,
                EngravingOption = engravingOption,
                ModifiedOption = modifiedOption,
                OutsideQuebecOption = outsideQuebecOption
            };

        if (vehicleId is not null)
        {
            query = query.Where(row => row.Vehicle.Id == vehicleId.Value);
        }

        var rows = await query
            .OrderByDescending(row => row.Vehicle.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return rows
            .Select(row => new QuoteVehicleResponse(
                row.Vehicle.Id,
                row.Vehicle.QuoteId,
                row.Vehicle.ModelYear,
                new LookupOption(row.Manufacturer.Name, row.Manufacturer.Name, 0),
                new VehicleModelOption(
                    row.Model.VehicleCode ?? row.Model.ModelName,
                    string.IsNullOrWhiteSpace(row.Model.Trim) ? row.Model.ModelName : row.Model.ModelName + " " + row.Model.Trim,
                    0,
                    row.Model.ModelYear,
                    row.Manufacturer.Name,
                    row.Model.Trim,
                    row.Model.VehicleCode),
                row.Vehicle.VehicleCode,
                row.Vehicle.PurchaseYear,
                row.Vehicle.PurchaseMonth,
                ToLookupOption(row.LeaseOption, locale),
                ToLookupOption(row.PurchaseCondition, locale),
                ToLookupOption(row.TrackingOption, locale),
                ToLookupOption(row.EngravingOption, locale),
                ToLookupOption(row.ModifiedOption, locale),
                ToLookupOption(row.OutsideQuebecOption, locale),
                row.Usage.CurrentOdometerKm,
                row.Usage.AnnualDistanceKm,
                row.Usage.DriveForBusiness,
                row.Vehicle.CreatedAtUtc,
                row.Vehicle.UpdatedAtUtc))
            .ToList();
    }

    public async Task<ServiceResult<QuoteVehicleResponse>> CreateVehicleAsync(
        Guid quoteId,
        CreateQuoteVehicleRequest request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var ids = await ResolveVehicleLookupIdsAsync(request, cancellationToken);

        if (ids.Errors.Count > 0)
        {
            await transaction.RollbackAsync(cancellationToken);
            return ServiceResult<QuoteVehicleResponse>.Validation(ids.Errors);
        }

        var vehicle = new QuoteVehicleEntity
        {
            QuoteId = quoteId,
            ModelYear = request.ModelYear,
            ManufacturerId = ids.ManufacturerId,
            VehicleModelId = ids.VehicleModelId,
            VehicleCode = request.VehicleModelCode,
            PurchaseYear = request.PurchaseYear,
            PurchaseMonth = request.PurchaseMonth,
            LeaseStatusId = ids.IsLeasedId,
            PurchaseConditionId = ids.PurchaseConditionId,
            TrackingSystemStatusId = ids.TrackingSystemId,
            IntensiveEngravingStatusId = ids.IntensiveEngravingId,
            ModifiedAfterManufacturingStatusId = ids.ModifiedAfterManufacturingId
        };

        dbContext.QuoteVehicles.Add(vehicle);
        await dbContext.SaveChangesAsync(cancellationToken);

        dbContext.QuoteVehicleUsages.Add(new QuoteVehicleUsageEntity
        {
            QuoteVehicleId = vehicle.Id,
            UsedOutsideQuebecStatusId = ids.OutsideQuebecId,
            CurrentOdometerKm = request.CurrentOdometerKm,
            AnnualDistanceKm = request.AnnualDistanceKm,
            DriveForBusiness = request.DriveForBusiness
        });

        dbContext.QuoteStepSubmissions.Add(new QuoteStepSubmissionEntity
        {
            QuoteId = quoteId,
            StepCode = "VehicleConfirmation",
            PayloadJson = JsonSerializer.Serialize(request)
        });

        await dbContext.Quotes
            .Where(quote => quote.Id == quoteId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(quote => quote.Status, "Submitted")
                    .SetProperty(quote => quote.CurrentStep, "Confirmation"),
                cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var savedVehicles = await GetVehiclesAsync(quoteId, vehicle.Id, "fr", cancellationToken);
        return ServiceResult<QuoteVehicleResponse>.Success(savedVehicles.Single());
    }

    public async Task<bool> DeleteVehicleAsync(Guid quoteId, Guid vehicleId, CancellationToken cancellationToken)
    {
        var deletedCount = await dbContext.QuoteVehicles
            .Where(vehicle => vehicle.Id == vehicleId && vehicle.QuoteId == quoteId)
            .ExecuteDeleteAsync(cancellationToken);

        return deletedCount > 0;
    }

    private static List<LookupOption> BuildYearOptions(int firstYear, int count)
    {
        return Enumerable
            .Range(0, count)
            .Select(index =>
            {
                var year = firstYear - index;
                return new LookupOption(year.ToString(), year.ToString(), index + 1);
            })
            .ToList();
    }

    private async Task<List<LookupOption>> ReadMonthOptionsAsync(string locale, CancellationToken cancellationToken)
    {
        var options = await dbContext.MonthOptions
            .AsNoTracking()
            .Where(option => option.IsActive)
            .OrderBy(option => option.SortOrder)
            .ToListAsync(cancellationToken);

        return options.Select(option => ToLookupOption(option, locale)).ToList();
    }

    private async Task<List<LookupOption>> ReadYesNoUnknownOptionsAsync(
        string locale,
        bool includeUnknown,
        CancellationToken cancellationToken)
    {
        var query = dbContext.YesNoUnknownOptions
            .AsNoTracking()
            .Where(option => option.IsActive);

        if (!includeUnknown)
        {
            query = query.Where(option => option.Code == "Yes" || option.Code == "No");
        }

        var options = await query
            .OrderBy(option => option.SortOrder)
            .ToListAsync(cancellationToken);

        return options.Select(option => ToLookupOption(option, locale)).ToList();
    }

    private async Task<List<LookupOption>> ReadPurchaseConditionOptionsAsync(
        string locale,
        CancellationToken cancellationToken)
    {
        var options = await dbContext.PurchaseConditionOptions
            .AsNoTracking()
            .Where(option => option.IsActive)
            .OrderBy(option => option.SortOrder)
            .ToListAsync(cancellationToken);

        return options.Select(option => ToLookupOption(option, locale)).ToList();
    }

    private async Task<List<LookupOption>> ReadDistanceOptionsAsync(
        string locale,
        string optionType,
        CancellationToken cancellationToken)
    {
        var options = await dbContext.DistanceOptions
            .AsNoTracking()
            .Where(option => option.IsActive && option.OptionType == optionType)
            .OrderBy(option => option.SortOrder)
            .ToListAsync(cancellationToken);

        return options
            .Select(option => new LookupOption(
                option.Kilometers.ToString(),
                Localize(option.DisplayText, option.DisplayTextEn, locale),
                option.SortOrder))
            .ToList();
    }

    private async Task<ResolvedVehicleLookupIds> ResolveVehicleLookupIdsAsync(
        CreateQuoteVehicleRequest request,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();

        var manufacturerId = await dbContext.VehicleManufacturers
            .Where(manufacturer => manufacturer.Name == request.ManufacturerCode && manufacturer.IsActive)
            .Select(manufacturer => (int?)manufacturer.Id)
            .SingleOrDefaultAsync(cancellationToken);
        AddMissingLookup(errors, manufacturerId, nameof(request.ManufacturerCode), "Manufacturer was not found.");

        var vehicleModelId = manufacturerId is null
            ? null
            : await dbContext.VehicleModels
                .Where(model =>
                    model.ManufacturerId == manufacturerId.Value
                    && model.ModelYear == request.ModelYear
                    && model.VehicleCode == request.VehicleModelCode
                    && model.IsActive)
                .Select(model => (int?)model.Id)
                .SingleOrDefaultAsync(cancellationToken);
        AddMissingLookup(errors, vehicleModelId, nameof(request.VehicleModelCode), "Vehicle model was not found for the selected manufacturer and year.");

        var isLeasedId = await ResolveYesNoUnknownIdAsync(request.IsLeasedCode, cancellationToken);
        var trackingSystemId = await ResolveYesNoUnknownIdAsync(request.TrackingSystemCode, cancellationToken);
        var intensiveEngravingId = await ResolveYesNoUnknownIdAsync(request.IntensiveEngravingCode, cancellationToken);
        var modifiedAfterManufacturingId = await ResolveYesNoUnknownIdAsync(request.ModifiedAfterManufacturingCode, cancellationToken);
        var outsideQuebecId = await ResolveYesNoUnknownIdAsync(request.OutsideOfProvinceForPersonalUseCode, cancellationToken);
        var purchaseConditionId = await dbContext.PurchaseConditionOptions
            .Where(option => option.Code == request.PurchaseConditionCode && option.IsActive)
            .Select(option => (int?)option.Id)
            .SingleOrDefaultAsync(cancellationToken);

        var missingLookupErrors = new (int? Id, string FieldName, string Message)[]
            {
                (isLeasedId, nameof(request.IsLeasedCode), "Lease option was not found."),
                (trackingSystemId, nameof(request.TrackingSystemCode), "Tracking system option was not found."),
                (intensiveEngravingId, nameof(request.IntensiveEngravingCode), "Intensive engraving option was not found."),
                (modifiedAfterManufacturingId, nameof(request.ModifiedAfterManufacturingCode), "Modified after manufacturing option was not found."),
                (outsideQuebecId, nameof(request.OutsideOfProvinceForPersonalUseCode), "Province use option was not found."),
                (purchaseConditionId, nameof(request.PurchaseConditionCode), "Purchase condition option was not found.")
            }
            .Where(lookup => lookup.Id is null)
            .ToDictionary(
                lookup => lookup.FieldName,
                lookup => new[] { lookup.Message });

        errors = errors
            .Concat(missingLookupErrors)
            .ToDictionary(error => error.Key, error => error.Value);

        if (!await DistanceOptionExistsAsync(request.CurrentOdometerKm, "CurrentOdometer", cancellationToken))
        {
            errors[nameof(request.CurrentOdometerKm)] = ["Current odometer option was not found."];
        }

        if (!await DistanceOptionExistsAsync(request.AnnualDistanceKm, "AnnualDistance", cancellationToken))
        {
            errors[nameof(request.AnnualDistanceKm)] = ["Annual distance option was not found."];
        }

        return errors.Count > 0
            ? ResolvedVehicleLookupIds.Invalid(errors)
            : new ResolvedVehicleLookupIds(
                manufacturerId!.Value,
                vehicleModelId!.Value,
                isLeasedId!.Value,
                purchaseConditionId!.Value,
                trackingSystemId!.Value,
                intensiveEngravingId!.Value,
                modifiedAfterManufacturingId!.Value,
                outsideQuebecId!.Value,
                errors);
    }

    private async Task<int?> ResolveYesNoUnknownIdAsync(string code, CancellationToken cancellationToken)
    {
        return await dbContext.YesNoUnknownOptions
            .Where(option => option.Code == code && option.IsActive)
            .Select(option => (int?)option.Id)
            .SingleOrDefaultAsync(cancellationToken);
    }

    private async Task<bool> DistanceOptionExistsAsync(
        int kilometers,
        string optionType,
        CancellationToken cancellationToken)
    {
        return await dbContext.DistanceOptions
            .AnyAsync(
                option =>
                    option.Kilometers == kilometers
                    && option.OptionType == optionType
                    && option.IsActive,
                cancellationToken);
    }

    private static QuoteResponse ToQuoteResponse(QuoteEntity quote)
    {
        return new QuoteResponse(
            quote.Id,
            quote.ProductType,
            quote.Status,
            quote.CurrentStep,
            quote.CreatedAtUtc,
            quote.UpdatedAtUtc);
    }

    private static LookupOption ToLookupOption(YesNoUnknownOptionEntity option, string locale)
    {
        return new LookupOption(
            option.Code,
            Localize(option.DisplayText, option.DisplayTextEn, locale),
            option.SortOrder);
    }

    private static LookupOption ToLookupOption(PurchaseConditionOptionEntity option, string locale)
    {
        return new LookupOption(
            option.Code,
            Localize(option.DisplayText, option.DisplayTextEn, locale),
            option.SortOrder);
    }

    private static LookupOption ToLookupOption(MonthOptionEntity option, string locale)
    {
        return new LookupOption(
            option.Code,
            Localize(option.DisplayText, option.DisplayTextEn, locale),
            option.SortOrder);
    }

    private static string Localize(string displayText, string? displayTextEn, string locale)
    {
        return locale == "en" && !string.IsNullOrWhiteSpace(displayTextEn) ? displayTextEn : displayText;
    }

    private static void AddMissingLookup(Dictionary<string, string[]> errors, int? id, string fieldName, string message)
    {
        if (id is null)
        {
            errors[fieldName] = [message];
        }
    }

    private sealed record ResolvedVehicleLookupIds(
        int ManufacturerId,
        int VehicleModelId,
        int IsLeasedId,
        int PurchaseConditionId,
        int TrackingSystemId,
        int IntensiveEngravingId,
        int ModifiedAfterManufacturingId,
        int OutsideQuebecId,
        Dictionary<string, string[]> Errors)
    {
        public static ResolvedVehicleLookupIds Invalid(Dictionary<string, string[]> errors)
        {
            return new ResolvedVehicleLookupIds(0, 0, 0, 0, 0, 0, 0, 0, errors);
        }
    }
}
