import { QuotePageShell } from "../components/QuotePageShell";
import { useApiHealth } from "../hooks/useApiHealth";
import { ConfirmedCarList } from "../features/confirmedCars/ConfirmedCarList";
import { useTranslation } from "../i18n/I18nProvider";
import type { ConfirmedCar } from "../types/vehicle";

type WelcomePageProps = {
  confirmedCars: ConfirmedCar[];
  onCreateCar: () => void;
  onDeleteCar: (carId: string) => void;
  onHome: () => void;
};

export function WelcomePage({ confirmedCars, onCreateCar, onDeleteCar, onHome }: WelcomePageProps) {
  const apiHealth = useApiHealth();
  const { t } = useTranslation();

  return (
    <QuotePageShell apiHealth={apiHealth} currentStep="welcome" onHome={onHome} title={t("welcome.title")}>
      <section className="welcome-panel">
        <div>
          <p className="section-kicker">{t("welcome.kicker")}</p>
          <h2>{t("welcome.createHeading")}</h2>
          <p>{t("welcome.intro")}</p>
        </div>
        <button className="primary-action" onClick={onCreateCar} type="button">
          {t("actions.createCar")}
        </button>
      </section>

      <section className="flow-overview" aria-label={t("progress.label")}>
        <article>
          <span>1</span>
          <h3>{t("progress.description")}</h3>
          <p>{t("welcome.descriptionText")}</p>
        </article>
        <article>
          <span>2</span>
          <h3>{t("progress.usage")}</h3>
          <p>{t("welcome.usageText")}</p>
        </article>
        <article>
          <span>3</span>
          <h3>{t("progress.confirmation")}</h3>
          <p>{t("welcome.confirmationText")}</p>
        </article>
      </section>

      <ConfirmedCarList cars={confirmedCars} onDeleteCar={onDeleteCar} />
    </QuotePageShell>
  );
}
