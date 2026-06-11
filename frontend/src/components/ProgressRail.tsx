import { useTranslation } from "../i18n/I18nProvider";
import type { TranslationKey } from "../i18n/translations";
import type { QuoteStep } from "../types/vehicle";

const stepOrder: QuoteStep[] = ["welcome", "description", "usage", "confirmation"];

const stepLabelKeys = {
  welcome: "actions.home",
  description: "progress.description",
  usage: "progress.usage",
  confirmation: "progress.confirmation",
} as const satisfies Record<QuoteStep, TranslationKey>;

type StepState = "done" | "current" | "next";

type ProgressRailProps = {
  currentStep: QuoteStep;
  onHome: () => void;
};

function getStepState(stepId: QuoteStep, currentStep: QuoteStep): StepState {
  const stepIndex = stepOrder.indexOf(stepId);
  const currentIndex = stepOrder.indexOf(currentStep);

  if (stepIndex < currentIndex) {
    return "done";
  }

  if (stepIndex === currentIndex) {
    return "current";
  }

  return "next";
}

export function ProgressRail({ currentStep, onHome }: ProgressRailProps) {
  const { t } = useTranslation();

  return (
    <aside className="progress-rail" aria-label={t("progress.label")}>
      <div className="brand-block">
        <span className="brand-mark">AH</span>
        <div>
          <p>{t("app.flowName")}</p>
          <strong>{t("app.domain")}</strong>
        </div>
      </div>

      <button className="home-link" onClick={onHome} type="button">
        {t("actions.home")}
      </button>

      <ol className="step-list">
        {stepOrder.map((stepId, index) => (
          <li className={`step-item step-${getStepState(stepId, currentStep)}`} key={stepId}>
            <span>{index + 1}</span>
            {t(stepLabelKeys[stepId])}
          </li>
        ))}
      </ol>
    </aside>
  );
}
