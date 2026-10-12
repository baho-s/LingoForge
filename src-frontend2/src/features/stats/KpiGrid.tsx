import type { ReactNode } from 'react';
import { Activity, BookOpen, CheckCircle2, Target, Timer, TrendingUp } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import Card from '../../components/ui/Card';
import type { StatsDto } from '../../types';

interface KpiGridProps {
  stats: StatsDto;
}

function Kpi({ icon, tone, value, label }: { icon: ReactNode; tone: string; value: string | number; label: string }) {
  return (
    <Card>
      <div className={`mb-3 flex h-10 w-10 items-center justify-center rounded-xl ${tone}`}>{icon}</div>
      <p className="text-2xl font-bold text-fg">{value}</p>
      <p className="text-sm text-muted">{label}</p>
    </Card>
  );
}

export default function KpiGrid({ stats }: KpiGridProps) {
  const { t } = useTranslation();
  return (
    <div className="grid grid-cols-2 gap-3 lg:grid-cols-3">
      <Kpi icon={<BookOpen size={20} />} tone="bg-blue-500/15 text-blue-500" value={stats.totalWords} label={t('stats.totalWords')} />
      <Kpi icon={<Activity size={20} />} tone="bg-indigo-500/15 text-indigo-500" value={stats.totalAttempts} label={t('stats.totalAttempts')} />
      <Kpi icon={<CheckCircle2 size={20} />} tone="bg-emerald-500/15 text-emerald-500" value={stats.correctAttempts} label={t('stats.correctAttempts')} />
      <Kpi icon={<TrendingUp size={20} />} tone="bg-green-500/15 text-green-500" value={`%${(stats.accuracyRate * 100).toFixed(1)}`} label={t('stats.accuracyRate')} />
      <Kpi icon={<Timer size={20} />} tone="bg-amber-500/15 text-amber-500" value={`${(stats.averageTimeTakenMs / 1000).toFixed(1)} ${t('stats.seconds')}`} label={t('stats.averageTimeTaken')} />
      <Kpi icon={<Target size={20} />} tone="bg-orange-500/15 text-orange-500" value={stats.correctAttemptsThisWeek} label={t('stats.correctThisWeek')} />
    </div>
  );
}
