import { QuotePageShell } from "../components/QuotePageShell";
import { VehicleUsageForm } from "../features/vehicleUsage/VehicleUsageForm";
import { useApiHealth } from "../hooks/useApiHealth";
import { useTranslation } from "../i18n/I18nProvider";
import type { VehicleUsageFormData } from "../types/vehicle";

type UsagePageProps = {
  initialData: VehicleUsageFormData | null;
  onBack: () => void;
  onHome: () => void;
  onSubmit: (usage: VehicleUsageFormData) => void;
};

export function UsagePage({ initialData, onBack, onHome, onSubmit }: UsagePageProps) {
  const apiHealth = useApiHealth();
  const { t } = useTranslation();

  return (
    <QuotePageShell apiHealth={apiHealth} currentStep="usage" onHome={onHome} title={t("usage.title")}>
      <VehicleUsageForm initialData={initialData} onBack={onBack} onSubmit={onSubmit} />
    </QuotePageShell>
  );
}
