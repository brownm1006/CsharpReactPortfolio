import type { ReactNode } from "react";
import { LanguageSwitcher } from "./LanguageSwitcher";
import { ProgressRail } from "./ProgressRail";
import { useTranslation } from "../i18n/I18nProvider";
import type { ApiHealth, QuoteStep } from "../types/vehicle";

type QuotePageShellProps = {
  apiHealth: ApiHealth;
  children: ReactNode;
  currentStep: QuoteStep;
  onHome: () => void;
  title: string;
};

export function QuotePageShell({ apiHealth, children, currentStep, onHome, title }: QuotePageShellProps) {
  const { t } = useTranslation();

  return (
    <main className="quote-page">
      <ProgressRail currentStep={currentStep} onHome={onHome} />
      <section className="quote-workspace">
        <header className="quote-header">
          <div>
            <p className="eyebrow">{t("app.brand")}</p>
            <h1>{title}</h1>
          </div>
          <div className="header-tools">
            <LanguageSwitcher />
            <div className="status-pill">API {apiHealth.status}</div>
          </div>
        </header>
        {children}
      </section>
    </main>
  );
}
