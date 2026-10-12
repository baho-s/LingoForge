import { useCallback, useEffect, useState } from 'react';
import { dashboardApi } from '../../api/endpoints';
import type { DashboardDto, WordDto } from '../../types';

const WORD_REFRESH_MS = 20 * 60 * 1000;

export function useDashboardData() {
  const [dashboard, setDashboard] = useState<DashboardDto | null>(null);
  const [wordOfDay, setWordOfDay] = useState<WordDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [failed, setFailed] = useState(false);

  const loadWordOfDay = useCallback(async () => {
    try {
      const { data } = await dashboardApi.getWordOfDay();
      setWordOfDay(data);
    } catch {
      setWordOfDay(null);
    }
  }, []);

  useEffect(() => {
    dashboardApi
      .get()
      .then(({ data }) => setDashboard(data))
      .catch(() => setFailed(true))
      .finally(() => setLoading(false));
    loadWordOfDay();
    const id = setInterval(loadWordOfDay, WORD_REFRESH_MS);
    return () => clearInterval(id);
  }, [loadWordOfDay]);

  return { dashboard, wordOfDay, loading, failed };
}
