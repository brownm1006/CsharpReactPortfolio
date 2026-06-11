import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";
import App from "./App";
import { I18nProvider } from "./i18n/I18nProvider";

function createJsonResponse(data: unknown, init?: ResponseInit): Response {
  return {
    ok: init?.status ? init.status >= 200 && init.status < 300 : true,
    status: init?.status ?? 200,
    json: async () => data,
    text: async () => JSON.stringify(data),
  } as Response;
}

function createEmptyResponse(init?: ResponseInit): Response {
  return {
    ok: init?.status ? init.status >= 200 && init.status < 300 : true,
    status: init?.status ?? 204,
    json: async () => undefined,
    text: async () => "",
  } as Response;
}

function createBackendVehicle() {
  return {
    id: "car-1",
    quoteId: "quote-1",
    modelYear: Number(new Date().getFullYear()),
    manufacturer: { code: "HYUNDAI", displayText: "HYUNDAI", sortOrder: 1 },
    vehicleModel: {
      code: "IONIQ_5_PREFERRED_LONG_RANGE_4",
      displayText: "IONIQ 5 PREFERRED LONG RANGE 4",
      sortOrder: 1,
      modelYear: Number(new Date().getFullYear()),
      manufacturerCode: "HYUNDAI",
      trim: "PREFERRED LONG RANGE 4",
      vehicleCode: "IONIQ_5_PREFERRED_LONG_RANGE_4",
    },
    vehicleCode: "IONIQ_5_PREFERRED_LONG_RANGE_4",
    purchaseYear: Number(new Date().getFullYear()),
    purchaseMonth: 1,
    isLeased: { code: "No", displayText: "Non", sortOrder: 2 },
    purchaseCondition: { code: "New", displayText: "Neuf", sortOrder: 1 },
    trackingSystem: { code: "No", displayText: "Non", sortOrder: 2 },
    intensiveEngraving: { code: "No", displayText: "Non", sortOrder: 2 },
    modifiedAfterManufacturing: { code: "No", displayText: "Non", sortOrder: 2 },
    outsideOfProvinceForPersonalUse: { code: "No", displayText: "Non", sortOrder: 2 },
    currentOdometerKm: 30000,
    annualDistanceKm: 15000,
    driveForBusiness: false,
    createdAtUtc: "2026-06-10T12:00:00Z",
    updatedAtUtc: "2026-06-10T12:00:00Z",
  };
}

function mockApi() {
  let vehicles: ReturnType<typeof createBackendVehicle>[] = [];

  vi.stubGlobal(
    "fetch",
    vi.fn(async (input: RequestInfo | URL, init?: RequestInit) => {
      const url = String(input);
      const method = init?.method ?? "GET";

      if (url.endsWith("/api/database/health")) {
        return createJsonResponse({ status: "ok", tableCount: 14 });
      }

      if (url.endsWith("/api/quote/lookups/manufacturers") && method === "GET") {
        return createJsonResponse([
          { code: "HYUNDAI", displayText: "HYUNDAI", sortOrder: 1 },
          { code: "TOYOTA", displayText: "TOYOTA", sortOrder: 2 },
        ]);
      }

      if (url.includes("/api/quote/lookups/manufacturers/HYUNDAI/models") && method === "GET") {
        return createJsonResponse([
          {
            code: "IONIQ_5_PREFERRED_LONG_RANGE_4",
            displayText: "IONIQ 5 PREFERRED LONG RANGE 4",
            sortOrder: 1,
            modelYear: Number(new Date().getFullYear()),
            manufacturerCode: "HYUNDAI",
            trim: "PREFERRED LONG RANGE 4",
            vehicleCode: "IONIQ_5_PREFERRED_LONG_RANGE_4",
          },
        ]);
      }

      if (url.includes("/api/quotes/stale-quote/vehicles")) {
        return createJsonResponse({ detail: "Quote not found." }, { status: 404 });
      }

      if (url.endsWith("/api/quotes") && method === "POST") {
        return createJsonResponse({
          id: "quote-1",
          productType: "Auto",
          status: "Draft",
          currentStep: "Description",
          createdAtUtc: "2026-06-10T12:00:00Z",
          updatedAtUtc: "2026-06-10T12:00:00Z",
        }, { status: 201 });
      }

      if (url.endsWith("/api/quotes/quote-1/vehicles") && method === "GET") {
        return createJsonResponse(vehicles);
      }

      if (url.endsWith("/api/quotes/quote-1/vehicles") && method === "POST") {
        const vehicle = createBackendVehicle();
        vehicles = [vehicle];
        return createJsonResponse(vehicle, { status: 201 });
      }

      if (url.endsWith("/api/quotes/quote-1/vehicles/car-1") && method === "DELETE") {
        vehicles = [];
        return createEmptyResponse({ status: 204 });
      }

      return createJsonResponse({ message: `Unhandled test endpoint: ${method} ${url}` }, { status: 500 });
    }),
  );
}

