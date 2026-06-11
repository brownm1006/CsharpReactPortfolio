using Microsoft.EntityFrameworkCore;
using PortfolioClubAssurance.Api.Entities;

namespace PortfolioClubAssurance.Api.Data;

public sealed class QuoteDbContext : DbContext
{
    public QuoteDbContext(DbContextOptions<QuoteDbContext> options)
        : base(options)
    {
    }

    public DbSet<QuoteEntity> Quotes => Set<QuoteEntity>();

    public DbSet<QuoteVehicleEntity> QuoteVehicles => Set<QuoteVehicleEntity>();

    public DbSet<QuoteVehicleUsageEntity> QuoteVehicleUsages => Set<QuoteVehicleUsageEntity>();

    public DbSet<QuoteStepSubmissionEntity> QuoteStepSubmissions => Set<QuoteStepSubmissionEntity>();

    public DbSet<VehicleManufacturerEntity> VehicleManufacturers => Set<VehicleManufacturerEntity>();

    public DbSet<VehicleModelEntity> VehicleModels => Set<VehicleModelEntity>();

    public DbSet<YesNoUnknownOptionEntity> YesNoUnknownOptions => Set<YesNoUnknownOptionEntity>();

    public DbSet<PurchaseConditionOptionEntity> PurchaseConditionOptions => Set<PurchaseConditionOptionEntity>();

    public DbSet<MonthOptionEntity> MonthOptions => Set<MonthOptionEntity>();

    public DbSet<DistanceOptionEntity> DistanceOptions => Set<DistanceOptionEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("quote");

        modelBuilder.Entity<QuoteEntity>(entity =>
        {
            entity.ToTable("quotes");
            entity.HasKey(quote => quote.Id);

            entity.Property(quote => quote.Id).HasColumnName("id");
            entity.Property(quote => quote.ProductType).HasColumnName("product_type");
            entity.Property(quote => quote.Status).HasColumnName("status");
            entity.Property(quote => quote.CurrentStep).HasColumnName("current_step");
            entity.Property(quote => quote.CreatedAtUtc).HasColumnName("created_at_utc");
            entity.Property(quote => quote.UpdatedAtUtc).HasColumnName("updated_at_utc");

            entity
                .HasMany(quote => quote.Vehicles)
                .WithOne(vehicle => vehicle.Quote)
                .HasForeignKey(vehicle => vehicle.QuoteId);

            entity
                .HasMany(quote => quote.StepSubmissions)
                .WithOne(submission => submission.Quote)
                .HasForeignKey(submission => submission.QuoteId);
        });

        modelBuilder.Entity<QuoteVehicleEntity>(entity =>
        {
            entity.ToTable("quote_vehicles");
            entity.HasKey(vehicle => vehicle.Id);

            entity.Property(vehicle => vehicle.Id).HasColumnName("id");
            entity.Property(vehicle => vehicle.QuoteId).HasColumnName("quote_id");
            entity.Property(vehicle => vehicle.ModelYear).HasColumnName("model_year");
            entity.Property(vehicle => vehicle.ManufacturerId).HasColumnName("manufacturer_id");
            entity.Property(vehicle => vehicle.VehicleModelId).HasColumnName("vehicle_model_id");
            entity.Property(vehicle => vehicle.VehicleCode).HasColumnName("vehicle_code");
            entity.Property(vehicle => vehicle.PurchaseYear).HasColumnName("purchase_year");
            entity.Property(vehicle => vehicle.PurchaseMonth).HasColumnName("purchase_month");
            entity.Property(vehicle => vehicle.LeaseStatusId).HasColumnName("lease_status_id");
            entity.Property(vehicle => vehicle.PurchaseConditionId).HasColumnName("purchase_condition_id");
            entity.Property(vehicle => vehicle.TrackingSystemStatusId).HasColumnName("tracking_system_status_id");
            entity.Property(vehicle => vehicle.IntensiveEngravingStatusId).HasColumnName("intensive_engraving_status_id");
            entity.Property(vehicle => vehicle.ModifiedAfterManufacturingStatusId).HasColumnName("modified_after_manufacturing_status_id");
            entity.Property(vehicle => vehicle.CreatedAtUtc).HasColumnName("created_at_utc");
            entity.Property(vehicle => vehicle.UpdatedAtUtc).HasColumnName("updated_at_utc");

            entity
                .HasOne(vehicle => vehicle.Manufacturer)
                .WithMany(manufacturer => manufacturer.QuoteVehicles)
                .HasForeignKey(vehicle => vehicle.ManufacturerId);

            entity
                .HasOne(vehicle => vehicle.VehicleModel)
                .WithMany(model => model.QuoteVehicles)
                .HasForeignKey(vehicle => vehicle.VehicleModelId);

            entity
                .HasOne(vehicle => vehicle.Usage)
                .WithOne(usage => usage.QuoteVehicle)
                .HasForeignKey<QuoteVehicleUsageEntity>(usage => usage.QuoteVehicleId);
        });

