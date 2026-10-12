import { useCallback, useEffect, useState } from 'react';
import { ArrowLeft, Brain } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { wordsApi } from '../../api/endpoints';
import Button from '../../components/ui/Button';
import Card from '../../components/ui/Card';
import ProgressBar from '../../components/ui/ProgressBar';
import { useToast } from '../../components/ui/toast-context';
import { ReviewOutcome, type WordDto } from '../../types';

interface ReviewSessionProps {
  words: WordDto[];
  onExit: () => void;
  onRestart: (includeAll: boolean) => void;
}

const OUTCOMES = [
  { value: ReviewOutcome.Again, label: 'again', desc: 'forgot', style: 'bg-red-500/10 text-red-600 dark:text-red-400' },
  { value: ReviewOutcome.Hard, label: 'hard', desc: 'difficult', style: 'bg-orange-500/10 text-orange-600 dark:text-orange-400' },
  { value: ReviewOutcome.Good, label: 'good', desc: 'isCorrect', style: 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400' },
  { value: ReviewOutcome.Easy, label: 'easy', desc: 'perfect', style: 'bg-blue-500/10 text-blue-600 dark:text-blue-400' },
] as const;

export default function ReviewSession({ words, onExit, onRestart }: ReviewSessionProps) {
  const { t } = useTranslation();
  const { addToast } = useToast();
  const [index, setIndex] = useState(0);
  const [flipped, setFlipped] = useState(false);
  const [reviewed, setReviewed] = useState(0);
  const done = index >= words.length;
  const word = words[index];
  // The back face keeps the previous word until the next flip, so the new answer never shows while the card rotates back.
  const [backWord, setBackWord] = useState(words[0]);

  const toggleFlip = () => {
    if (!flipped) setBackWord(word);
    setFlipped((f) => !f);
  };

  const answer = useCallback(
    async (outcome: ReviewOutcome) => {
      if (!word) return;
      try {
        await wordsApi.review(word.id, { outcome });
      } catch {
        addToast(t('common.error'), 'error');
      }
      const count = reviewed + 1;
      setReviewed(count);
      setFlipped(false);
      setIndex((i) => i + 1);
      if (index + 1 >= words.length) addToast(t('practice.reviewedToast', { count }), 'success');
    },
    [word, reviewed, index, words.length, addToast, t],
  );

  useEffect(() => {
    if (!flipped) return;
    const onKey = (e: KeyboardEvent) => {
      const outcome = OUTCOMES[Number(e.key) - 1];
      if (outcome) void answer(outcome.value);
    };
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
  }, [flipped, answer]);

  if (done) {
    const allReviewed = reviewed === words.length;
    return (
      <Card className="mx-auto max-w-2xl p-10 text-center">
        <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-emerald-500/15 text-emerald-500">
          <Brain size={32} />
        </div>
        <p className="text-2xl font-bold">{t('practice.sessionCompleteTitle')}</p>
        <p className="mb-6 mt-2 text-sm text-muted">
          {allReviewed
            ? t('practice.allWordsReviewed')
            : `${t('practice.youReviewed')} ${reviewed} ${t('practice.of')} ${words.length} ${t('practice.words')}.`}
        </p>
        <div className="flex justify-center gap-2">
          <Button variant="secondary" onClick={onExit}>
            {t('practice.backToPractice')}
          </Button>
          <Button onClick={() => onRestart(allReviewed)}>{t('practice.reviewAgain')}</Button>
        </div>
      </Card>
    );
  }

  const face = 'absolute inset-0 flex flex-col items-center justify-center rounded-2xl border border-line bg-surface p-8 text-center [backface-visibility:hidden]';

  return (
    <div className="mx-auto max-w-2xl space-y-4">
      <button type="button" onClick={onExit} className="flex items-center gap-2 text-sm text-muted hover:text-fg">
        <ArrowLeft size={16} /> {t('practice.back')}
      </button>
      <div className="flex items-center gap-3">
        <span className="whitespace-nowrap text-sm text-muted">
          {t('practice.progress', { current: index + 1, total: words.length })}
        </span>
        <ProgressBar value={index + 1} max={words.length} />
      </div>

      <button
        type="button"
        onClick={toggleFlip}
        aria-label={t('practice.clickToReveal')}
        className="block h-64 w-full [perspective:1000px]"
      >
        <div
          className="relative h-full w-full transition-transform duration-500 [transform-style:preserve-3d]"
          style={{ transform: flipped ? 'rotateY(180deg)' : 'rotateY(0deg)' }}
        >
          <div className={face}>
            <p className="text-3xl font-bold text-fg">{word.original}</p>
            <p className="mt-3 text-sm text-muted">{t('practice.clickToReveal')}</p>
          </div>
          <div className={`${face} [transform:rotateY(180deg)]`}>
            <p className="text-2xl font-bold text-brand">{backWord.translation}</p>
            {backWord.aiSentence && <p className="mt-3 text-sm italic text-muted">“{backWord.aiSentence}”</p>}
          </div>
        </div>
      </button>

      {flipped && (
        <div className="grid grid-cols-4 gap-2">
          {OUTCOMES.map((o) => (
            <button
              key={o.value}
              type="button"
              onClick={() => void answer(o.value)}
              className={`rounded-xl py-3 text-sm font-semibold transition hover:brightness-110 ${o.style}`}
            >
              {t(`practice.${o.label}`)}
              <span className="block text-xs font-normal opacity-70">{t(`practice.${o.desc}`)}</span>
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
