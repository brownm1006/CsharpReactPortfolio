import { useEffect, useMemo, useRef, useState } from "react";
import type { FormEvent } from "react";
import { DateSelectGroup } from "../../components/DateSelectGroup";
import { FormSelect } from "../../components/FormSelect";
import { RadioGroup } from "../../components/RadioGroup";
import { ValidationSummary } from "../../components/ValidationSummary";
import { listManufacturers, listVehicleModels } from "../../api/quoteApi";
import {
  modelYearOptions,
  monthOptions,
  purchaseConditionOptions,
  purchaseYearOptions,
  yesNoUnknownOptions,
} from "../../data/vehicleDescriptionOptions";
import { useTranslation } from "../../i18n/I18nProvider";
import { localizeOptions } from "../../i18n/options";
import { getMissingRequiredFields } from "../../utils/validation";
import type { Option, VehicleDescriptionFormData } from "../../types/vehicle";
import type { TranslationKey } from "../../i18n/translations";

const initialFormState: VehicleDescriptionFormData = {
  modelYear: "",
  manufacturer: "",
  vehicleModel: "",
  purchaseYear: "",
  purchaseMonth: "",
  isLeased: "",
  purchaseCondition: "",
  trackingSystem: "",
  intensiveEngraving: "",
  modifiedAfterManufacturing: "",
};

const requiredFields: [keyof VehicleDescriptionFormData, TranslationKey][] = [
  ["modelYear", "fields.modelYear"],
  ["manufacturer", "fields.manufacturer"],
  ["vehicleModel", "fields.model"],
  ["purchaseYear", "fields.acquisitionYear"],
  ["purchaseMonth", "fields.acquisitionMonth"],
  ["isLeased", "fields.isLeased"],
  ["purchaseCondition", "fields.purchaseCondition"],
  ["trackingSystem", "fields.trackingSystem"],
  ["intensiveEngraving", "fields.intensiveEngraving"],
  ["modifiedAfterManufacturing", "fields.modifiedAfterManufacturing"],
];

type VehicleDescriptionFormProps = {
  initialData: VehicleDescriptionFormData | null;
  onSubmit: (description: VehicleDescriptionFormData) => void;
};

