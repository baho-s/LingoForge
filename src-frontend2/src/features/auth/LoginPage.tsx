import { useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { authApi } from '../../api/endpoints';
import { useAuthStore } from '../../store/auth';
import { useToast } from '../../components/ui/toast-context';
import Button from '../../components/ui/Button';
import Card from '../../components/ui/Card';
import Input from '../../components/ui/Input';
import AuthShell from './AuthShell';
import GuestLoginButton from './GuestLoginButton';
import PasswordInput from './PasswordInput';
import { EMAIL_PATTERN } from './validation';

export default function LoginPage() {
  const { t } = useTranslation();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const login = useAuthStore((s) => s.login);
  const navigate = useNavigate();
  const { addToast } = useToast();

  const validate = (): string | null => {
    if (!EMAIL_PATTERN.test(email.trim())) return t('auth.invalidEmail');
    if (!password) return t('auth.passwordRequired');
    if (password.length < 6) return t('auth.passwordTooShort');
    return null;
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    const validationError = validate();
    if (validationError) {
      setError(validationError);
      return;
    }
    setError('');
    setLoading(true);
    try {
      const { data } = await authApi.login(email, password);
      const token = data.token ?? (data as { Token?: string }).Token;
      if (!token) {
        setError(t('auth.loginFailed'));
        return;
      }
      login(token);
      addToast(t('auth.welcomeBack'), 'success');
      navigate('/');
    } catch {
      setError(t('auth.loginFailed'));
    } finally {
      setLoading(false);
    }
  };

  return (
    <AuthShell subtitle={t('auth.loginSubtitle')}>
      <GuestLoginButton />
      <Card className="md:p-8">
        <form onSubmit={handleSubmit} className="space-y-5" noValidate>
          {error && (
            <div role="alert" className="rounded-xl bg-red-500/10 px-4 py-3 text-sm text-red-600 dark:text-red-400">
              {error}
            </div>
          )}
          <Input
            name="email"
            type="email"
            label={t('auth.email')}
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="sen@ornek.com"
            autoComplete="email"
          />
          <PasswordInput
            name="password"
            label={t('auth.password')}
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder={t('auth.passwordPlaceholder')}
            autoComplete="current-password"
          />
          <Button type="submit" size="lg" fullWidth disabled={loading}>
            {loading ? t('auth.loggingIn') : t('auth.loginButton')}
          </Button>
          <p className="text-center text-sm text-muted">
            {t('auth.noAccount')}{' '}
            <Link to="/register" className="font-semibold text-brand hover:underline">
              {t('auth.register')}
            </Link>
          </p>
        </form>
      </Card>
    </AuthShell>
  );
}
