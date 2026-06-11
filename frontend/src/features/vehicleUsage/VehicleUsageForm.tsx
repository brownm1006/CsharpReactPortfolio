import { useEffect, useMemo, useRef, useState } from "react";
import type { FormEvent } from "react";
import { FormSelect } from "../../components/FormSelect";
import { RadioGroup } from "../../components/RadioGroup";
import { ValidationSummary } from "../../components/ValidationSummary";
import {
  annualDistanceOptions,
  odometerOptions,
  provinceUseOptions,
  yesNoOptions,
} from "../../data/vehicleUsageOptions";
import { useTranslation } from "../../i18n/I18nProvider";
import { localizeOptions } from "../../i18n/options";
import { getMissingRequiredFields } from "../../utils/validation";
import type { VehicleUsageFormData } from "../../types/vehicle";
import type { TranslationKey } from "../../i18n/translations";

const initialUsageState: VehicleUsageFormData = {
  outsideOfProvinceForPersonalUse: "",
  distanceTotal: "",
  distanceYearly: "",
  driveForBusiness: "",
};

const requiredFields: [keyof VehicleUsageFormData, TranslationKey][] = [
  ["outsideOfProvinceForPersonalUse", "fields.outsideProvince"],
  ["distanceTotal", "fields.currentMileage"],
  ["distanceYearly", "fields.annualDistance"],
  ["driveForBusiness", "fields.businessUse"],
];

type VehicleUsageFormProps = {
  initialData: VehicleUsageFormData | null;
  onBack: () => void;
  onSubmit: (usage: VehicleUsageFormData) => void;
};

export function VehicleUsageForm({ initialData, onBack, onSubmit }: VehicleUsageFormProps) {
  const { t } = useTranslation();
  const validationSummaryRef = useRef<HTMLElement | null>(null);
  const [formData, setFormData] = useState(() => ({ ...initialUsageState, ...initialData }));
  const [missingFields, setMissingFields] = useState<string[]>([]);
  const localizedAnnualDistanceOptions = useMemo(() => localizeOptions(annualDistanceOptions, t), [t]);
  const localizedOdometerOptions = useMemo(() => localizeOptions(odometerOptions, t), [t]);
  const localizedProvinceUseOptions = useMemo(() => localizeOptions(provinceUseOptions, t), [t]);
  const localizedYesNoOptions = useMemo(() => localizeOptions(yesNoOptions, t), [t]);

  useEffect(() => {
    if (missingFields.length > 0) {
      validationSummaryRef.current?.focus();
    }
  }, [missingFields]);

  function updateField(name: string, value: string) {
    setMissingFields([]);
    setFormData((current) => ({ ...current, [name]: value }));
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const missing = getMissingRequiredFields(requiredFields, formData).map((key) => t(key));

    if (missing.length > 0) {
      setMissingFields(missing);
      return;
    }

    onSubmit(formData);
  }

  return (
    <form className="quote-form" onSubmit={handleSubmit}>
      <ValidationSummary ref={validationSummaryRef} missingFields={missingFields} />

      <section className="form-card" aria-labelledby="usage-section-title">
        <div className="form-card-header">
          <div>
            <p className="section-kicker">{t("usage.kicker")}</p>
            <h2 id="usage-section-title">{t("usage.heading")}</h2>
          </div>
          <span className="form-code">{t("usage.formCode")}</span>
        </div>

        <div className="question-stack">
          <RadioGroup
            fieldId="outsideOfProvinceForPersonalUse"
            label={t("fields.outsideProvince")}
            name="outsideOfProvinceForPersonalUse"
            onChange={updateField}
            options={localizedProvinceUseOptions}
            value={formData.outsideOfProvinceForPersonalUse}
          />

          <div className="form-grid form-grid-two">
            <FormSelect
              fieldId="distanceTotal"
              label={t("fields.currentMileage")}
              name="distanceTotal"
              onChange={updateField}
              options={localizedOdometerOptions}
              value={formData.distanceTotal}
            />

            <FormSelect
              fieldId="distanceYearly"
              label={t("fields.annualDistance")}
              name="distanceYearly"
              onChange={updateField}
              options={localizedAnnualDistanceOptions}
              value={formData.distanceYearly}
            />
          </div>

          <RadioGroup
            fieldId="driveForBusiness"
            label={t("fields.businessUse")}
            name="driveForBusiness"
            onChange={updateField}
            options={localizedYesNoOptions}
            value={formData.driveForBusiness}
          />
        </div>
      </section>

      <div className="form-actions form-actions-split">
        <button className="secondary-action" onClick={onBack} type="button">
          {t("actions.back")}
        </button>
        <button className="primary-action" type="submit">
          {t("actions.continue")}
        </button>
      </div>
    </form>
  );
}
