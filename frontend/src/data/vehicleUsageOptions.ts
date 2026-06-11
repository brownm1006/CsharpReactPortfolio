import type { Option } from "../types/vehicle";

export const provinceUseOptions: Option[] = [
  { value: "Yes", label: "Oui", labelKey: "options.yes" },
  { value: "No", label: "Non", labelKey: "options.no" },
  { value: "Unknown", label: "Je ne sais pas", labelKey: "options.unknown" },
];

export const odometerOptions: Option[] = [
  { value: "10000", label: "10,000 km" },
  { value: "30000", label: "30,000 km" },
  { value: "50000", label: "50,000 km" },
  { value: "75000", label: "75,000 km" },
  { value: "100000", label: "100,000 km" },
];

export const annualDistanceOptions: Option[] = [
  { value: "5000", label: "5,000 km" },
  { value: "10000", label: "10,000 km" },
  { value: "15000", label: "15,000 km" },
  { value: "20000", label: "20,000 km" },
  { value: "25000", label: "25,000 km" },
];

export const yesNoOptions: Option[] = [
  { value: "Yes", label: "Oui", labelKey: "options.yes" },
  { value: "No", label: "Non", labelKey: "options.no" },
];
