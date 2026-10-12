import { useTranslation } from 'react-i18next';

export default function LanguageToggle() {
  const { t, i18n } = useTranslation();
  const isTr = i18n.language.startsWith('tr');

  return (
    <button
      type="button"
      onClick={() => i18n.changeLanguage(isTr ? 'en' : 'tr')}
      aria-label={t('common.switchLanguage')}
      title={t('common.switchLanguage')}
      className="flex h-10 min-w-10 items-center justify-center rounded-xl border border-line bg-surface px-2 text-xs font-bold text-muted transition-colors hover:text-brand"
    >
      {isTr ? 'EN' : 'TR'}
    </button>
  );
}
