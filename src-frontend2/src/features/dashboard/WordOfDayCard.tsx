import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { BookOpen, Plus, Sparkles, Volume2 } from 'lucide-react';
import type { WordDto } from '../../types';
import Badge from '../../components/ui/Badge';
import Button from '../../components/ui/Button';
import Card from '../../components/ui/Card';

function speak(text: string) {
  if (!('speechSynthesis' in window)) return;
  window.speechSynthesis.cancel();
  const utterance = new SpeechSynthesisUtterance(text);
  utterance.lang = 'en-US';
  window.speechSynthesis.speak(utterance);
}

export default function WordOfDayCard({ word }: { word: WordDto | null }) {
  const { t } = useTranslation();
  const navigate = useNavigate();

  if (!word) {
    return (
      <Card className="text-center">
        <div className="mx-auto mb-3 flex h-12 w-12 items-center justify-center rounded-full bg-brand/15 text-brand">
          <BookOpen size={20} />
        </div>
        <p className="text-lg font-semibold">{t('dashboard.emptyTitle')}</p>
        <p className="mb-4 mt-1 text-sm text-muted">{t('dashboard.emptyDesc')}</p>
        <Button onClick={() => navigate('/words')}>
          <Plus size={16} /> {t('dashboard.addWord')}
        </Button>
      </Card>
    );
  }

  return (
    <Card className="relative overflow-hidden">
      <div className="pointer-events-none absolute -right-10 -top-10 h-40 w-40 rounded-full bg-brand/10 blur-2xl" />
      <div className="relative">
        <div className="mb-4 flex items-center gap-2">
          <Sparkles size={18} className="text-brand" />
          <span className="text-sm font-semibold text-brand">{t('dashboard.wordOfTheDay')}</span>
          <span className="ml-auto">
            <Badge tone="amber">{t('dashboard.new')}</Badge>
          </span>
        </div>
        <div className="flex flex-col gap-4 md:flex-row md:items-center">
          <div>
            <div className="flex items-center gap-3">
              <p className="text-3xl font-extrabold tracking-tight">{word.original}</p>
              <button
                type="button"
                onClick={() => speak(word.original)}
                aria-label={t('dashboard.listen')}
                title={t('dashboard.listen')}
                className="flex h-10 w-10 items-center justify-center rounded-full bg-brand/15 text-brand transition-colors hover:bg-brand/25"
              >
                <Volume2 size={18} />
              </button>
            </div>
            <p className="mt-1 text-muted">{word.translation}</p>
          </div>
          {word.aiSentence && (
            <p className="rounded-xl bg-elevated px-4 py-3 text-sm italic text-muted md:ml-auto md:max-w-sm">
              “{word.aiSentence}”
            </p>
          )}
        </div>
      </div>
    </Card>
  );
}
