import { forwardRef } from "react";
import { useTranslation } from "../i18n/I18nProvider";

type ValidationSummaryProps = {
  missingFields: string[];
};

export const ValidationSummary = forwardRef<HTMLElement, ValidationSummaryProps>(function ValidationSummary(
  { missingFields },
  ref,
) {
  const { t } = useTranslation();

  if (missingFields.length === 0) {
    return null;
  }

  return (
    <section
      className="validation-summary"
      aria-live="polite"
      aria-labelledby="validation-summary-title"
      ref={ref}
      role="alert"
      tabIndex={-1}
    >
      <h3 id="validation-summary-title">{t("validation.title")}</h3>
      <ul>
        {missingFields.map((field) => (
          <li key={field}>{field}</li>
        ))}
      </ul>
    </section>
  );
});
