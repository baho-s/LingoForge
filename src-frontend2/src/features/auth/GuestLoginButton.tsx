import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { UserRound } from 'lucide-react';
import { authApi } from '../../api/endpoints';
import { useAuthStore } from '../../store/auth';
import { useToast } from '../../components/ui/toast-context';
import Button from '../../components/ui/Button';

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
      {error && (
        <div role="alert" className="rounded-xl bg-red-500/10 px-4 py-3 text-sm text-red-600 dark:text-red-400">
          {error}
        </div>
      )}
      <Button size="lg" fullWidth onClick={handleClick} disabled={loading}>
        <UserRound size={18} />
        {loading ? t('auth.guest.starting') : t('auth.guest.start')}
      </Button>
      <p className="text-center text-xs text-muted">{t('auth.guest.hint')}</p>
      <div className="flex items-center gap-3 pt-2">
        <div className="flex-1 border-t border-line" />
        <span className="text-xs text-muted">{t('auth.guest.or')}</span>
        <div className="flex-1 border-t border-line" />
      </div>
    </div>
  );
}