function mockRandomUuid() {
  vi.stubGlobal("crypto", {
    randomUUID: vi.fn(() => "car-1"),
  });
}

function renderApp(initialEntries = ["/"]) {
  render(
    <I18nProvider>
      <MemoryRouter initialEntries={initialEntries}>
        <App />
      </MemoryRouter>
    </I18nProvider>,
  );
}

async function startNewCar(user: ReturnType<typeof userEvent.setup>) {
  renderApp();
  await user.click(await screen.findByRole("button", { name: "Nouvelle automobile" }));
  await screen.findByRole("heading", { name: "Description - Nouvelle automobile" });
}

async function completeDescriptionStep(user: ReturnType<typeof userEvent.setup>, purchaseYear?: string) {
  const currentYear = String(new Date().getFullYear());
  const [modelYearSelect] = screen.getAllByLabelText("Année");

  await user.selectOptions(modelYearSelect, currentYear);
  await screen.findByRole("option", { name: "HYUNDAI" });
  await user.selectOptions(screen.getByLabelText("Manufacturier"), "HYUNDAI");
  await screen.findByRole("option", { name: "IONIQ 5 PREFERRED LONG RANGE 4" });
  await user.selectOptions(screen.getByLabelText("Modèle"), "IONIQ_5_PREFERRED_LONG_RANGE_4");

  const acquisitionDateGroup = screen.getByRole("group", { name: "Date d'acquisition du véhicule" });
  await user.selectOptions(within(acquisitionDateGroup).getByLabelText("Année"), purchaseYear ?? currentYear);
  await user.selectOptions(within(acquisitionDateGroup).getByLabelText("Mois"), "01");

  await user.click(within(screen.getByRole("group", { name: "Ce véhicule est-il loué?" })).getByLabelText("Non"));
  await user.click(within(screen.getByRole("group", { name: "État du véhicule à l'acquisition?" })).getByLabelText("Neuf"));
  await user.click(
    within(screen.getByRole("group", { name: "Le véhicule possède-t-il un système de repérage?" })).getByLabelText(
      "Non",
    ),
  );
  await user.click(
    within(screen.getByRole("group", { name: "Le véhicule possède-t-il un marquage intensif?" })).getByLabelText(
      "Non",
    ),
  );
  await user.click(
    within(
      screen.getByRole("group", { name: "Des modifications ont-elles été apportées à ce véhicule?" }),
    ).getByLabelText("Non"),
  );
}

async function completeUsageStep(user: ReturnType<typeof userEvent.setup>) {
  await user.click(
    within(screen.getByRole("group", { name: "Le véhicule est-il utilisé à l'extérieur du Québec?" })).getByLabelText(
      "Non",
    ),
  );
  await user.selectOptions(screen.getByLabelText("Quel est le kilométrage actuel du véhicule?"), "30000");
  await user.selectOptions(screen.getByLabelText("Quelle distance le véhicule parcourt-il par année?"), "15000");
  await user.click(
    within(
      screen.getByRole("group", {
        name: /Le véhicule est-il utilisé dans le cadre de votre travail/,
      }),
    ).getByLabelText("Non"),
  );
}

