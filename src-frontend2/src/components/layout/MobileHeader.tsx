import { useEffect, useState } from 'react';
import { Flame, LogOut } from 'lucide-react';
import { Link, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { dashboardApi } from '../../api/endpoints';
import { useAuthStore } from '../../store/auth';
import ThemeToggle from '../ui/ThemeToggle';
import LanguageToggle from './LanguageToggle';

export default function MobileHeader() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const logout = useAuthStore((s) => s.logout);
  const [streak, setStreak] = useState<number | null>(null);

  useEffect(() => {
    dashboardApi
      .get()
      .then(({ data }) => setStreak(data.streak))
      .catch(() => setStreak(null));
  }, []);

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <header className="sticky top-0 z-30 flex items-center justify-between gap-2 border-b border-line bg-surface/95 px-4 py-3 backdrop-blur md:hidden">
      <Link to="/" aria-label="BeeZillion">
        <h1 className="bg-gradient-to-r from-amber-400 to-amber-600 bg-clip-text text-lg font-extrabold text-transparent">
          🐝 BeeZillion
        </h1>
      </Link>
      <div className="flex items-center gap-2">
        {streak !== null && (
          <span className="inline-flex h-10 items-center gap-1 rounded-xl bg-brand/15 px-3 text-sm font-bold text-brand">
            <Flame size={16} />
            {streak}
          </span>
        )}
        <ThemeToggle />
        <LanguageToggle />
        <button
          type="button"
          onClick={handleLogout}
          aria-label={t('common.logout')}
          className="flex h-10 w-10 items-center justify-center rounded-xl border border-line bg-surface text-muted hover:text-red-500"
        >
          <LogOut size={18} />
        </button>
      </div>
    </header>
  );
}
