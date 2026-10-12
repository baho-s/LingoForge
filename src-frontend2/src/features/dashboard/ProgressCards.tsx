import { useTranslation } from 'react-i18next';
import { Flame, Sparkles, Target } from 'lucide-react';
import type { DashboardDto } from '../../types';
import Card from '../../components/ui/Card';
import ProgressBar from '../../components/ui/ProgressBar';

export default function ProgressCards({ dashboard }: { dashboard: DashboardDto | null }) {
  const { t } = useTranslation();
  const streak = dashboard?.streak ?? 0;
  const goal = dashboard?.weeklyGoal ?? 0;
  const reviewed = dashboard?.reviewedThisWeek ?? 0;
  const badges = dashboard?.badges?.length ?? 0;
  const percent = goal > 0 ? Math.min((reviewed / goal) * 100, 100) : 0;

  return (
    <div className="grid grid-cols-2 gap-3 md:grid-cols-3 md:gap-4">
      <Card className="flex items-center gap-3">
        <div className="flex h-11 w-11 items-center justify-center rounded-xl bg-orange-500/15 text-orange-500">
          <Flame size={22} />
        </div>
        <div>
          <p className="text-2xl font-extrabold">{streak}</p>
          <p className="text-xs text-muted">{t('dashboard.dayStreak')}</p>
        </div>
      </Card>

      <Card className="flex items-center gap-3">
        <div className="flex h-11 w-11 items-center justify-center rounded-xl bg-emerald-500/15 text-emerald-500">
          <Sparkles size={22} />
        </div>
        <div>
          <p className="text-2xl font-extrabold">{badges}</p>
          <p className="text-xs text-muted">{t('dashboard.badgesEarned')}</p>
        </div>
      </Card>

      <Card className="col-span-2 md:col-span-1">
        <div className="mb-3 flex items-center gap-3">
          <div className="flex h-11 w-11 items-center justify-center rounded-xl bg-brand/15 text-brand">
            <Target size={22} />
          </div>
          <div>
            <p className="text-2xl font-extrabold">
              {reviewed}
              <span className="text-sm font-normal text-muted">/{goal}</span>
            </p>
            <p className="text-xs text-muted">{t('dashboard.weeklyGoal')}</p>
          </div>
        </div>
        <ProgressBar value={percent} />
        <p className="mt-1.5 text-xs text-muted">
          {percent >= 100 ? t('dashboard.goalReached') : `${Math.round(percent)}${t('dashboard.complete')}`}
        </p>
      </Card>
    </div>
  );
}
