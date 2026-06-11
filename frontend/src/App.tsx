import { lazy, Suspense, useCallback, useEffect, useReducer, useState } from "react";
import { Navigate, Route, Routes, useNavigate } from "react-router-dom";
import { ApiError, createQuote, createQuoteVehicle, deleteQuoteVehicle, listQuoteVehicles } from "./api/quoteApi";
import type {
  ConfirmedCar,
  DraftCar,
  VehicleDescriptionFormData,
  VehicleUsageFormData,
} from "./types/vehicle";
import type { TranslationKey } from "./i18n/translations";
import "./styles.css";

const ConfirmationPage = lazy(() =>
  import("./pages/ConfirmationPage").then((module) => ({ default: module.ConfirmationPage })),
);
const UsagePage = lazy(() => import("./pages/UsagePage").then((module) => ({ default: module.UsagePage })));
const VehicleDescriptionPage = lazy(() =>
  import("./pages/VehicleDescriptionPage").then((module) => ({ default: module.VehicleDescriptionPage })),
);
const WelcomePage = lazy(() => import("./pages/WelcomePage").then((module) => ({ default: module.WelcomePage })));

const initialDraft: DraftCar = {
  description: null,
  usage: null,
};

const quoteIdStorageKey = "portfolio-club-assurance.quoteId";

type AppState = {
  draftCar: DraftCar;
  isConfirmationComplete: boolean;
};

type AppAction =
  | { type: "startNewCar" }
  | { type: "saveDescription"; description: VehicleDescriptionFormData }
  | { type: "saveUsage"; usage: VehicleUsageFormData }
  | { type: "completeConfirmation" };

const initialState: AppState = {
  draftCar: initialDraft,
  isConfirmationComplete: false,
};

function appReducer(state: AppState, action: AppAction): AppState {
  switch (action.type) {
    case "startNewCar":
      return {
        ...state,
        draftCar: initialDraft,
        isConfirmationComplete: false,
      };
    case "saveDescription":
      return {
        ...state,
        draftCar: { ...state.draftCar, description: action.description },
        isConfirmationComplete: false,
      };
    case "saveUsage":
      return {
        ...state,
        draftCar: { ...state.draftCar, usage: action.usage },
        isConfirmationComplete: false,
      };
    case "completeConfirmation":
      return {
        ...state,
        draftCar: initialDraft,
        isConfirmationComplete: true,
      };
  }
}

