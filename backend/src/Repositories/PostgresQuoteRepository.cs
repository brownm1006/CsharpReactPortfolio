using System.Data;
using System.Text.Json;
using Npgsql;
using PortfolioClubAssurance.Api.Data;
using PortfolioClubAssurance.Api.Dtos.Lookups;
using PortfolioClubAssurance.Api.Dtos.Requests;
using PortfolioClubAssurance.Api.Dtos.Responses;
using PortfolioClubAssurance.Api.Services.Common;

namespace PortfolioClubAssurance.Api.Repositories;

public sealed class PostgresQuoteRepository : IQuoteRepository
{
    private readonly INpgsqlConnectionFactory connectionFactory;

    public PostgresQuoteRepository(INpgsqlConnectionFactory connectionFactory)
    {
        this.connectionFactory = connectionFactory;
    }

    public async Task<DatabaseHealthResponse> GetDatabaseHealthAsync(CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(
            """
            select count(*)
            from information_schema.tables
            where table_schema = 'quote';
            """,
            connection);

        var tableCount = (long)(await command.ExecuteScalarAsync(cancellationToken) ?? 0L);
        return new DatabaseHealthResponse("ok", "quote", tableCount);
    }

    public async Task<VehicleDescriptionLookupsResponse> GetVehicleDescriptionLookupsAsync(
        string locale,
        CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);

