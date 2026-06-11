import { FormSelect } from "./FormSelect";
import { useTranslation } from "../i18n/I18nProvider";
import type { Option } from "../types/vehicle";

type DateSelectGroupProps = {
  monthName: string;
  monthOptions: Option[];
  monthValue: string;
  onChange: (name: string, value: string) => void;
  yearName: string;
  yearOptions: Option[];
  yearValue: string;
};

export function DateSelectGroup({
  monthName,
  monthOptions,
  monthValue,
  onChange,
  yearName,
  yearOptions,
  yearValue,
}: DateSelectGroupProps) {
  const { t } = useTranslation();

  return (
    <fieldset className="date-select-group">
      <legend>{t("date.acquisitionLegend")}</legend>
      <div className="date-select-grid">
        <FormSelect
          fieldId="purchaseYear"
          label={t("date.year")}
          name={yearName}
          onChange={onChange}
          options={yearOptions}
          placeholder={t("date.year")}
          value={yearValue}
        />
        <FormSelect
          disabled={!yearValue}
          fieldId="purchaseMonth"
          helpText={!yearValue ? t("date.monthHelp") : undefined}
          label={t("date.month")}
          name={monthName}
          onChange={onChange}
          options={monthOptions}
          placeholder={t("date.month")}
          value={monthValue}
        />
      </div>
    </fieldset>
  );
}
