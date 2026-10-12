import { Link, NavLink, useNavigate } from 'react-router-dom';
import { LogOut } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { useAuthStore } from '../../store/auth';
import { useNavItems } from './nav-items';
import ThemeToggle from '../ui/ThemeToggle';
import LanguageToggle from './LanguageToggle';

export default function Sidebar() {
  const { t } = useTranslation();
  const items = useNavItems();
  const logout = useAuthStore((s) => s.logout);
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <aside className="hidden w-64 flex-shrink-0 flex-col border-r border-line bg-surface md:flex">
      <div className="px-6 py-6">
        <Link to="/" aria-label="BeeZillion" className="inline-block">
          <h1 className="bg-gradient-to-r from-amber-400 to-amber-600 bg-clip-text text-xl font-extrabold tracking-tight text-transparent">
            🐝 BeeZillion
          </h1>
        </Link>
        <p className="mt-1 text-xs text-muted">{t('common.improveEnglish')}</p>
      </div>
      <nav className="flex-1 space-y-1 px-3">
        {items.map(({ to, label, icon: Icon }) => (
          <NavLink
            key={to}
            to={to}
            end={to === '/'}
            className={({ isActive }) =>
              `flex items-center gap-3 rounded-xl px-4 py-3 text-sm font-semibold transition-colors ${
                isActive ? 'bg-brand/15 text-brand' : 'text-muted hover:bg-elevated hover:text-fg'
              }`
            }
          >
            <Icon size={18} />
            {label}
          </NavLink>
        ))}
      </nav>
      <div className="space-y-2 px-3 pb-4">
        <div className="flex gap-2 px-1">
          <ThemeToggle />
          <LanguageToggle />
        </div>
        <button
          type="button"
          onClick={handleLogout}
          className="flex w-full items-center gap-3 rounded-xl px-4 py-3 text-sm font-medium text-muted transition-colors hover:bg-red-500/10 hover:text-red-500"
        >
          <LogOut size={18} />
          {t('common.logout')}
        </button>
      </div>
    </aside>
  );
}
