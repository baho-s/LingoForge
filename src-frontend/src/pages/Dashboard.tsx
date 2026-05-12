import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Flame, Target, Sparkles, Brain, Plus, BookOpen } from 'lucide-react';
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer } from 'recharts';
import { dashboardApi } from '../api/endpoints';
import type { DashboardDto, WordDto } from '../types';
import { SkeletonCard, SkeletonBar } from '../components/Skeleton';

export default function Dashboard() {
  const [dashboard, setDashboard] = useState<DashboardDto | null>(null);
  const [wordOfDay, setWordOfDay] = useState<WordDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const navigate = useNavigate();

  useEffect(() => {
    const fetch = async () => {
      try {
        const [dashRes, wodRes] = await Promise.all([
          dashboardApi.get(),
          dashboardApi.getWordOfDay(),
        ]);
        setDashboard(dashRes.data);
        setWordOfDay(wodRes.data);
      } catch {
        setError('Failed to load dashboard data.');
      } finally {
        setLoading(false);
      }
    };
    fetch();
  }, []);

  if (error) {
    return (
      <div className="bg-red-50 text-red-700 rounded-xl px-4 py-3 text-sm">{error}</div>
    );
  }

  const streak = dashboard?.streak ?? 0;
  const dailyGoal = dashboard?.dailyGoal ?? 0;
  const wordsReviewedToday = dashboard?.reviewCount ?? 0;
  const weeklyActivity = dashboard?.weeklyActivity ?? [];
  const goalProgress = dailyGoal > 0 ? Math.min((wordsReviewedToday / dailyGoal) * 100, 100) : 0;

  const dayLabels = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

  return (
    <div className="max-w-5xl mx-auto space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold text-gray-900">Welcome back</h2>
      </div>

      {/* Word of the Day */}
      {loading ? (
        <SkeletonCard />
      ) : wordOfDay ? (
        <div className="bg-white rounded-2xl shadow-sm p-6 border border-gray-100">
          <div className="flex items-center gap-2 mb-4">
            <Sparkles size={18} className="text-blue-600" />
            <span className="text-sm font-semibold text-blue-600">Word of the Day</span>
            <span className="ml-auto text-xs bg-blue-100 text-blue-700 px-2 py-0.5 rounded-full font-medium">New</span>
          </div>
          <div className="flex flex-col md:flex-row md:items-center gap-4">
            <div>
              <p className="text-2xl font-bold text-gray-900">{wordOfDay.original}</p>
              <p className="text-gray-500 text-sm mt-1">{wordOfDay.translation}</p>
            </div>
            {wordOfDay.aiSentence && (
              <div className="md:ml-auto md:max-w-sm bg-gray-50 rounded-xl px-4 py-3 text-sm text-gray-600 italic">
                "{wordOfDay.aiSentence}"
              </div>
            )}
          </div>
        </div>
      ) : !loading && (
        <div className="bg-white rounded-2xl shadow-sm p-6 border border-gray-100 text-center">
          <div className="w-12 h-12 rounded-full bg-gray-100 flex items-center justify-center mx-auto mb-3">
            <BookOpen size={20} className="text-gray-400" />
          </div>
          <p className="text-sm text-gray-500">No words yet. Add your first word to get started!</p>
          <button
            onClick={() => navigate('/words')}
            className="mt-3 inline-flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-xl text-sm font-medium hover:bg-blue-700 transition-colors"
          >
            <Plus size={16} /> Add Word
          </button>
        </div>
      )}

      {/* Progress Grid */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        {loading ? (
          <>
            <SkeletonCard />
            <SkeletonCard />
            <SkeletonCard />
          </>
        ) : (
          <>
            <div className="bg-white rounded-xl shadow-sm p-5 border border-gray-100">
              <div className="flex items-center gap-3 mb-3">
                <div className="w-10 h-10 rounded-xl bg-orange-50 flex items-center justify-center">
                  <Flame size={20} className="text-orange-500" />
                </div>
                <div>
                  <p className="text-2xl font-bold text-gray-900">{streak}</p>
                  <p className="text-xs text-gray-500">Day Streak</p>
                </div>
              </div>
            </div>
            <div className="bg-white rounded-xl shadow-sm p-5 border border-gray-100">
              <div className="flex items-center gap-3 mb-3">
                <div className="w-10 h-10 rounded-xl bg-blue-50 flex items-center justify-center">
                  <Target size={20} className="text-blue-600" />
                </div>
                <div>
                  <p className="text-2xl font-bold text-gray-900">{wordsReviewedToday}<span className="text-sm font-normal text-gray-400">/{dailyGoal}</span></p>
                  <p className="text-xs text-gray-500">Daily Goal</p>
                </div>
              </div>
              <div className="w-full bg-gray-100 rounded-full h-2 mt-2">
                <div
                  className={`h-2 rounded-full transition-all duration-500 ${goalProgress >= 100 ? 'bg-green-500' : 'bg-blue-600'}`}
                  style={{ width: `${goalProgress}%` }}
                />
              </div>
              <p className="text-xs text-gray-400 mt-1.5">
                {goalProgress >= 100 ? 'Goal reached!' : `${Math.round(goalProgress)}% complete`}
              </p>
            </div>
            <div className="bg-white rounded-xl shadow-sm p-5 border border-gray-100">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-xl bg-green-50 flex items-center justify-center">
                  <Sparkles size={20} className="text-green-600" />
                </div>
                <div>
                  <p className="text-2xl font-bold text-gray-900">{dashboard?.badges?.length ?? 0}</p>
                  <p className="text-xs text-gray-500">Badges Earned</p>
                </div>
              </div>
            </div>
          </>
        )}
      </div>

      {/* Quick Actions */}
      {!loading && (
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <button
            onClick={() => navigate('/practice')}
            className="flex items-center gap-4 bg-white rounded-xl shadow-sm p-5 border border-gray-100 hover:shadow-md hover:border-blue-200 transition-all text-left group"
          >
            <div className="w-12 h-12 rounded-xl bg-blue-50 flex items-center justify-center group-hover:bg-blue-100 transition-colors">
              <Brain size={24} className="text-blue-600" />
            </div>
            <div>
              <p className="text-sm font-semibold text-gray-900">Start Review</p>
              <p className="text-xs text-gray-500">Practice your due words</p>
            </div>
          </button>
          <button
            onClick={() => navigate('/words')}
            className="flex items-center gap-4 bg-white rounded-xl shadow-sm p-5 border border-gray-100 hover:shadow-md hover:border-green-200 transition-all text-left group"
          >
            <div className="w-12 h-12 rounded-xl bg-green-50 flex items-center justify-center group-hover:bg-green-100 transition-colors">
              <Plus size={24} className="text-green-600" />
            </div>
            <div>
              <p className="text-sm font-semibold text-gray-900">Add New Word</p>
              <p className="text-xs text-gray-500">Expand your vocabulary</p>
            </div>
          </button>
        </div>
      )}

      {/* Weekly Activity */}
      <div className="bg-white rounded-2xl shadow-sm p-6 border border-gray-100">
        <h3 className="text-sm font-semibold text-gray-900 mb-4">Weekly Activity</h3>
        {loading ? (
          <SkeletonBar />
        ) : weeklyActivity.length > 0 ? (
          <ResponsiveContainer width="100%" height={160}>
            <BarChart data={weeklyActivity.map((d) => ({
              day: dayLabels[new Date(d.date).getDay()],
              words: d.wordsAdded,
            }))}>
              <XAxis dataKey="day" tick={{ fontSize: 12 }} stroke="#9ca3af" />
              <YAxis allowDecimals={false} tick={{ fontSize: 12 }} stroke="#9ca3af" />
              <Tooltip
                contentStyle={{ borderRadius: 12, border: '1px solid #e5e7eb', fontSize: 13 }}
              />
              <Bar dataKey="words" fill="#2563eb" radius={[6, 6, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        ) : (
          <p className="text-sm text-gray-400">No activity this week yet.</p>
        )}
      </div>
    </div>
  );
}
