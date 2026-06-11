import { annualDistanceOptions } from "../../data/vehicleUsageOptions";
import { useTranslation } from "../../i18n/I18nProvider";
import { localizeOptions } from "../../i18n/options";
import { getOptionLabel } from "../../utils/options";
import type { ConfirmedCar } from "../../types/vehicle";

type ConfirmedCarListProps = {
  cars: ConfirmedCar[];
  onDeleteCar: (carId: string) => void;
};

export function ConfirmedCarList({ cars, onDeleteCar }: ConfirmedCarListProps) {
  const { locale, t } = useTranslation();
  const localizedAnnualDistanceOptions = localizeOptions(annualDistanceOptions, t);

  return (
    <section className="form-card car-list-card">
      <div className="form-card-header">
        <div>
          <p className="section-kicker">{t("carList.kicker")}</p>
          <h2>{t("carList.heading")}</h2>
        </div>
        <span className="form-code">{cars.length}</span>
      </div>

      {cars.length === 0 ? (
        <p className="empty-state">{t("carList.empty")}</p>
      ) : (
        <div className="car-list">
          {cars.map((car) => (
            <article className="car-list-item" key={car.id}>
              <div>
                <h3>
                  {car.modelYear} {car.manufacturerLabel ?? car.manufacturer}{" "}
                  {car.vehicleModelLabel ?? car.vehicleModel}
                </h3>
                <p>
                  {t("carList.yearlyDistance")}:{" "}
                  {getOptionLabel(localizedAnnualDistanceOptions, car.distanceYearly)}
                </p>
              </div>
              <div className="car-list-actions">
                <time dateTime={car.createdAt}>
                  {new Intl.DateTimeFormat(locale === "fr" ? "fr-CA" : "en-CA", {
                    dateStyle: "medium",
                    timeStyle: "short",
                  }).format(new Date(car.createdAt))}
                </time>
                <button className="danger-action" onClick={() => onDeleteCar(car.id)} type="button">
                  {t("actions.delete")}
                </button>
              </div>
            </article>
          ))}
        </div>
      )}
    </section>
  );
}
