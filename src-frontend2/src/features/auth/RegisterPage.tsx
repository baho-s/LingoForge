import { useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Check, X } from 'lucide-react';
import { authApi } from '../../api/endpoints';
import { useAuthStore } from '../../store/auth';
import { useToast } from '../../components/ui/toast-context';
import Button from '../../components/ui/Button';
import Card from '../../components/ui/Card';
import Input from '../../components/ui/Input';
import AuthShell from './AuthShell';
import GuestLoginButton from './GuestLoginButton';
import PasswordInput from './PasswordInput';
import { EMAIL_PATTERN, checkPassword, type PasswordChecks } from './validation';

const STRENGTH = [
  { key: 'weak', color: 'bg-red-500', width: '25%' },
  { key: 'weak', color: 'bg-red-500', width: '25%' },
  { key: 'medium', color: 'bg-orange-500', width: '50%' },
  { key: 'good', color: 'bg-yellow-500', width: '75%' },
  { key: 'strong', color: 'bg-emerald-500', width: '100%' },
] as const;

const RULES: (keyof PasswordChecks)[] = ['length', 'uppercase', 'lowercase', 'number'];

export default function RegisterPage() {
  const { t } = useTranslation();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const login = useAuthStore((s) => s.login);
  const navigate = useNavigate();
  const { addToast } = useToast();

  const checks = checkPassword(password);
  const strength = STRENGTH[Object.values(checks).filter(Boolean).length];
  const mismatch = confirmPassword.length > 0 && password !== confirmPassword;

  const validate = (): string | null => {
    if (!EMAIL_PATTERN.test(email.trim())) return t('auth.invalidEmail');
    if (!password) return t('auth.passwordRequired');
    if (password.length < 6) return t('auth.passwordTooShort');
    if (password !== confirmPassword) return t('auth.passwordsMustMatch');
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
      const { data } = await authApi.register(email, password);
      const token = data.token ?? (data as { Token?: string }).Token;
      if (!token) {
        setError(t('auth.registerFailed'));
        return;
      }
      login(token);
      addToast(t('auth.accountCreated'), 'success');
      navigate('/');
    } catch {
      setError(t('auth.registerFailed'));
    } finally {
      setLoading(false);
    }
  };

  return (
    <AuthShell subtitle={t('auth.registerSubtitle')}>
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
          <div>
            <PasswordInput
              name="password"
              label={t('auth.password')}
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder={t('auth.createPasswordPlaceholder')}
              autoComplete="new-password"
            />
            {password && (
              <div className="mt-3 space-y-2">
                <div className="flex items-center gap-2">
                  <div className="h-1.5 flex-1 rounded-full bg-elevated">
                    <div
                      className={`h-1.5 rounded-full transition-all ${strength.color}`}
                      style={{ width: strength.width }}
                    />
                  </div>
                  <span className="text-xs text-muted">{t(`auth.strength.${strength.key}`)}</span>
                </div>
                <div className="grid grid-cols-2 gap-1">
                  {RULES.map((rule) => (
                    <div key={rule} className="flex items-center gap-1.5 text-xs">
                      {checks[rule] ? (
                        <Check size={12} className="text-emerald-500" />
                      ) : (
                        <X size={12} className="text-muted" />
                      )}
                      <span className={checks[rule] ? 'text-emerald-500' : 'text-muted'}>
                        {t(`auth.rules.${rule}`)}
                      </span>
                    </div>
                  ))}
                </div>
              </div>
            )}
          </div>
          <div>
            <PasswordInput
              name="confirmPassword"
              label={t('auth.confirmPassword')}
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              placeholder={t('auth.confirmPlaceholder')}
              autoComplete="new-password"
              invalid={mismatch}
            />
            {mismatch && <p className="mt-1 text-xs text-red-500">{t('auth.passwordsMustMatch')}</p>}
          </div>
          <Button type="submit" size="lg" fullWidth disabled={loading || mismatch}>
            {loading ? t('auth.registering') : t('auth.registerButton')}
          </Button>
          <p className="text-center text-sm text-muted">
            {t('auth.haveAccount')}{' '}
            <Link to="/login" className="font-semibold text-brand hover:underline">
              {t('auth.login')}
            </Link>
          </p>
        </form>
      </Card>
    </AuthShell>
  );
}