        modelBuilder.Entity<QuoteVehicleUsageEntity>(entity =>
        {
            entity.ToTable("quote_vehicle_usages");
            entity.HasKey(usage => usage.QuoteVehicleId);

            entity.Property(usage => usage.QuoteVehicleId).HasColumnName("quote_vehicle_id");
            entity.Property(usage => usage.UsedOutsideQuebecStatusId).HasColumnName("used_outside_quebec_status_id");
            entity.Property(usage => usage.CurrentOdometerKm).HasColumnName("current_odometer_km");
            entity.Property(usage => usage.AnnualDistanceKm).HasColumnName("annual_distance_km");
            entity.Property(usage => usage.DriveForBusiness).HasColumnName("drive_for_business");
        });

        modelBuilder.Entity<QuoteStepSubmissionEntity>(entity =>
        {
            entity.ToTable("quote_step_submissions");
            entity.HasKey(submission => submission.Id);

            entity.Property(submission => submission.Id).HasColumnName("id");
            entity.Property(submission => submission.QuoteId).HasColumnName("quote_id");
            entity.Property(submission => submission.StepCode).HasColumnName("step_code");
            entity.Property(submission => submission.PayloadJson).HasColumnName("payload_json").HasColumnType("jsonb");
            entity.Property(submission => submission.CreatedAtUtc).HasColumnName("created_at_utc");
        });

        modelBuilder.Entity<VehicleManufacturerEntity>(entity =>
        {
            entity.ToTable("vehicle_manufacturers");
            entity.HasKey(manufacturer => manufacturer.Id);

            entity.Property(manufacturer => manufacturer.Id).HasColumnName("id");
            entity.Property(manufacturer => manufacturer.Name).HasColumnName("name");
            entity.Property(manufacturer => manufacturer.IsActive).HasColumnName("is_active");
        });

        modelBuilder.Entity<VehicleModelEntity>(entity =>
        {
            entity.ToTable("vehicle_models");
            entity.HasKey(model => model.Id);

            entity.Property(model => model.Id).HasColumnName("id");
            entity.Property(model => model.ManufacturerId).HasColumnName("manufacturer_id");
            entity.Property(model => model.ModelYear).HasColumnName("model_year");
            entity.Property(model => model.ModelName).HasColumnName("model_name");
            entity.Property(model => model.Trim).HasColumnName("trim");
            entity.Property(model => model.VehicleCode).HasColumnName("vehicle_code");
            entity.Property(model => model.IsActive).HasColumnName("is_active");

            entity
                .HasOne(model => model.Manufacturer)
                .WithMany(manufacturer => manufacturer.Models)
                .HasForeignKey(model => model.ManufacturerId);
        });

        modelBuilder.Entity<YesNoUnknownOptionEntity>(entity =>
        {
            entity.ToTable("yes_no_unknown_options");
            entity.HasKey(option => option.Id);

            entity.Property(option => option.Id).HasColumnName("id");
            entity.Property(option => option.Code).HasColumnName("code");
            entity.Property(option => option.DisplayText).HasColumnName("display_text");
            entity.Property(option => option.DisplayTextEn).HasColumnName("display_text_en");
            entity.Property(option => option.SortOrder).HasColumnName("sort_order");
            entity.Property(option => option.IsActive).HasColumnName("is_active");
        });

        modelBuilder.Entity<PurchaseConditionOptionEntity>(entity =>
        {
            entity.ToTable("purchase_condition_options");
            entity.HasKey(option => option.Id);

            entity.Property(option => option.Id).HasColumnName("id");
            entity.Property(option => option.Code).HasColumnName("code");
            entity.Property(option => option.DisplayText).HasColumnName("display_text");
            entity.Property(option => option.DisplayTextEn).HasColumnName("display_text_en");
            entity.Property(option => option.SortOrder).HasColumnName("sort_order");
            entity.Property(option => option.IsActive).HasColumnName("is_active");
        });

        modelBuilder.Entity<MonthOptionEntity>(entity =>
        {
            entity.ToTable("month_options");
            entity.HasKey(option => option.Id);

            entity.Property(option => option.Id).HasColumnName("id");
            entity.Property(option => option.Code).HasColumnName("code");
            entity.Property(option => option.DisplayText).HasColumnName("display_text");
            entity.Property(option => option.DisplayTextEn).HasColumnName("display_text_en");
            entity.Property(option => option.SortOrder).HasColumnName("sort_order");
            entity.Property(option => option.IsActive).HasColumnName("is_active");
        });

        modelBuilder.Entity<DistanceOptionEntity>(entity =>
        {
            entity.ToTable("distance_options");
            entity.HasKey(option => option.Id);

            entity.Property(option => option.Id).HasColumnName("id");
            entity.Property(option => option.OptionType).HasColumnName("option_type");
            entity.Property(option => option.Kilometers).HasColumnName("kilometers");
            entity.Property(option => option.DisplayText).HasColumnName("display_text");
            entity.Property(option => option.DisplayTextEn).HasColumnName("display_text_en");
            entity.Property(option => option.SortOrder).HasColumnName("sort_order");
            entity.Property(option => option.IsActive).HasColumnName("is_active");
        });
    }
}
