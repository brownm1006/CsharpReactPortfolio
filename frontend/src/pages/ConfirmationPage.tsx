import { QuotePageShell } from "../components/QuotePageShell";
import { useApiHealth } from "../hooks/useApiHealth";
import { getOptionLabel } from "../utils/options";
import {
  monthOptions,
  purchaseConditionOptions,
  yesNoUnknownOptions,
} from "../data/vehicleDescriptionOptions";
import {
  annualDistanceOptions,
  odometerOptions,
  provinceUseOptions,
  yesNoOptions,
} from "../data/vehicleUsageOptions";
import { useTranslation } from "../i18n/I18nProvider";
import { localizeOptions } from "../i18n/options";
import type { VehicleDescriptionFormData, VehicleUsageFormData } from "../types/vehicle";
import type { TranslationKey } from "../i18n/translations";

type ConfirmationCar = VehicleDescriptionFormData & VehicleUsageFormData;

type SummaryRowProps = {
  label: string;
  value?: string;
};

type ConfirmationPageProps = {
  car: ConfirmationCar;
  errorMessageKey: TranslationKey | null;
  isConfirming: boolean;
  onBack: () => void;
  onConfirm: () => void;
  onHome: () => void;
};

function SummaryRow({ label, value }: SummaryRowProps) {
  const { t } = useTranslation();

  return (
    <div className="summary-row">
      <dt>{label}</dt>
      <dd>{value || t("summary.notProvided")}</dd>
    </div>
  );
}

export function ConfirmationPage({
  car,
  errorMessageKey,
  isConfirming,
  onBack,
  onConfirm,
  onHome,
}: ConfirmationPageProps) {
  const apiHealth = useApiHealth();
  const { t } = useTranslation();
  const localizedAnnualDistanceOptions = localizeOptions(annualDistanceOptions, t);
  const localizedMonthOptions = localizeOptions(monthOptions, t);
  const localizedOdometerOptions = localizeOptions(odometerOptions, t);
  const localizedProvinceUseOptions = localizeOptions(provinceUseOptions, t);
  const localizedPurchaseConditionOptions = localizeOptions(purchaseConditionOptions, t);
  const localizedYesNoOptions = localizeOptions(yesNoOptions, t);
  const localizedYesNoUnknownOptions = localizeOptions(yesNoUnknownOptions, t);

  return (
    <QuotePageShell
      apiHealth={apiHealth}
      currentStep="confirmation"
      onHome={onHome}
      title={t("confirmation.title")}
    >
      <section className="form-card">
        <div className="form-card-header">
          <div>
            <p className="section-kicker">{t("confirmation.kicker")}</p>
            <h2>{t("confirmation.heading")}</h2>
          </div>
        </div>

        <dl className="summary-grid">
          <SummaryRow label={t("summary.modelYear")} value={car.modelYear} />
          <SummaryRow label={t("summary.manufacturer")} value={car.manufacturerLabel ?? car.manufacturer} />
          <SummaryRow label={t("summary.model")} value={car.vehicleModelLabel ?? car.vehicleModel} />
          <SummaryRow label={t("summary.acquisitionYear")} value={car.purchaseYear} />
          <SummaryRow label={t("summary.acquisitionMonth")} value={getOptionLabel(localizedMonthOptions, car.purchaseMonth)} />
          <SummaryRow label={t("summary.isLeased")} value={getOptionLabel(localizedYesNoUnknownOptions, car.isLeased)} />
          <SummaryRow label={t("summary.purchaseCondition")} value={getOptionLabel(localizedPurchaseConditionOptions, car.purchaseCondition)} />
          <SummaryRow label={t("summary.trackingSystem")} value={getOptionLabel(localizedYesNoUnknownOptions, car.trackingSystem)} />
          <SummaryRow label={t("summary.intensiveEngraving")} value={getOptionLabel(localizedYesNoUnknownOptions, car.intensiveEngraving)} />
          <SummaryRow label={t("summary.modifiedAfterManufacturing")} value={getOptionLabel(localizedYesNoUnknownOptions, car.modifiedAfterManufacturing)} />
          <SummaryRow label={t("summary.outsideProvince")} value={getOptionLabel(localizedProvinceUseOptions, car.outsideOfProvinceForPersonalUse)} />
          <SummaryRow label={t("summary.currentMileage")} value={getOptionLabel(localizedOdometerOptions, car.distanceTotal)} />
          <SummaryRow label={t("summary.yearlyDistance")} value={getOptionLabel(localizedAnnualDistanceOptions, car.distanceYearly)} />
          <SummaryRow label={t("summary.businessUse")} value={getOptionLabel(localizedYesNoOptions, car.driveForBusiness)} />
        </dl>
      </section>

      {errorMessageKey ? (
        <div className="validation-summary" role="alert">
          <p>{t(errorMessageKey)}</p>
        </div>
      ) : null}

      <div className="form-actions form-actions-split">
        <button className="secondary-action" disabled={isConfirming} onClick={onBack} type="button">
          {t("actions.back")}
        </button>
        <button className="primary-action" disabled={isConfirming} onClick={onConfirm} type="button">
          {isConfirming ? t("actions.saving") : t("actions.confirm")}
        </button>
      </div>
    </QuotePageShell>
  );
}
