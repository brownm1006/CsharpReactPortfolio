import { createContext, useContext, useEffect, useMemo, useState } from "react";
import type { ReactNode } from "react";
import { translations } from "./translations";
import type { Locale, TranslationKey } from "./translations";

type I18nContextValue = {
  locale: Locale;
  setLocale: (locale: Locale) => void;
  t: (key: TranslationKey) => string;
};

const defaultLocale: Locale = "fr";
const storageKey = "portfolio-club-assurance-locale";
const I18nContext = createContext<I18nContextValue | null>(null);

type I18nProviderProps = {
  children: ReactNode;
};

function isLocale(value: string | null): value is Locale {
  return value === "fr" || value === "en";
}

export function I18nProvider({ children }: I18nProviderProps) {
  const [locale, setLocale] = useState<Locale>(() => {
    if (typeof window === "undefined") {
      return defaultLocale;
    }

    const storedLocale = window.localStorage.getItem(storageKey);
    return isLocale(storedLocale) ? storedLocale : defaultLocale;
  });

  useEffect(() => {
    window.localStorage.setItem(storageKey, locale);
    document.documentElement.lang = locale;
  }, [locale]);

  const value = useMemo<I18nContextValue>(
    () => ({
      locale,
      setLocale,
      t: (key) => translations[locale][key],
    }),
    [locale],
  );

  return <I18nContext.Provider value={value}>{children}</I18nContext.Provider>;
}

export function useTranslation() {
  const context = useContext(I18nContext);

  if (!context) {
    throw new Error("useTranslation must be used inside I18nProvider.");
  }

  return context;
}