describe("App", () => {
  beforeEach(() => {
    window.localStorage.clear();
    mockApi();
    mockRandomUuid();
  });

  afterEach(() => {
    window.localStorage.clear();
    vi.unstubAllGlobals();
  });

  it("switches the visible language", async () => {
    const user = userEvent.setup();
    renderApp();

    expect(await screen.findByRole("heading", { name: "Bienvenue" })).toBeInTheDocument();

    await user.click(screen.getByRole("button", { name: "EN" }));

    expect(screen.getByRole("heading", { name: "Welcome" })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "New vehicle" })).toBeInTheDocument();

    await user.click(screen.getByRole("button", { name: "FR" }));

    expect(screen.getByRole("heading", { name: "Bienvenue" })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Nouvelle automobile" })).toBeInTheDocument();
  });

  it("shows required description fields before moving to usage", async () => {
    const user = userEvent.setup();
    await startNewCar(user);

    await user.click(await screen.findByRole("button", { name: "Continuer" }));

    const validationSummary = screen.getByRole("alert");
    expect(validationSummary).toHaveTextContent("Champs à compléter");
    expect(within(validationSummary).getByText("Manufacturier")).toBeInTheDocument();
    expect(within(validationSummary).getByText("Date d'acquisition - mois")).toBeInTheDocument();
    expect(validationSummary).toHaveFocus();
    expect(screen.getByRole("heading", { name: "Description - Nouvelle automobile" })).toBeInTheDocument();
  });

  it("redirects protected creation routes when draft data is missing", async () => {
    renderApp(["/car/new/usage"]);

    expect(await screen.findByRole("heading", { name: "Description - Nouvelle automobile" })).toBeInTheDocument();
  });

  it("validates that acquisition year is greater than or equal to construction year", async () => {
    const user = userEvent.setup();
    await startNewCar(user);

    await completeDescriptionStep(user, String(new Date().getFullYear() - 1));
    await user.click(screen.getByRole("button", { name: "Continuer" }));

    expect(screen.getByRole("alert")).toHaveTextContent(
      "Date d'acquisition du véhicule - l'année doit être supérieure ou égale à l'année de construction.",
    );
    expect(screen.getByRole("heading", { name: "Description - Nouvelle automobile" })).toBeInTheDocument();
  });

  it("creates, confirms, lists, and deletes a vehicle", async () => {
    const user = userEvent.setup();
    await startNewCar(user);

    await completeDescriptionStep(user);
    await user.click(screen.getByRole("button", { name: "Continuer" }));
    expect(await screen.findByRole("heading", { name: "Usage - Nouvelle automobile" })).toBeInTheDocument();

    await completeUsageStep(user);
    await user.click(screen.getByRole("button", { name: "Continuer" }));
    expect(await screen.findByRole("heading", { name: "Confirmation - Nouvelle automobile" })).toBeInTheDocument();
    expect(screen.getByText("IONIQ 5 PREFERRED LONG RANGE 4")).toBeInTheDocument();

    await user.click(screen.getByRole("button", { name: "Confirmer" }));
    expect(await screen.findByRole("heading", { name: "Liste des véhicules" })).toBeInTheDocument();
    expect(screen.getByText(/HYUNDAI IONIQ 5 PREFERRED LONG RANGE 4/)).toBeInTheDocument();

    await user.click(screen.getByRole("button", { name: "Supprimer" }));
    expect(screen.queryByText(/HYUNDAI IONIQ 5 PREFERRED LONG RANGE 4/)).not.toBeInTheDocument();
    expect(screen.getByText("Aucune automobile confirmée pour le moment.")).toBeInTheDocument();
  });

  it("recovers from a stale quote id when confirming a vehicle", async () => {
    window.localStorage.setItem("portfolio-club-assurance.quoteId", "stale-quote");
    const user = userEvent.setup();
    renderApp();

    await user.click(await screen.findByRole("button", { name: "Nouvelle automobile" }));
    await screen.findByRole("heading", { name: "Description - Nouvelle automobile" });
    await completeDescriptionStep(user);
    await user.click(screen.getByRole("button", { name: "Continuer" }));

    await completeUsageStep(user);
    await user.click(screen.getByRole("button", { name: "Continuer" }));
    await user.click(await screen.findByRole("button", { name: "Confirmer" }));

    expect(await screen.findByRole("heading", { name: "Liste des véhicules" })).toBeInTheDocument();
    expect(screen.getByText(/HYUNDAI IONIQ 5 PREFERRED LONG RANGE 4/)).toBeInTheDocument();
    expect(window.localStorage.getItem("portfolio-club-assurance.quoteId")).toBe("quote-1");
  });
});
