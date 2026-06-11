import type { Option } from "../types/vehicle";

type RadioGroupProps = {
  fieldId: string;
  label: string;
  name: string;
  onChange: (name: string, value: string) => void;
  options: Option[];
  stacked?: boolean;
  value: string;
};

export function RadioGroup({ fieldId, label, name, onChange, options, value, stacked = false }: RadioGroupProps) {
  return (
    <fieldset className="radio-fieldset" id={fieldId}>
      <legend>{label}</legend>
      <div className={stacked ? "radio-group radio-group-stacked" : "radio-group"}>
        {options.map((option) => (
          <label className="radio-option" key={option.value}>
            <input
              checked={value === option.value}
              name={name}
              onChange={() => onChange(name, option.value)}
              type="radio"
              value={option.value}
            />
            <span>{option.label}</span>
          </label>
        ))}
      </div>
    </fieldset>
  );
}
