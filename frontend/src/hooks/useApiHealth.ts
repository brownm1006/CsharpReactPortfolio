import { useEffect, useState } from "react";
import type { ApiHealth } from "../types/vehicle";

const initialState: ApiHealth = {
  status: "checking",
  tableCount: null,
};

export function useApiHealth(): ApiHealth {
  const [apiHealth, setApiHealth] = useState(initialState);
  const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5080";

  useEffect(() => {
    const controller = new AbortController();

    fetch(`${apiBaseUrl}/api/database/health`, { signal: controller.signal })
      .then((response) => {
        if (!response.ok) {
          throw new Error(`API returned ${response.status}`);
        }

        return response.json();
      })
      .then((data: ApiHealth) => setApiHealth(data))
      .catch((error) => {
        if (error instanceof Error && error.name !== "AbortError") {
          setApiHealth({ status: "unavailable", detail: error.message, tableCount: null });
        }
      });

    return () => controller.abort();
  }, [apiBaseUrl]);

  return apiHealth;
}
