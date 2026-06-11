import type { Option } from "../types/vehicle";

const currentYear = new Date().getFullYear();

export const modelYearOptions: Option[] = Array.from({ length: 8 }, (_, index) => {
  const year = currentYear + 1 - index;
  return { value: String(year), label: String(year) };
});

export const purchaseYearOptions: Option[] = Array.from({ length: 15 }, (_, index) => {
  const year = currentYear - index;
  return { value: String(year), label: String(year) };
});

export const monthOptions: Option[] = [
  { value: "01", label: "Janvier", labelKey: "month.01" },
  { value: "02", label: "Février", labelKey: "month.02" },
  { value: "03", label: "Mars", labelKey: "month.03" },
  { value: "04", label: "Avril", labelKey: "month.04" },
  { value: "05", label: "Mai", labelKey: "month.05" },
  { value: "06", label: "Juin", labelKey: "month.06" },
  { value: "07", label: "Juillet", labelKey: "month.07" },
  { value: "08", label: "Août", labelKey: "month.08" },
  { value: "09", label: "Septembre", labelKey: "month.09" },
  { value: "10", label: "Octobre", labelKey: "month.10" },
  { value: "11", label: "Novembre", labelKey: "month.11" },
  { value: "12", label: "Décembre", labelKey: "month.12" },
];

export const yesNoUnknownOptions: Option[] = [
  { value: "Yes", label: "Oui", labelKey: "options.yes" },
  { value: "No", label: "Non", labelKey: "options.no" },
  { value: "Unknown", label: "Je ne sais pas", labelKey: "options.unknown" },
];

export const purchaseConditionOptions: Option[] = [
  { value: "New", label: "Neuf", labelKey: "options.new" },
  { value: "Demo", label: "Démo", labelKey: "options.demo" },
  { value: "Used", label: "Usagé", labelKey: "options.used" },
];
