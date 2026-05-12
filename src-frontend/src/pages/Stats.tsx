import { useEffect, useState } from 'react';
import { BookOpen, Target, TrendingUp } from 'lucide-react';
import { statsApi } from '../api/endpoints';
import type { StatsDto } from '../types';
import { SkeletonCard } from '../components/Skeleton';

const statCards = [
  { key: 'totalWords' as const, label: 'Total Words', icon: BookOpen, color: 'bg-blue-50 text-blue-600' },
  { key: 'wordsLearnedThisWeek' as const, label: 'Learned This Week', icon: TrendingUp, color: 'bg-green-50 text-green-600' },
  { key: 'averageEaseFactor' as const, label: 'Average Ease', icon: Target, color: 'bg-amber-50 text-amber-600', suffix: '' },
];

export default function Stats() {
  const [stats, setStats] = useState<StatsDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetch = async () => {
      try {
        const { data } = await statsApi.get();
        setStats(data);
      } catch {
        setError('Failed to load statistics.');
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
      <h2 className="text-2xl font-bold text-gray-900">Statistics</h2>

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

      {/* Empty state */}
      {!loading && stats && stats.totalWords === 0 && (
        <div className="text-center py-12">
          <div className="w-16 h-16 rounded-full bg-gray-100 flex items-center justify-center mx-auto mb-4">
            <BookOpen size={24} className="text-gray-300" />
          </div>
          <p className="text-gray-500 text-sm">No statistics yet. Start adding words and reviewing!</p>
        </div>
      )}
    </div>
  );
}