        var modelYears = BuildYearOptions(DateTime.UtcNow.Year + 1, 8);
        var purchaseYears = BuildYearOptions(DateTime.UtcNow.Year, 15);
        var months = await ReadLookupOptionsAsync(
            connection,
            """
            select code, display_text, display_text_en, sort_order
            from quote.month_options
            where is_active
            order by sort_order;
            """,
            locale,
            cancellationToken);
        var manufacturers = await ReadLookupOptionsAsync(
            connection,
            """
            select name, name, name, id
            from quote.vehicle_manufacturers
            where is_active
            order by name;
            """,
            locale,
            cancellationToken);
        var yesNoUnknown = await ReadYesNoUnknownOptionsAsync(connection, locale, includeUnknown: true, cancellationToken);
        var purchaseConditions = await ReadPurchaseConditionOptionsAsync(connection, locale, cancellationToken);

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
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);

        var provinceUse = await ReadYesNoUnknownOptionsAsync(connection, locale, includeUnknown: true, cancellationToken);
        var odometer = await ReadDistanceOptionsAsync(connection, locale, "CurrentOdometer", cancellationToken);
        var annualDistance = await ReadDistanceOptionsAsync(connection, locale, "AnnualDistance", cancellationToken);
        var yesNo = await ReadYesNoUnknownOptionsAsync(connection, locale, includeUnknown: false, cancellationToken);

        return new VehicleUsageLookupsResponse(provinceUse, odometer, annualDistance, yesNo);
    }

    public async Task<IReadOnlyList<LookupOption>> GetYesNoUnknownOptionsAsync(string locale, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await ReadYesNoUnknownOptionsAsync(connection, locale, includeUnknown: true, cancellationToken);
    }

    public async Task<IReadOnlyList<LookupOption>> GetPurchaseConditionOptionsAsync(string locale, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await ReadPurchaseConditionOptionsAsync(connection, locale, cancellationToken);
    }

    public async Task<IReadOnlyList<LookupOption>> GetManufacturersAsync(CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await ReadLookupOptionsAsync(
            connection,
            """
            select name, name, name, id
            from quote.vehicle_manufacturers
            where is_active
            order by name;
            """,
            "fr",
            cancellationToken);
    }

    public async Task<ServiceResult<IReadOnlyList<VehicleModelOption>>> GetModelsAsync(
        string manufacturerCode,
        int? modelYear,
        CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);

        if (!await ManufacturerExistsAsync(connection, manufacturerCode, cancellationToken))
        {
            return ServiceResult<IReadOnlyList<VehicleModelOption>>.NotFound("Manufacturer not found.");
        }

        var commandText = modelYear is null
            ? """
              select
                  coalesce(vehicle_code, model_name) as code,
                  concat_ws(' ', model_name, trim) as display_text,
                  row_number() over (order by model_name, trim, model_year)::integer as sort_order,
                  model_year,
                  manufacturer.name as manufacturer_code,
                  trim,
                  vehicle_code
              from quote.vehicle_models model
              join quote.vehicle_manufacturers manufacturer
                  on manufacturer.id = model.manufacturer_id
              where model.is_active
                and manufacturer.is_active
                and manufacturer.name = @manufacturerCode
              order by model_year desc, model_name, trim;
              """
            : """
              select
                  coalesce(vehicle_code, model_name) as code,
                  concat_ws(' ', model_name, trim) as display_text,
                  row_number() over (order by model_name, trim)::integer as sort_order,
                  model_year,
                  manufacturer.name as manufacturer_code,
                  trim,
                  vehicle_code
              from quote.vehicle_models model
              join quote.vehicle_manufacturers manufacturer
                  on manufacturer.id = model.manufacturer_id
              where model.is_active
                and manufacturer.is_active
                and manufacturer.name = @manufacturerCode
                and model.model_year = @modelYear
              order by model_name, trim;
              """;

        await using var command = new NpgsqlCommand(commandText, connection);
        command.Parameters.AddWithValue("manufacturerCode", manufacturerCode);

        if (modelYear is not null)
        {
            command.Parameters.AddWithValue("modelYear", modelYear.Value);
        }

        var models = new List<VehicleModelOption>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            models.Add(new VehicleModelOption(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetInt32(2),
                reader.GetInt32(3),
                reader.GetString(4),
                reader.IsDBNull(5) ? null : reader.GetString(5),
                reader.IsDBNull(6) ? null : reader.GetString(6)));
        }

        return ServiceResult<IReadOnlyList<VehicleModelOption>>.Success(models);
    }

    public async Task<QuoteResponse> CreateQuoteAsync(CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(
            """
            insert into quote.quotes (product_type, status, current_step)
            values ('Auto', 'Draft', 'Description')
            returning id, product_type, status, current_step, created_at_utc, updated_at_utc;
            """,
            connection);

        return await ReadSingleQuoteResponseAsync(command, cancellationToken);
    }

    public async Task<bool> QuoteExistsAsync(Guid quoteId, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await QuoteExistsAsync(connection, quoteId, cancellationToken);
    }

    public async Task<IReadOnlyList<QuoteVehicleResponse>> GetVehiclesAsync(
        Guid quoteId,
        Guid? vehicleId,
        string locale,
        CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await ReadQuoteVehiclesAsync(connection, quoteId, vehicleId, locale, cancellationToken);
    }

    public async Task<ServiceResult<QuoteVehicleResponse>> CreateVehicleAsync(
        Guid quoteId,
        CreateQuoteVehicleRequest request,
        CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var ids = await ResolveVehicleLookupIdsAsync(connection, transaction, request, cancellationToken);

        if (ids.Errors.Count > 0)
        {
            await transaction.RollbackAsync(cancellationToken);
            return ServiceResult<QuoteVehicleResponse>.Validation(ids.Errors);
        }

        Guid vehicleId;

        await using (var insertVehicle = new NpgsqlCommand(
            """
            insert into quote.quote_vehicles
            (
                quote_id,
                model_year,
                manufacturer_id,
                vehicle_model_id,
                vehicle_code,
                purchase_year,
                purchase_month,
                lease_status_id,
                purchase_condition_id,
                tracking_system_status_id,
                intensive_engraving_status_id,
                modified_after_manufacturing_status_id
            )
            values
            (
                @quoteId,
                @modelYear,
                @manufacturerId,
                @vehicleModelId,
                @vehicleCode,
                @purchaseYear,
                @purchaseMonth,
                @leaseStatusId,
                @purchaseConditionId,
                @trackingSystemStatusId,
                @intensiveEngravingStatusId,
                @modifiedAfterManufacturingStatusId
            )
            returning id;
            """,
            connection,
            transaction))
        {
            insertVehicle.Parameters.AddWithValue("quoteId", quoteId);
            insertVehicle.Parameters.AddWithValue("modelYear", request.ModelYear);
            insertVehicle.Parameters.AddWithValue("manufacturerId", ids.ManufacturerId);
            insertVehicle.Parameters.AddWithValue("vehicleModelId", ids.VehicleModelId);
            insertVehicle.Parameters.AddWithValue("vehicleCode", request.VehicleModelCode);
            insertVehicle.Parameters.AddWithValue("purchaseYear", request.PurchaseYear);
            insertVehicle.Parameters.AddWithValue("purchaseMonth", request.PurchaseMonth);
            insertVehicle.Parameters.AddWithValue("leaseStatusId", ids.IsLeasedId);
            insertVehicle.Parameters.AddWithValue("purchaseConditionId", ids.PurchaseConditionId);
            insertVehicle.Parameters.AddWithValue("trackingSystemStatusId", ids.TrackingSystemId);
            insertVehicle.Parameters.AddWithValue("intensiveEngravingStatusId", ids.IntensiveEngravingId);
            insertVehicle.Parameters.AddWithValue("modifiedAfterManufacturingStatusId", ids.ModifiedAfterManufacturingId);

            vehicleId = (Guid)(await insertVehicle.ExecuteScalarAsync(cancellationToken)
                ?? throw new InvalidOperationException("The vehicle insert did not return an id."));
        }

        await using (var insertUsage = new NpgsqlCommand(
            """
            insert into quote.quote_vehicle_usages
            (
                quote_vehicle_id,
                used_outside_quebec_status_id,
                current_odometer_km,
                annual_distance_km,
                drive_for_business
            )
            values
            (
                @vehicleId,
                @outsideQuebecStatusId,
                @currentOdometerKm,
                @annualDistanceKm,
                @driveForBusiness
            );
            """,
            connection,
            transaction))
        {
            insertUsage.Parameters.AddWithValue("vehicleId", vehicleId);
            insertUsage.Parameters.AddWithValue("outsideQuebecStatusId", ids.OutsideQuebecId);
            insertUsage.Parameters.AddWithValue("currentOdometerKm", request.CurrentOdometerKm);
            insertUsage.Parameters.AddWithValue("annualDistanceKm", request.AnnualDistanceKm);
            insertUsage.Parameters.AddWithValue("driveForBusiness", request.DriveForBusiness);
            await insertUsage.ExecuteNonQueryAsync(cancellationToken);
        }

        await using (var insertSubmission = new NpgsqlCommand(
            """
            insert into quote.quote_step_submissions (quote_id, step_code, payload_json)
            values (@quoteId, 'VehicleConfirmation', @payloadJson::jsonb);
            """,
            connection,
            transaction))
        {
            insertSubmission.Parameters.AddWithValue("quoteId", quoteId);
            insertSubmission.Parameters.AddWithValue("payloadJson", JsonSerializer.Serialize(request));
            await insertSubmission.ExecuteNonQueryAsync(cancellationToken);
        }

        await using (var updateQuote = new NpgsqlCommand(
            """
            update quote.quotes
            set status = 'Submitted',
                current_step = 'Confirmation'
            where id = @quoteId;
            """,
            connection,
            transaction))
        {
            updateQuote.Parameters.AddWithValue("quoteId", quoteId);
            await updateQuote.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);

        var savedVehicles = await ReadQuoteVehiclesAsync(connection, quoteId, vehicleId, "fr", cancellationToken);
        return ServiceResult<QuoteVehicleResponse>.Success(savedVehicles.Single());
    }

    public async Task<bool> DeleteVehicleAsync(Guid quoteId, Guid vehicleId, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(
            """
            delete from quote.quote_vehicles
            where id = @vehicleId
              and quote_id = @quoteId;
            """,
            connection);
        command.Parameters.AddWithValue("vehicleId", vehicleId);
        command.Parameters.AddWithValue("quoteId", quoteId);

        var deletedCount = await command.ExecuteNonQueryAsync(cancellationToken);
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

    private static async Task<List<LookupOption>> ReadYesNoUnknownOptionsAsync(
        NpgsqlConnection connection,
        string locale,
        bool includeUnknown,
        CancellationToken cancellationToken)
    {
        var whereUnknown = includeUnknown ? "" : "and code in ('Yes', 'No')";

        return await ReadLookupOptionsAsync(
            connection,
            $"""
            select code, display_text, display_text_en, sort_order
            from quote.yes_no_unknown_options
            where is_active
            {whereUnknown}
            order by sort_order;
            """,
            locale,
            cancellationToken);
    }

    private static async Task<List<LookupOption>> ReadPurchaseConditionOptionsAsync(
        NpgsqlConnection connection,
        string locale,
        CancellationToken cancellationToken)
    {
        return await ReadLookupOptionsAsync(
            connection,
            """
            select code, display_text, display_text_en, sort_order
            from quote.purchase_condition_options
            where is_active
            order by sort_order;
            """,
            locale,
            cancellationToken);
    }

    private static async Task<List<LookupOption>> ReadDistanceOptionsAsync(
        NpgsqlConnection connection,
        string locale,
        string optionType,
        CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(
            """
            select kilometers::text, display_text, display_text_en, sort_order
            from quote.distance_options
            where is_active
              and option_type = @optionType
            order by sort_order;
            """,
            connection);
        command.Parameters.AddWithValue("optionType", optionType);

        return await ReadLookupOptionsFromCommandAsync(command, locale, cancellationToken);
    }

    private static async Task<List<LookupOption>> ReadLookupOptionsAsync(
        NpgsqlConnection connection,
        string commandText,
        string locale,
        CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(commandText, connection);
        return await ReadLookupOptionsFromCommandAsync(command, locale, cancellationToken);
    }

    private static async Task<List<LookupOption>> ReadLookupOptionsFromCommandAsync(
        NpgsqlCommand command,
        string locale,
        CancellationToken cancellationToken)
    {
        var options = new List<LookupOption>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var code = reader.GetString(0);
            var displayText = reader.GetString(1);
            var displayTextEn = reader.IsDBNull(2) ? null : reader.GetString(2);
            var sortOrder = reader.GetInt32(3);

            options.Add(new LookupOption(
                code,
                locale == "en" && !string.IsNullOrWhiteSpace(displayTextEn) ? displayTextEn : displayText,
                sortOrder));
        }

        return options;
    }

    private static async Task<bool> QuoteExistsAsync(NpgsqlConnection connection, Guid quoteId, CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(
            "select exists(select 1 from quote.quotes where id = @quoteId);",
            connection);
        command.Parameters.AddWithValue("quoteId", quoteId);

        return (bool)(await command.ExecuteScalarAsync(cancellationToken) ?? false);
    }

    private static async Task<bool> ManufacturerExistsAsync(
        NpgsqlConnection connection,
        string manufacturerCode,
        CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(
            """
            select exists(
                select 1
                from quote.vehicle_manufacturers
                where name = @manufacturerCode
                  and is_active
            );
            """,
            connection);
        command.Parameters.AddWithValue("manufacturerCode", manufacturerCode);

        return (bool)(await command.ExecuteScalarAsync(cancellationToken) ?? false);
    }

    private static async Task<QuoteResponse> ReadSingleQuoteResponseAsync(
        NpgsqlCommand command,
        CancellationToken cancellationToken)
    {
        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException("The quote query returned no rows.");
        }

        return new QuoteResponse(
            reader.GetGuid(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.IsDBNull(3) ? null : reader.GetString(3),
            reader.GetDateTime(4),
            reader.GetDateTime(5));
    }

    private static async Task<ResolvedVehicleLookupIds> ResolveVehicleLookupIdsAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        CreateQuoteVehicleRequest request,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();

        var manufacturerId = await ResolveIdAsync(
            connection,
            transaction,
            """
            select id
            from quote.vehicle_manufacturers
            where name = @code
              and is_active;
            """,
            request.ManufacturerCode,
            cancellationToken);
        AddMissingLookup(errors, manufacturerId, nameof(request.ManufacturerCode), "Manufacturer was not found.");

        var vehicleModelId = manufacturerId is null
            ? null
            : await ResolveIdAsync(
                connection,
                transaction,
                """
                select id
                from quote.vehicle_models
                where manufacturer_id = @manufacturerId
                  and model_year = @modelYear
                  and vehicle_code = @code
                  and is_active;
                """,
                request.VehicleModelCode,
                cancellationToken,
                ("manufacturerId", manufacturerId.Value),
                ("modelYear", request.ModelYear));
        AddMissingLookup(errors, vehicleModelId, nameof(request.VehicleModelCode), "Vehicle model was not found for the selected manufacturer and year.");

        var isLeasedId = await ResolveYesNoUnknownIdAsync(connection, transaction, request.IsLeasedCode, cancellationToken);
        var trackingSystemId = await ResolveYesNoUnknownIdAsync(connection, transaction, request.TrackingSystemCode, cancellationToken);
        var intensiveEngravingId = await ResolveYesNoUnknownIdAsync(connection, transaction, request.IntensiveEngravingCode, cancellationToken);
        var modifiedAfterManufacturingId = await ResolveYesNoUnknownIdAsync(connection, transaction, request.ModifiedAfterManufacturingCode, cancellationToken);
        var outsideQuebecId = await ResolveYesNoUnknownIdAsync(connection, transaction, request.OutsideOfProvinceForPersonalUseCode, cancellationToken);
        var purchaseConditionId = await ResolveIdAsync(
            connection,
            transaction,
            """
            select id
            from quote.purchase_condition_options
            where code = @code
              and is_active;
            """,
            request.PurchaseConditionCode,
            cancellationToken);

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

        if (!await DistanceOptionExistsAsync(connection, transaction, request.CurrentOdometerKm, "CurrentOdometer", cancellationToken))
        {
            errors[nameof(request.CurrentOdometerKm)] = ["Current odometer option was not found."];
        }

        if (!await DistanceOptionExistsAsync(connection, transaction, request.AnnualDistanceKm, "AnnualDistance", cancellationToken))
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

    private static async Task<int?> ResolveYesNoUnknownIdAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        string code,
        CancellationToken cancellationToken)
    {
        return await ResolveIdAsync(
            connection,
            transaction,
            """
            select id
            from quote.yes_no_unknown_options
            where code = @code
              and is_active;
            """,
            code,
            cancellationToken);
    }

    private static async Task<int?> ResolveIdAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        string commandText,
        string code,
        CancellationToken cancellationToken,
        params (string Name, object Value)[] additionalParameters)
    {
        await using var command = new NpgsqlCommand(commandText, connection, transaction);
        command.Parameters.AddWithValue("code", code);

        foreach (var (name, value) in additionalParameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is null ? null : (int)result;
    }

    private static async Task<bool> DistanceOptionExistsAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        int kilometers,
        string optionType,
        CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(
            """
            select exists(
                select 1
                from quote.distance_options
                where kilometers = @kilometers
                  and option_type = @optionType
                  and is_active
            );
            """,
            connection,
            transaction);
        command.Parameters.AddWithValue("kilometers", kilometers);
        command.Parameters.AddWithValue("optionType", optionType);

        return (bool)(await command.ExecuteScalarAsync(cancellationToken) ?? false);
    }

    private static void AddMissingLookup(Dictionary<string, string[]> errors, int? id, string fieldName, string message)
    {
        if (id is null)
        {
            errors[fieldName] = [message];
        }
    }

    private static async Task<IReadOnlyList<QuoteVehicleResponse>> ReadQuoteVehiclesAsync(
        NpgsqlConnection connection,
        Guid quoteId,
        Guid? vehicleId,
        string locale,
        CancellationToken cancellationToken)
    {
        var vehicleFilter = vehicleId is null ? "" : "and vehicle.id = @vehicleId";

        await using var command = new NpgsqlCommand(
            $"""
            select
                vehicle.id,
                vehicle.quote_id,
                vehicle.model_year,
                manufacturer.name as manufacturer_code,
                manufacturer.name as manufacturer_display_text,
                model.vehicle_code as vehicle_model_code,
                concat_ws(' ', model.model_name, model.trim) as vehicle_model_display_text,
                model.model_year,
                manufacturer.name as model_manufacturer_code,
                model.trim,
                model.vehicle_code,
                vehicle.vehicle_code,
                vehicle.purchase_year,
                vehicle.purchase_month,
                lease_option.code,
                lease_option.display_text,
                lease_option.display_text_en,
                lease_option.sort_order,
                purchase_condition.code,
                purchase_condition.display_text,
                purchase_condition.display_text_en,
                purchase_condition.sort_order,
                tracking_option.code,
                tracking_option.display_text,
                tracking_option.display_text_en,
                tracking_option.sort_order,
                engraving_option.code,
                engraving_option.display_text,
                engraving_option.display_text_en,
                engraving_option.sort_order,
                modified_option.code,
                modified_option.display_text,
                modified_option.display_text_en,
                modified_option.sort_order,
                outside_quebec_option.code,
                outside_quebec_option.display_text,
                outside_quebec_option.display_text_en,
                outside_quebec_option.sort_order,
                usage.current_odometer_km,
                usage.annual_distance_km,
                usage.drive_for_business,
                vehicle.created_at_utc,
                vehicle.updated_at_utc
            from quote.quote_vehicles vehicle
            join quote.vehicle_manufacturers manufacturer
                on manufacturer.id = vehicle.manufacturer_id
            join quote.vehicle_models model
                on model.id = vehicle.vehicle_model_id
            join quote.yes_no_unknown_options lease_option
                on lease_option.id = vehicle.lease_status_id
            join quote.purchase_condition_options purchase_condition
                on purchase_condition.id = vehicle.purchase_condition_id
            join quote.yes_no_unknown_options tracking_option
                on tracking_option.id = vehicle.tracking_system_status_id
            join quote.yes_no_unknown_options engraving_option
                on engraving_option.id = vehicle.intensive_engraving_status_id
            join quote.yes_no_unknown_options modified_option
                on modified_option.id = vehicle.modified_after_manufacturing_status_id
            join quote.quote_vehicle_usages usage
                on usage.quote_vehicle_id = vehicle.id
            join quote.yes_no_unknown_options outside_quebec_option
                on outside_quebec_option.id = usage.used_outside_quebec_status_id
            where vehicle.quote_id = @quoteId
            {vehicleFilter}
            order by vehicle.created_at_utc desc;
            """,
            connection);
        command.Parameters.AddWithValue("quoteId", quoteId);

        if (vehicleId is not null)
        {
            command.Parameters.AddWithValue("vehicleId", vehicleId.Value);
        }

        var vehicles = new List<QuoteVehicleResponse>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            vehicles.Add(new QuoteVehicleResponse(
                reader.GetGuid(0),
                reader.GetGuid(1),
                reader.GetInt32(2),
                new LookupOption(reader.GetString(3), reader.GetString(4), 0),
                new VehicleModelOption(
                    reader.GetString(5),
                    reader.GetString(6),
                    0,
                    reader.GetInt32(7),
                    reader.GetString(8),
                    reader.IsDBNull(9) ? null : reader.GetString(9),
                    reader.IsDBNull(10) ? null : reader.GetString(10)),
                reader.IsDBNull(11) ? null : reader.GetString(11),
                reader.GetInt32(12),
                reader.GetInt32(13),
                ReadLookupOption(reader, 14, locale),
                ReadLookupOption(reader, 18, locale),
                ReadLookupOption(reader, 22, locale),
                ReadLookupOption(reader, 26, locale),
                ReadLookupOption(reader, 30, locale),
                ReadLookupOption(reader, 34, locale),
                reader.GetInt32(38),
                reader.GetInt32(39),
                reader.GetBoolean(40),
                reader.GetDateTime(41),
                reader.GetDateTime(42)));
        }

        return vehicles;
    }

    private static LookupOption ReadLookupOption(NpgsqlDataReader reader, int startIndex, string locale)
    {
        var code = reader.GetString(startIndex);
        var displayText = reader.GetString(startIndex + 1);
        var displayTextEn = reader.IsDBNull(startIndex + 2) ? null : reader.GetString(startIndex + 2);
        var sortOrder = reader.GetInt32(startIndex + 3);

        return new LookupOption(
            code,
            locale == "en" && !string.IsNullOrWhiteSpace(displayTextEn) ? displayTextEn : displayText,
            sortOrder);
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