function App() {
  const [{ draftCar, isConfirmationComplete }, dispatch] = useReducer(appReducer, initialState);
  const [quoteId, setQuoteId] = useState<string | null>(() => window.localStorage.getItem(quoteIdStorageKey));
  const [confirmedCars, setConfirmedCars] = useState<ConfirmedCar[]>([]);
  const [confirmationError, setConfirmationError] = useState<TranslationKey | null>(null);
  const [isConfirming, setIsConfirming] = useState(false);
  const navigate = useNavigate();

  const createQuoteForSession = useCallback(async () => {
    const quote = await createQuote();
    window.localStorage.setItem(quoteIdStorageKey, quote.id);
    setQuoteId(quote.id);
    return quote;
  }, []);

  const refreshConfirmedCars = useCallback(async (activeQuoteId: string) => {
    try {
      const vehicles = await listQuoteVehicles(activeQuoteId);
      setConfirmedCars(vehicles);
    } catch (error) {
      if (isNotFoundError(error)) {
        window.localStorage.removeItem(quoteIdStorageKey);
        setQuoteId(null);
      }

      setConfirmedCars([]);
    }
  }, []);

  useEffect(() => {
    if (!quoteId) {
      setConfirmedCars([]);
      return;
    }

    void refreshConfirmedCars(quoteId).catch(() => setConfirmedCars([]));
  }, [quoteId, refreshConfirmedCars]);

  const startNewCar = useCallback(async () => {
    if (!quoteId) {
      await createQuoteForSession();
      setConfirmedCars([]);
    }

    setConfirmationError(null);
    dispatch({ type: "startNewCar" });
    navigate("/car/new/description");
  }, [createQuoteForSession, navigate, quoteId]);

  const saveDescription = useCallback((description: VehicleDescriptionFormData) => {
    dispatch({ type: "saveDescription", description });
    navigate("/car/new/usage");
  }, [navigate]);

  const saveUsage = useCallback((usage: VehicleUsageFormData) => {
    dispatch({ type: "saveUsage", usage });
    navigate("/car/new/confirmation");
  }, [navigate]);

  const confirmCar = useCallback(async () => {
    const { description, usage } = draftCar;

    if (!description || !usage) {
      navigate("/car/new/description");
      return;
    }

    setIsConfirming(true);
    setConfirmationError(null);

    try {
      const activeQuoteId = quoteId ?? (await createQuoteForSession()).id;

      try {
        await createQuoteVehicle(activeQuoteId, description, usage);
        await refreshConfirmedCars(activeQuoteId);
      } catch (error) {
        if (!isNotFoundError(error)) {
          throw error;
        }

        const replacementQuote = await createQuoteForSession();
        await createQuoteVehicle(replacementQuote.id, description, usage);
        await refreshConfirmedCars(replacementQuote.id);
      }

      navigate("/", { replace: true });
      dispatch({ type: "completeConfirmation" });
    } catch {
      setConfirmationError("confirmation.saveError");
    } finally {
      setIsConfirming(false);
    }
  }, [createQuoteForSession, draftCar, navigate, quoteId, refreshConfirmedCars]);

  const deleteConfirmedCar = useCallback(async (carId: string) => {
    if (!quoteId) {
      return;
    }

    await deleteQuoteVehicle(quoteId, carId);
    await refreshConfirmedCars(quoteId);
  }, [quoteId, refreshConfirmedCars]);

  const goHome = useCallback(() => {
    navigate("/");
  }, [navigate]);

  const goToDescription = useCallback(() => {
    navigate("/car/new/description");
  }, [navigate]);

  const goToUsage = useCallback(() => {
    navigate("/car/new/usage");
  }, [navigate]);

  return (
    <Suspense fallback={<PageLoading />}>
      <Routes>
        <Route
          path="/"
          element={
            <WelcomePage
              confirmedCars={confirmedCars}
              onCreateCar={startNewCar}
              onDeleteCar={deleteConfirmedCar}
              onHome={goHome}
            />
          }
        />
        <Route
          path="/car/new/description"
          element={<VehicleDescriptionPage initialData={draftCar.description} onHome={goHome} onSubmit={saveDescription} />}
        />
        <Route
          path="/car/new/usage"
          element={
            draftCar.description ? (
              <UsagePage initialData={draftCar.usage} onBack={goToDescription} onHome={goHome} onSubmit={saveUsage} />
            ) : (
              <Navigate replace to="/car/new/description" />
            )
          }
        />
        <Route
          path="/car/new/confirmation"
          element={
            draftCar.description && draftCar.usage ? (
              <ConfirmationPage
                car={{ ...draftCar.description, ...draftCar.usage }}
                errorMessageKey={confirmationError}
                isConfirming={isConfirming}
                onBack={goToUsage}
                onConfirm={confirmCar}
                onHome={goHome}
              />
            ) : (
              <Navigate replace to={isConfirmationComplete ? "/" : draftCar.description ? "/car/new/usage" : "/car/new/description"} />
            )
          }
        />
        <Route path="*" element={<Navigate replace to="/" />} />
      </Routes>
    </Suspense>
  );
}

function isNotFoundError(error: unknown): boolean {
  return error instanceof ApiError && error.status === 404;
}

function PageLoading() {
  return (
    <main className="quote-page-loading" aria-busy="true">
      <div className="loading-panel">Chargement...</div>
    </main>
  );
}

export default App;
