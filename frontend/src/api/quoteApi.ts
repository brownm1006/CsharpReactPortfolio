import type {
  BackendLookupOption,
  BackendVehicleModelOption,
  ConfirmedCar,
  Option,
  QuoteResponse,
  QuoteVehicleResponse,
  VehicleDescriptionFormData,
  VehicleUsageFormData,
} from "../types/vehicle";

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5080";

export class ApiError extends Error {
  constructor(
    message: string,
    public readonly status: number,
    public readonly responseText: string,
  ) {
    super(message);
    this.name = "ApiError";
  }
}

type CreateQuoteVehicleRequest = {
  modelYear: number;
  manufacturerCode: string;
  vehicleModelCode: string;
  purchaseYear: number;
  purchaseMonth: number;
  isLeasedCode: string;
  purchaseConditionCode: string;
  trackingSystemCode: string;
  intensiveEngravingCode: string;
  modifiedAfterManufacturingCode: string;
  outsideOfProvinceForPersonalUseCode: string;
  currentOdometerKm: number;
  annualDistanceKm: number;
  driveForBusiness: boolean;
};

export async function createQuote(): Promise<QuoteResponse> {
  return requestJson<QuoteResponse>("/api/quotes", {
    method: "POST",
    body: JSON.stringify({ productType: "Auto" }),
  });
}

export async function listManufacturers(): Promise<Option[]> {
  const options = await requestJson<BackendLookupOption[]>("/api/quote/lookups/manufacturers");
  return options.map(mapBackendLookupOptionToOption);
}

export async function listVehicleModels(manufacturerCode: string, modelYear: number): Promise<Option[]> {
  const query = new URLSearchParams({ modelYear: String(modelYear) });
  const options = await requestJson<BackendVehicleModelOption[]>(
    `/api/quote/lookups/manufacturers/${encodeURIComponent(manufacturerCode)}/models?${query.toString()}`,
  );

  return options.map(mapBackendLookupOptionToOption);
}

export async function listQuoteVehicles(quoteId: string): Promise<ConfirmedCar[]> {
  const vehicles = await requestJson<QuoteVehicleResponse[]>(`/api/quotes/${quoteId}/vehicles`);
  return vehicles.map(mapQuoteVehicleResponseToConfirmedCar);
}

export async function createQuoteVehicle(
  quoteId: string,
  description: VehicleDescriptionFormData,
  usage: VehicleUsageFormData,
): Promise<ConfirmedCar> {
  const vehicle = await requestJson<QuoteVehicleResponse>(`/api/quotes/${quoteId}/vehicles`, {
    method: "POST",
    body: JSON.stringify(mapFormDataToCreateVehicleRequest(description, usage)),
  });

  return mapQuoteVehicleResponseToConfirmedCar(vehicle);
}

export async function deleteQuoteVehicle(quoteId: string, vehicleId: string): Promise<void> {
  await requestVoid(`/api/quotes/${quoteId}/vehicles/${vehicleId}`, { method: "DELETE" });
}

async function requestJson<TResponse>(path: string, init?: RequestInit): Promise<TResponse> {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    headers: {
      "Content-Type": "application/json",
      ...init?.headers,
    },
    ...init,
  });

  if (!response.ok) {
    throw await createApiError(response);
  }

  return response.json() as Promise<TResponse>;
}

async function requestVoid(path: string, init?: RequestInit): Promise<void> {
  const response = await fetch(`${apiBaseUrl}${path}`, init);

  if (!response.ok) {
    throw await createApiError(response);
  }
}

async function createApiError(response: Response): Promise<ApiError> {
  const responseText = typeof response.text === "function" ? await response.text() : "";
  return new ApiError(`API returned ${response.status}`, response.status, responseText);
}

function mapFormDataToCreateVehicleRequest(
  description: VehicleDescriptionFormData,
  usage: VehicleUsageFormData,
): CreateQuoteVehicleRequest {
  return {
    modelYear: Number(description.modelYear),
    manufacturerCode: description.manufacturer,
    vehicleModelCode: description.vehicleModel,
    purchaseYear: Number(description.purchaseYear),
    purchaseMonth: Number(description.purchaseMonth),
    isLeasedCode: description.isLeased,
    purchaseConditionCode: description.purchaseCondition,
    trackingSystemCode: description.trackingSystem,
    intensiveEngravingCode: description.intensiveEngraving,
    modifiedAfterManufacturingCode: description.modifiedAfterManufacturing,
    outsideOfProvinceForPersonalUseCode: usage.outsideOfProvinceForPersonalUse,
    currentOdometerKm: Number(usage.distanceTotal),
    annualDistanceKm: Number(usage.distanceYearly),
    driveForBusiness: usage.driveForBusiness === "Yes",
  };
}

function mapBackendLookupOptionToOption(option: BackendLookupOption): Option {
  return {
    value: option.code,
    label: option.displayText,
  };
}

function mapQuoteVehicleResponseToConfirmedCar(vehicle: QuoteVehicleResponse): ConfirmedCar {
  return {
    id: vehicle.id,
    createdAt: vehicle.createdAtUtc,
    modelYear: String(vehicle.modelYear),
    manufacturer: vehicle.manufacturer.code,
    manufacturerLabel: vehicle.manufacturer.displayText,
    vehicleModel: vehicle.vehicleModel.code,
    vehicleModelLabel: vehicle.vehicleModel.displayText,
    purchaseYear: String(vehicle.purchaseYear),
    purchaseMonth: String(vehicle.purchaseMonth).padStart(2, "0"),
    isLeased: vehicle.isLeased.code,
    purchaseCondition: vehicle.purchaseCondition.code,
    trackingSystem: vehicle.trackingSystem.code,
    intensiveEngraving: vehicle.intensiveEngraving.code,
    modifiedAfterManufacturing: vehicle.modifiedAfterManufacturing.code,
    outsideOfProvinceForPersonalUse: vehicle.outsideOfProvinceForPersonalUse.code,
    distanceTotal: String(vehicle.currentOdometerKm),
    distanceYearly: String(vehicle.annualDistanceKm),
    driveForBusiness: vehicle.driveForBusiness ? "Yes" : "No",
  };
}
