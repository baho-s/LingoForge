import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { UserRound } from 'lucide-react';
import { authApi } from '../api/endpoints';
import { useAuthStore } from '../store/auth';
import { useToast } from './Toast';

export default function GuestLoginButton() {
  const { t } = useTranslation();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const login = useAuthStore((s) => s.login);
  const navigate = useNavigate();
  const { addToast } = useToast();

  const handleClick = async () => {
    setError('');
    setLoading(true);
    try {
      const { data } = await authApi.startGuest();
      const token = data.token ?? (data as { Token?: string }).Token;
      if (!token) {
        setError(t('auth.guest.failed'));
        return;
      }
      login(token);
      addToast(t('auth.guest.welcome'), 'success');
      navigate('/');
    } catch {
      setError(t('auth.guest.failed'));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="mb-6 space-y-2">
      {error && <div className="bg-red-50 text-red-700 text-sm rounded-xl px-4 py-3">{error}</div>}
      <button
        type="button"
        onClick={handleClick}
        disabled={loading}
        className="w-full py-3 bg-amber-500 text-white rounded-xl font-medium text-sm hover:bg-amber-600 transition-colors disabled:opacity-50 flex items-center justify-center gap-2"
      >
        <UserRound size={18} />
        {loading ? t('auth.guest.starting') : t('auth.guest.start')}
      </button>
      <p className="text-center text-xs text-gray-500">{t('auth.guest.hint')}</p>
      <div className="flex items-center gap-3 pt-2">
        <div className="flex-1 border-t border-gray-200" />
        <span className="text-xs text-gray-400">{t('auth.guest.or')}</span>
        <div className="flex-1 border-t border-gray-200" />
      </div>
    </div>
  );
}
