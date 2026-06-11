import { FieldGroup } from "./FieldGroup";
import { useTranslation } from "../i18n/I18nProvider";
import type { Option } from "../types/vehicle";

type FormSelectProps = {
  disabled?: boolean;
  fieldId: string;
  helpText?: string;
  label: string;
  name: string;
  onChange: (name: string, value: string) => void;
  options: Option[];
  placeholder?: string;
  value: string;
};

export function FormSelect({
  disabled = false,
  fieldId,
  helpText,
  label,
  name,
  onChange,
  options,
  placeholder,
  value,
}: FormSelectProps) {
  const { t } = useTranslation();

  return (
    <FieldGroup fieldId={fieldId} helpText={helpText} label={label}>
      <select
        className="form-select"
        disabled={disabled}
        id={fieldId}
        name={name}
        onChange={(event) => onChange(name, event.target.value)}
        value={value}
      >
        <option value="">{placeholder ?? t("forms.selectPlaceholder")}</option>
        {options.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
    </FieldGroup>
  );
}
