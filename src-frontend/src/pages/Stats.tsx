import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { BookOpen, Target, TrendingUp, Flame, CheckCircle2, Activity, Timer } from 'lucide-react';
import { statsApi } from '../api/endpoints';
import type { StatsDto } from '../types';
import { SkeletonCard, SkeletonBar } from '../components/Skeleton';

export default function Stats() {
  const { t } = useTranslation();
  const [stats, setStats] = useState<StatsDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const statCards = [
    { key: 'totalWords' as const, label: t('stats.totalWords'), icon: BookOpen, color: 'bg-blue-50 text-blue-600' },
    { key: 'totalAttempts' as const, label: t('stats.totalAttempts'), icon: Activity, color: 'bg-indigo-50 text-indigo-600' },
    { key: 'correctAttempts' as const, label: t('stats.correctAttempts'), icon: CheckCircle2, color: 'bg-emerald-50 text-emerald-600' },
    { key: 'accuracyRate' as const, label: t('stats.accuracyRate'), icon: TrendingUp, color: 'bg-green-50 text-green-600', format: (v: number) => `%${(v * 100).toFixed(1)}` },
    { key: 'averageTimeTakenMs' as const, label: t('stats.averageTimeTaken'), icon: Timer, color: 'bg-amber-50 text-amber-600', format: (v: number) => `${(v / 1000).toFixed(1)} sn` },
    { key: 'correctAttemptsThisWeek' as const, label: t('stats.correctThisWeek'), icon: Target, color: 'bg-orange-50 text-orange-600' },
  ];

  useEffect(() => {
    const fetch = async () => {
      try {
        const { data } = await statsApi.get();
        setStats(data);
      } catch {
        setError(t('common.error'));
      } finally {
        setLoading(false);
      }
    };
    fetch();
  }, []);

  if (error) {
    return (
      <div className="max-w-5xl mx-auto">
        <div className="bg-red-50 text-red-700 rounded-xl px-4 py-3 text-sm">{error}</div>
      </div>
    );
  }

  return (
    <div className="max-w-5xl mx-auto space-y-6">
      <h2 className="text-2xl font-bold text-gray-900">{t('stats.statistics')}</h2>

      {/* Today Summary */}
      {loading ? (
        <SkeletonBar />
      ) : stats ? (
        <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-100">
          <div className="flex items-center gap-2 mb-4">
            <Flame size={18} className="text-orange-500" />
            <h3 className="text-lg font-semibold text-gray-900">{t('stats.todaySummary')}</h3>
          </div>
          {(() => {
            const todayAttempts = stats.todayAttempts ?? 0;
            const todayCorrect = stats.todayCorrectAttempts ?? 0;
            const todayIncorrect = Math.max(0, todayAttempts - todayCorrect);
            const correctPercent = todayAttempts > 0 ? (todayCorrect / todayAttempts) * 100 : 0;
            const incorrectPercent = todayAttempts > 0 ? (todayIncorrect / todayAttempts) * 100 : 0;

            return (
              <div className="space-y-3">
                <div className="flex items-center justify-between text-sm text-gray-600">
                  <span>{t('stats.todayAttempts')}: {todayAttempts}</span>
                  <span>{t('stats.todayCorrect')}: {todayCorrect}</span>
                  <span>{t('stats.todayIncorrect')}: {todayIncorrect}</span>
                </div>
                <div className="flex h-3 w-full rounded-full bg-gray-100 overflow-hidden">
                  <div
                    className="h-full bg-emerald-500"
                    style={{ width: `${correctPercent}%` }}
                  />
                  <div
                    className="h-full bg-rose-400"
                    style={{ width: `${incorrectPercent}%` }}
                  />
                </div>
              </div>
            );
          })()}
        </div>
      ) : null}

      {/* Main stat cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
        {loading
          ? Array.from({ length: statCards.length }).map((_, i) => <SkeletonCard key={i} />)
          : statCards.map((card) => {
              const Icon = card.icon;
              const value = stats?.[card.key] ?? 0;
              const displayValue = card.format
                ? card.format(Number(value))
                : value;
              return (
                <div key={card.key} className="bg-white rounded-xl shadow-sm p-6 border border-gray-100">
                  <div className={`w-10 h-10 rounded-xl ${card.color} flex items-center justify-center mb-4`}>
                    <Icon size={20} />
                  </div>
                  <p className="text-3xl font-bold text-gray-900">{displayValue}</p>
                  <p className="text-sm text-gray-500 mt-1">{card.label}</p>
                </div>
              );
            })}
      </div>

      {/* Empty state */}
      {!loading && stats && stats.totalWords === 0 && (
        <div className="text-center py-12">
          <div className="w-16 h-16 rounded-full bg-gray-100 flex items-center justify-center mx-auto mb-4">
            <BookOpen size={24} className="text-gray-300" />
          </div>
          <p className="text-gray-500 text-sm">Henüz istatistik yok. Kelime eklemeye ve tekrar etmeye başla!</p>
        </div>
      )}
    </div>
  );
}
