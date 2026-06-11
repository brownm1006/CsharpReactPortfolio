import type { TranslationKey } from "../i18n/translations";

export type Option = {
  value: string;
  label: string;
  labelKey?: TranslationKey;
};

export type VehicleDescriptionFormData = {
  modelYear: string;
  manufacturer: string;
  manufacturerLabel?: string;
  vehicleModel: string;
  vehicleModelLabel?: string;
  purchaseYear: string;
  purchaseMonth: string;
  isLeased: string;
  purchaseCondition: string;
  trackingSystem: string;
  intensiveEngraving: string;
  modifiedAfterManufacturing: string;
};

export type VehicleUsageFormData = {
  outsideOfProvinceForPersonalUse: string;
  distanceTotal: string;
  distanceYearly: string;
  driveForBusiness: string;
};

export type DraftCar = {
  description: VehicleDescriptionFormData | null;
  usage: VehicleUsageFormData | null;
};

export type ConfirmedCar = VehicleDescriptionFormData &
  VehicleUsageFormData & {
    id: string;
    createdAt: string;
  };

export type CarSummary = Partial<VehicleDescriptionFormData & VehicleUsageFormData>;

export type QuoteStep = "welcome" | "description" | "usage" | "confirmation";

export type ApiHealth = {
  status: "checking" | "ok" | "unavailable" | string;
  detail?: string;
  tableCount: number | null;
};

export type QuoteResponse = {
  id: string;
  productType: "Auto" | string;
  status: string;
  currentStep: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
};

export type BackendLookupOption = {
  code: string;
  displayText: string;
  sortOrder: number;
};

export type BackendVehicleModelOption = BackendLookupOption & {
  modelYear: number;
  manufacturerCode: string;
  trim: string | null;
  vehicleCode: string | null;
};

export type QuoteVehicleResponse = {
  id: string;
  quoteId: string;
  modelYear: number;
  manufacturer: BackendLookupOption;
  vehicleModel: BackendVehicleModelOption;
  vehicleCode: string | null;
  purchaseYear: number;
  purchaseMonth: number;
  isLeased: BackendLookupOption;
  purchaseCondition: BackendLookupOption;
  trackingSystem: BackendLookupOption;
  intensiveEngraving: BackendLookupOption;
  modifiedAfterManufacturing: BackendLookupOption;
  outsideOfProvinceForPersonalUse: BackendLookupOption;
  currentOdometerKm: number;
  annualDistanceKm: number;
  driveForBusiness: boolean;
  createdAtUtc: string;
  updatedAtUtc: string;
};
