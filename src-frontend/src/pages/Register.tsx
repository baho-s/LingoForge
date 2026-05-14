import { useState, useMemo } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Eye, EyeOff, Check, X } from 'lucide-react';
import { authApi } from '../api/endpoints';
import { useAuthStore } from '../store/auth';
import { useToast } from '../components/Toast';

export default function Register() {
  const { t } = useTranslation();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const login = useAuthStore((s) => s.login);
  const navigate = useNavigate();
  const { addToast } = useToast();

  const passwordChecks = useMemo(() => ({
    length: password.length >= 6,
    uppercase: /[A-Z]/.test(password),
    lowercase: /[a-z]/.test(password),
    number: /[0-9]/.test(password),
  }), [password]);

  const passwordStrength = useMemo(() => {
    const score = Object.values(passwordChecks).filter(Boolean).length;
    if (score <= 1) return { label: 'Zayıf', color: 'bg-red-500', width: '25%' };
    if (score === 2) return { label: 'Orta', color: 'bg-orange-500', width: '50%' };
    if (score === 3) return { label: 'İyi', color: 'bg-yellow-500', width: '75%' };
    return { label: 'Güçlü', color: 'bg-green-500', width: '100%' };
  }, [passwordChecks]);

  const validate = (): string | null => {
    if (!email.trim()) return t('auth.invalidEmail');
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) return t('auth.invalidEmail');
    if (!password) return 'Şifre gerekli.';
    if (password.length < 6) return t('auth.passwordTooShort');
    if (password !== confirmPassword) return t('auth.passwordsMustMatch');
    return null;
  };

  const handleSubmit = async (e: React.FormEvent) => {
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
      addToast('Hesap başarıyla oluşturuldu!', 'success');
      navigate('/');
    } catch {
      setError(t('auth.registerFailed'));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center px-4">
      <div className="w-full max-w-md">
        <div className="text-center mb-8">
          <h1 className="text-3xl font-bold text-gray-900">VocabApp</h1>
          <p className="text-gray-500 mt-2">Hesabını oluştur</p>
        </div>
        <form onSubmit={handleSubmit} className="bg-white rounded-2xl shadow-sm p-8 space-y-5">
          {error && (
            <div className="bg-red-50 text-red-700 text-sm rounded-xl px-4 py-3">{error}</div>
          )}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1.5">{t('auth.email')}</label>
            <input
              type="email"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full px-4 py-3 rounded-xl border border-gray-200 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition"
              placeholder="sen@ornek.com"
              autoFocus
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1.5">{t('auth.password')}</label>
            <div className="relative">
              <input
                type={showPassword ? 'text' : 'password'}
                required
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className="w-full px-4 py-3 pr-12 rounded-xl border border-gray-200 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition"
                placeholder="Create a password"
              />
              <button
                type="button"
                onClick={() => setShowPassword(!showPassword)}
                className="absolute right-4 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600"
              >
                {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
              </button>
            </div>
            {password && (
              <div className="mt-2 space-y-2">
                <div className="flex items-center gap-2">
                  <div className="flex-1 bg-gray-100 rounded-full h-1.5">
                    <div className={`h-1.5 rounded-full transition-all ${passwordStrength.color}`} style={{ width: passwordStrength.width }} />
                  </div>
                  <span className="text-xs text-gray-500">{passwordStrength.label}</span>
                </div>
                <div className="grid grid-cols-2 gap-1">
                  {[
                    { label: '6+ karakter', met: passwordChecks.length },
                    { label: 'Büyük harf', met: passwordChecks.uppercase },
                    { label: 'Küçük harf', met: passwordChecks.lowercase },
                    { label: 'Sayı', met: passwordChecks.number },
                  ].map((check) => (
                    <div key={check.label} className="flex items-center gap-1.5 text-xs">
                      {check.met ? <Check size={12} className="text-green-500" /> : <X size={12} className="text-gray-300" />}
                      <span className={check.met ? 'text-green-600' : 'text-gray-400'}>{check.label}</span>
                    </div>
                  ))}
                </div>
              </div>
            )}
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1.5">{t('auth.confirmPassword')}</label>
            <input
              type="password"
              required
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              className={`w-full px-4 py-3 rounded-xl border text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition ${
                confirmPassword && password !== confirmPassword ? 'border-red-300' : 'border-gray-200'
              }`}
              placeholder="Şifreni onayla"
            />
            {confirmPassword && password !== confirmPassword && (
              <p className="text-xs text-red-500 mt-1">Şifreler eşleşmiyor</p>
            )}
          </div>
          <button
            type="submit"
            disabled={loading || (confirmPassword.length > 0 && password !== confirmPassword)}
            className="w-full py-3 bg-blue-600 text-white rounded-xl font-medium text-sm hover:bg-blue-700 transition-colors disabled:opacity-50"
          >
            {loading ? 'Hesap oluşturuluyor...' : t('auth.registerButton')}
          </button>
          <p className="text-center text-sm text-gray-500">
            {t('auth.haveAccount')} <Link to="/login" className="text-blue-600 hover:text-blue-700 font-medium">{t('auth.login')}</Link>
          </p>
        </form>
      </div>
    </div>
  );
}
