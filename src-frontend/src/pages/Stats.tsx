import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { BookOpen, Target, TrendingUp, Flame } from 'lucide-react';
import { statsApi } from '../api/endpoints';
import type { StatsDto, ActivityHeatmapDay } from '../types';
import { SkeletonCard, SkeletonBar } from '../components/Skeleton';

export default function Stats() {
  const { t } = useTranslation();
  const [stats, setStats] = useState<StatsDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const statCards = [
    { key: 'totalWords' as const, label: t('stats.totalWords'), icon: BookOpen, color: 'bg-blue-50 text-blue-600' },
    { key: 'wordsLearnedThisWeek' as const, label: t('stats.wordsLearnedThisWeek'), icon: TrendingUp, color: 'bg-green-50 text-green-600' },
    { key: 'averageEaseFactor' as const, label: t('stats.averageEaseFactor'), icon: Target, color: 'bg-amber-50 text-amber-600', suffix: '' },
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

      {/* Main stat cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
        {loading
          ? Array.from({ length: 3 }).map((_, i) => <SkeletonCard key={i} />)
          : statCards.map((card) => {
              const Icon = card.icon;
              const value = stats?.[card.key] ?? 0;
              const displayValue = card.key === 'averageEaseFactor'
                ? Number(value).toFixed(2)
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

      {/* Activity Heatmap */}
      {loading ? (
        <SkeletonBar />
      ) : stats?.activityHeatmap && stats.activityHeatmap.length > 0 ? (
        <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-100">
          <div className="flex items-center gap-2 mb-6">
            <Flame size={18} className="text-orange-500" />
            <h3 className="text-lg font-semibold text-gray-900">Aktivite Haritası (Son 1 Yıl)</h3>
          </div>
          <ActivityHeatmap data={stats.activityHeatmap} />
        </div>
      ) : null}

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

// Modern Heatmap Component
function ActivityHeatmap({ data }: { data: ActivityHeatmapDay[] }) {
  const weekDays = ['Pz', 'Pt', 'Sa', 'Ça', 'Pe', 'Cu', 'Cm'];
  const monthNames = ['Oca', 'Şub', 'Mar', 'Nis', 'May', 'Haz', 'Tem', 'Ağu', 'Eyl', 'Eki', 'Kas', 'Ara'];
  
  const maxActivity = Math.max(...data.map(d => d.activityCount), 1);

  // Veriyi haftaya göre organize et
  const weeks: (ActivityHeatmapDay | null)[][] = [];
  let currentWeek: (ActivityHeatmapDay | null)[] = [];

  data.forEach((day) => {
    const date = new Date(day.date);
    const dayOfWeek = date.getDay();
    
    if (dayOfWeek === 1 && currentWeek.length > 0) {
      weeks.push(currentWeek);
      currentWeek = [];
    }
    
    // Pazartesi'ye kadar boşluk doldur
    while (currentWeek.length < dayOfWeek) {
      currentWeek.push(null);
    }
    currentWeek.push(day);
  });

  if (currentWeek.length > 0) {
    weeks.push(currentWeek);
  }

  const getColor = (count: number): string => {
    if (count === 0) return 'bg-gray-100';
    
    const intensity = (count / maxActivity) * 100;
    if (intensity < 20) return 'bg-green-100';
    if (intensity < 40) return 'bg-green-200';
    if (intensity < 60) return 'bg-green-400';
    if (intensity < 80) return 'bg-green-500';
    return 'bg-green-600';
  };

  return (
    <div className="space-y-4">
      <div className="overflow-x-auto">
        <div className="inline-block">
          {/* Ay başlıkları */}
          <div className="flex gap-0.5 mb-2 ml-7">
            {monthNames.map((month, idx) => (
              <div key={idx} className="text-xs text-gray-400 font-semibold" style={{ width: '40px' }}>
                {month}
              </div>
            ))}
          </div>

          {/* Heatmap Grid */}
          {weekDays.map((day, dayIdx) => (
            <div key={dayIdx} className="flex items-center gap-0.5 mb-0.5">
              <div className="text-xs text-gray-500 font-medium w-6 text-right pr-1">
                {day}
              </div>
              <div className="flex gap-0.5">
                {weeks.map((week, weekIdx) => {
                  const dayData = week[dayIdx];
                  return (
                    <div
                      key={`${dayIdx}-${weekIdx}`}
                      className={`w-4 h-4 rounded transition-all duration-200 cursor-help hover:ring-2 hover:ring-offset-1 hover:ring-gray-300 ${getColor(dayData?.activityCount ?? 0)}`}
                      title={dayData ? `${dayData.date}: ${dayData.activityCount} aktivite` : ''}
                    />
                  );
                })}
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Enhanced Legend */}
      <div className="flex items-center justify-center gap-6 mt-6 pt-4 border-t border-gray-100">
        <div className="text-xs text-gray-600 font-medium">Aktivite Yoğunluğu:</div>
        <div className="flex items-center gap-2">
          <div className="text-xs text-gray-500">Az</div>
          <div className="flex gap-1">
            <div className="w-3 h-3 rounded bg-gray-100" />
            <div className="w-3 h-3 rounded bg-green-100" />
            <div className="w-3 h-3 rounded bg-green-200" />
            <div className="w-3 h-3 rounded bg-green-400" />
            <div className="w-3 h-3 rounded bg-green-500" />
            <div className="w-3 h-3 rounded bg-green-600" />
          </div>
          <div className="text-xs text-gray-500">Çok</div>
        </div>
      </div>
    </div>
  );
}
