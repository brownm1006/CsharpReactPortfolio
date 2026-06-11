import type { Option } from "../types/vehicle";
import type { TranslationKey } from "./translations";

type Translate = (key: TranslationKey) => string;

export function localizeOptions(options: Option[], t: Translate): Option[] {
  return options.map((option) => ({
    ...option,
    label: option.labelKey ? t(option.labelKey) : option.label,
  }));
}
