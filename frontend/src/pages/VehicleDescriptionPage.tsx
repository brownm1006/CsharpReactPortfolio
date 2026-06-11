import { QuotePageShell } from "../components/QuotePageShell";
import { VehicleDescriptionForm } from "../features/vehicleDescription/VehicleDescriptionForm";
import { useApiHealth } from "../hooks/useApiHealth";
import { useTranslation } from "../i18n/I18nProvider";
import type { VehicleDescriptionFormData } from "../types/vehicle";

type VehicleDescriptionPageProps = {
  initialData: VehicleDescriptionFormData | null;
  onHome: () => void;
  onSubmit: (description: VehicleDescriptionFormData) => void;
};

export function VehicleDescriptionPage({ initialData, onHome, onSubmit }: VehicleDescriptionPageProps) {
  const apiHealth = useApiHealth();
  const { t } = useTranslation();

  return (
    <QuotePageShell
      apiHealth={apiHealth}
      currentStep="description"
      onHome={onHome}
      title={t("description.title")}
    >
      <VehicleDescriptionForm initialData={initialData} onSubmit={onSubmit} />
    </QuotePageShell>
  );
}
