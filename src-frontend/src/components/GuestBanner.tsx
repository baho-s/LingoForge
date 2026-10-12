import { useCallback, useEffect, useState } from 'react';
import { useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { authApi } from '../api/endpoints';
import { GUEST_AI_LIMIT_EVENT } from '../api/client';
import type { CurrentUserResponse } from '../types';
import { useToast } from './Toast';

const MS_PER_DAY = 24 * 60 * 60 * 1000;

export default function GuestBanner() {
  const { t } = useTranslation();
  const { addToast } = useToast();
  const location = useLocation();
  const [me, setMe] = useState<CurrentUserResponse | null>(null);

  const refresh = useCallback(() => {
    authApi
      .me()
      .then(({ data }) => setMe(data))
      .catch(() => setMe(null));
  }, []);

  useEffect(() => {
    refresh();
  }, [refresh, location.pathname]);

  useEffect(() => {
    const handleLimit = () => {
      addToast(t('auth.guest.limitReached'), 'error');
      refresh();
    };
    window.addEventListener(GUEST_AI_LIMIT_EVENT, handleLimit);
    return () => window.removeEventListener(GUEST_AI_LIMIT_EVENT, handleLimit);
  }, [addToast, refresh, t]);

  if (!me?.isGuest) return null;

  const expiresAt = me.guestExpiresAt ? new Date(me.guestExpiresAt).getTime() : Date.now();
  const days = Math.max(0, Math.ceil((expiresAt - Date.now()) / MS_PER_DAY));

  return (
    <div className="bg-amber-50 border-b border-amber-200 text-amber-800 text-xs md:text-sm px-4 py-2 text-center">
      {t('auth.guest.banner', { days, remaining: me.aiRemaining ?? 0, limit: me.aiLimit ?? 0 })}
    </div>
  );
}
