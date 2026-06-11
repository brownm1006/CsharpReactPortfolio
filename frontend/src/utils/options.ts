import type { Option } from "../types/vehicle";

export function getOptionLabel(options: Option[], value: string | undefined): string | undefined {
  return options.find((option) => option.value === value)?.label ?? value;
}