export function VehicleDescriptionForm({ initialData, onSubmit }: VehicleDescriptionFormProps) {
  const { t } = useTranslation();
  const validationSummaryRef = useRef<HTMLElement | null>(null);
  const [formData, setFormData] = useState(() => ({ ...initialFormState, ...initialData }));
  const [missingFields, setMissingFields] = useState<string[]>([]);
  const [manufacturerOptions, setManufacturerOptions] = useState<Option[]>([]);
  const [modelOptions, setModelOptions] = useState<Option[]>([]);
  const [isLoadingManufacturers, setIsLoadingManufacturers] = useState(true);
  const [isLoadingModels, setIsLoadingModels] = useState(false);
  const [manufacturerLookupError, setManufacturerLookupError] = useState(false);
  const [modelLookupError, setModelLookupError] = useState(false);

  const localizedModelYearOptions = useMemo(() => localizeOptions(modelYearOptions, t), [t]);
  const localizedMonthOptions = useMemo(() => localizeOptions(monthOptions, t), [t]);
  const localizedPurchaseConditionOptions = useMemo(() => localizeOptions(purchaseConditionOptions, t), [t]);
  const localizedPurchaseYearOptions = useMemo(() => localizeOptions(purchaseYearOptions, t), [t]);
  const localizedYesNoUnknownOptions = useMemo(() => localizeOptions(yesNoUnknownOptions, t), [t]);

  useEffect(() => {
    let ignoreResponse = false;

    setIsLoadingManufacturers(true);
    setManufacturerLookupError(false);

    listManufacturers()
      .then((options) => {
        if (!ignoreResponse) {
          setManufacturerOptions(options);
        }
      })
      .catch(() => {
        if (!ignoreResponse) {
          setManufacturerOptions([]);
          setManufacturerLookupError(true);
        }
      })
      .finally(() => {
        if (!ignoreResponse) {
          setIsLoadingManufacturers(false);
        }
      });

    return () => {
      ignoreResponse = true;
    };
  }, []);

  useEffect(() => {
    if (!formData.manufacturer || !formData.modelYear) {
      setModelOptions([]);
      setModelLookupError(false);
      setIsLoadingModels(false);
      return;
    }

    let ignoreResponse = false;

    setIsLoadingModels(true);
    setModelLookupError(false);

    listVehicleModels(formData.manufacturer, Number(formData.modelYear))
      .then((options) => {
        if (ignoreResponse) {
          return;
        }

        setModelOptions(options);
        setFormData((current) => {
          if (!current.vehicleModel || options.some((option) => option.value === current.vehicleModel)) {
            return current;
          }

          return { ...current, vehicleModel: "" };
        });
      })
      .catch(() => {
        if (!ignoreResponse) {
          setModelOptions([]);
          setModelLookupError(true);
        }
      })
      .finally(() => {
        if (!ignoreResponse) {
          setIsLoadingModels(false);
        }
      });

    return () => {
      ignoreResponse = true;
    };
  }, [formData.manufacturer, formData.modelYear]);

  useEffect(() => {
    if (missingFields.length > 0) {
      validationSummaryRef.current?.focus();
    }
  }, [missingFields]);

  function updateField(name: string, value: string) {
    setMissingFields([]);
    setFormData((current) => {
      if (name === "manufacturer") {
        return {
          ...current,
          manufacturer: value,
          manufacturerLabel: manufacturerOptions.find((option) => option.value === value)?.label,
          vehicleModel: "",
          vehicleModelLabel: undefined,
        };
      }

      if (name === "modelYear") {
        return { ...current, modelYear: value, vehicleModel: "", vehicleModelLabel: undefined };
      }

      if (name === "vehicleModel") {
        return {
          ...current,
          vehicleModel: value,
          vehicleModelLabel: modelOptions.find((option) => option.value === value)?.label,
        };
      }

      if (name === "purchaseYear" && !value) {
        return { ...current, purchaseYear: "", purchaseMonth: "" };
      }

      return { ...current, [name]: value };
    });
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const missing = getMissingRequiredFields(requiredFields, formData).map((key) => t(key));
    const validationMessages = [...missing];

    if (
      formData.modelYear &&
      formData.purchaseYear &&
      Number(formData.purchaseYear) < Number(formData.modelYear)
    ) {
      validationMessages.push(
        t("validation.acquisitionYear"),
      );
    }

    if (validationMessages.length > 0) {
      setMissingFields(validationMessages);
      return;
    }

    onSubmit(formData);
  }

  return (
    <form className="quote-form" onSubmit={handleSubmit}>
      <ValidationSummary ref={validationSummaryRef} missingFields={missingFields} />

      <section className="form-card" aria-labelledby="vehicle-section-title">
        <div className="form-card-header">
          <div>
            <p className="section-kicker">{t("description.kicker")}</p>
            <h2 id="vehicle-section-title">{t("description.heading")}</h2>
          </div>
          <span className="form-code">{t("description.formCode")}</span>
        </div>

        <div className="form-grid">
          <FormSelect
            fieldId="modelYear"
            label={t("fields.modelYear")}
            name="modelYear"
            onChange={updateField}
            options={localizedModelYearOptions}
            value={formData.modelYear}
          />

          <FormSelect
            disabled={!formData.modelYear || isLoadingManufacturers || manufacturerLookupError}
            fieldId="manufacturer"
            helpText={
              !formData.modelYear
                ? t("help.chooseModelYearFirst")
                : isLoadingManufacturers
                  ? t("help.loadingManufacturers")
                  : manufacturerLookupError
                    ? t("help.lookupUnavailable")
                    : undefined
            }
            label={t("fields.manufacturer")}
            name="manufacturer"
            onChange={updateField}
            options={manufacturerOptions}
            value={formData.manufacturer}
          />

          <FormSelect
            disabled={!formData.manufacturer || isLoadingModels || modelLookupError}
            fieldId="vehicleModel"
            helpText={
              !formData.manufacturer
                ? t("help.chooseManufacturerFirst")
                : isLoadingModels
                  ? t("help.loadingModels")
                  : modelLookupError
                    ? t("help.lookupUnavailable")
                    : undefined
            }
            label={t("fields.model")}
            name="vehicleModel"
            onChange={updateField}
            options={modelOptions}
            value={formData.vehicleModel}
          />

          <DateSelectGroup
            monthName="purchaseMonth"
            monthOptions={localizedMonthOptions}
            monthValue={formData.purchaseMonth}
            onChange={updateField}
            yearName="purchaseYear"
            yearOptions={localizedPurchaseYearOptions}
            yearValue={formData.purchaseYear}
          />
        </div>
      </section>

      <section className="form-card" aria-label={t("description.characteristics")}>
        <div className="question-stack">
          <RadioGroup
            fieldId="isLeased"
            label={t("fields.isLeased")}
            name="isLeased"
            onChange={updateField}
            options={localizedYesNoUnknownOptions}
            value={formData.isLeased}
          />

          <RadioGroup
            fieldId="purchaseCondition"
            label={t("fields.purchaseCondition")}
            name="purchaseCondition"
            onChange={updateField}
            options={localizedPurchaseConditionOptions}
            stacked
            value={formData.purchaseCondition}
          />

          <RadioGroup
            fieldId="trackingSystem"
            label={t("fields.trackingSystem")}
            name="trackingSystem"
            onChange={updateField}
            options={localizedYesNoUnknownOptions}
            value={formData.trackingSystem}
          />

          <RadioGroup
            fieldId="intensiveEngraving"
            label={t("fields.intensiveEngraving")}
            name="intensiveEngraving"
            onChange={updateField}
            options={localizedYesNoUnknownOptions}
            value={formData.intensiveEngraving}
          />

          <RadioGroup
            fieldId="modifiedAfterManufacturing"
            label={t("fields.modifiedAfterManufacturing")}
            name="modifiedAfterManufacturing"
            onChange={updateField}
            options={localizedYesNoUnknownOptions}
            value={formData.modifiedAfterManufacturing}
          />
        </div>
      </section>

      <div className="form-actions">
        <button className="primary-action" type="submit">
          {t("actions.continue")}
        </button>
      </div>
    </form>
  );
}
