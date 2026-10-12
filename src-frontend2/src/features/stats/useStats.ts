import { useEffect, useState } from 'react';
import { statsApi } from '../../api/endpoints';
import type { StatsDto } from '../../types';

export function useStats() {
  const [stats, setStats] = useState<StatsDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [failed, setFailed] = useState(false);

  useEffect(() => {
    statsApi
      .get()
      .then(({ data }) => setStats(data))
      .catch(() => setFailed(true))
      .finally(() => setLoading(false));
  }, []);

  return { stats, loading, failed };
}

const dayKey = (d: Date) => d.toLocaleDateString('sv-SE');

// Last 7 days ending today; days without activity are filled with 0.
// The heatmap stops at yesterday, so today's value comes from todayAttempts.
export function lastSevenDays(stats: StatsDto) {
  const counts = new Map(stats.activityHeatmap.map((h) => [h.date.slice(0, 10), h.activityCount]));
  const todayKey = dayKey(new Date());
  return Array.from({ length: 7 }, (_, i) => {
    const d = new Date();
    d.setDate(d.getDate() - (6 - i));
    const key = dayKey(d);
    const value = key === todayKey ? Math.max(counts.get(key) ?? 0, stats.todayAttempts) : (counts.get(key) ?? 0);
    return { date: key, value };
  });
}
