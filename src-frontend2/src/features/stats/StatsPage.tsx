import { BookOpen } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import DonutChart from '../../components/charts/DonutChart';
import WeeklyBarChart from '../../components/charts/WeeklyBarChart';
import Card from '../../components/ui/Card';
import ProgressBar from '../../components/ui/ProgressBar';
import Skeleton from '../../components/ui/Skeleton';
import KpiGrid from './KpiGrid';
import { lastSevenDays, useStats } from './useStats';

export default function StatsPage() {
  const { t } = useTranslation();
  const { stats, loading, failed } = useStats();

  if (failed) {
    return (
      <div role="alert" className="rounded-xl bg-red-500/10 px-4 py-3 text-sm text-red-600 dark:text-red-400">
        {t('common.error')}
      </div>
    );
  }

  if (loading || !stats) {
    return (
      <div className="space-y-4">
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-52" />
        <div className="grid grid-cols-2 gap-3 lg:grid-cols-3">
          {[0, 1, 2, 3, 4, 5].map((i) => (
            <Skeleton key={i} className="h-28" />
          ))}
        </div>
      </div>
    );
  }

  const wrong = Math.max(0, stats.totalAttempts - stats.correctAttempts);
  const todayWrong = Math.max(0, stats.todayAttempts - stats.todayCorrectAttempts);

  return (
    <div className="space-y-5">
      <h2 className="text-2xl font-bold">{t('stats.statistics')}</h2>

      {stats.totalWords === 0 && (
        <div className="rounded-2xl border border-dashed border-line p-8 text-center">
          <BookOpen className="mx-auto mb-3 text-muted" />
          <p className="text-sm text-muted">{t('stats.emptyDesc')}</p>
        </div>
      )}

      <div className="grid gap-4 lg:grid-cols-3">
        <Card className="lg:col-span-2">
          <h3 className="mb-4 font-semibold">{t('stats.weeklyActivity')}</h3>
          <WeeklyBarChart data={lastSevenDays(stats)} />
        </Card>
        <Card className="flex flex-col items-center">
          <h3 className="mb-4 self-start font-semibold">{t('stats.accuracyDistribution')}</h3>
          <DonutChart correct={stats.correctAttempts} wrong={wrong} />
          <div className="mt-4 flex gap-4 text-xs text-muted">
            <span>
              <span className="mr-1 inline-block h-2 w-2 rounded-full bg-amber-500" />
              {t('stats.correctAttempts')}: {stats.correctAttempts}
            </span>
            <span>
              <span className="mr-1 inline-block h-2 w-2 rounded-full bg-red-500/60" />
              {t('stats.wrongAttempts')}: {wrong}
            </span>
          </div>
        </Card>
      </div>

      <KpiGrid stats={stats} />

      <Card>
        <h3 className="mb-3 font-semibold">{t('stats.todaySummary')}</h3>
        <div className="mb-2 flex justify-between text-sm text-muted">
          <span>
            {t('stats.todayReviewed')}: {stats.todayReviewedWords}
          </span>
          <span>
            {t('stats.todayCorrect')}: {stats.todayCorrectAttempts}
          </span>
          <span>
            {t('stats.todayIncorrect')}: {todayWrong}
          </span>
        </div>
        <ProgressBar value={stats.todayCorrectAttempts} max={stats.todayAttempts} />
      </Card>
    </div>
  );
}
