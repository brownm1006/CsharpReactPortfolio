import { useTranslation } from "../i18n/I18nProvider";

export function LanguageSwitcher() {
  const { locale, setLocale, t } = useTranslation();

  return (
    <div className="language-switcher" aria-label={t("app.language")}>
      <button
        aria-pressed={locale === "fr"}
        className={locale === "fr" ? "language-option language-option-active" : "language-option"}
        onClick={() => setLocale("fr")}
        type="button"
      >
        FR
      </button>
      <button
        aria-pressed={locale === "en"}
        className={locale === "en" ? "language-option language-option-active" : "language-option"}
        onClick={() => setLocale("en")}
        type="button"
      >
        EN
      </button>
    </div>
  );
}
