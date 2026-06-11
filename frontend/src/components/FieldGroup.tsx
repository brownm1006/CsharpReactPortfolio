import type { ReactNode } from "react";

type FieldGroupProps = {
  children: ReactNode;
  fieldId: string;
  helpText?: string;
  label: string;
};

export function FieldGroup({ children, helpText, label, fieldId }: FieldGroupProps) {
  return (
    <div className="field-group">
      <label className="field-label" htmlFor={fieldId}>
        {label}
      </label>
      {children}
      {helpText ? <p className="field-help">{helpText}</p> : null}
    </div>
  );
}
