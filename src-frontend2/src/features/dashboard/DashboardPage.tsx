import { useTranslation } from 'react-i18next';
import Card from '../../components/ui/Card';
import Skeleton from '../../components/ui/Skeleton';
import WeeklyBarChart from '../../components/charts/WeeklyBarChart';
import { useDashboardData } from './useDashboardData';
import WordOfDayCard from './WordOfDayCard';
import ProgressCards from './ProgressCards';
import QuickActions from './QuickActions';

export default function DashboardPage() {
  const { t } = useTranslation();
  const { dashboard, wordOfDay, loading, failed } = useDashboardData();

  if (failed) {
    return (
      <div role="alert" className="rounded-xl bg-red-500/10 px-4 py-3 text-sm text-red-600 dark:text-red-400">
        {t('dashboard.failedToLoadDashboard')}
      </div>
    );
  }

  const weekly = (dashboard?.weeklyActivity ?? []).map((d) => ({ date: d.date, value: d.reviewedCount }));

  return (
    <div className="space-y-5">
      <h2 className="text-2xl font-bold">{t('dashboard.welcomeBack')}</h2>

      {loading ? (
        <>
          <Skeleton className="h-36" />
          <div className="grid grid-cols-2 gap-3 md:grid-cols-3">
            <Skeleton className="h-24" />
            <Skeleton className="h-24" />
            <Skeleton className="col-span-2 h-24 md:col-span-1" />
          </div>
          <Skeleton className="h-48" />
        </>
      ) : (
        <>
          <WordOfDayCard word={wordOfDay} />
          <ProgressCards dashboard={dashboard} />
          <QuickActions />
          <Card>
            <h3 className="mb-4 text-sm font-semibold">{t('dashboard.weeklyActivity')}</h3>
            {weekly.length > 0 ? (
              <WeeklyBarChart data={weekly} />
            ) : (
              <p className="text-sm text-muted">{t('dashboard.noActivityThisWeek')}</p>
            )}
          </Card>
        </>
      )}
    </div>
  );
}
