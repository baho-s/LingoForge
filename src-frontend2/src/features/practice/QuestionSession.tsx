import { useState } from 'react';
import { ArrowLeft } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import Button from '../../components/ui/Button';
import Card from '../../components/ui/Card';
import ProgressBar from '../../components/ui/ProgressBar';
import type { PracticeQuestion } from '../../types';
import QuestionView from './QuestionView';

interface QuestionSessionProps {
  questions: PracticeQuestion[];
  title: string;
  onExit: () => void;
  onRetry: () => void;
}

export default function QuestionSession({ questions, title, onExit, onRetry }: QuestionSessionProps) {
  const { t } = useTranslation();
  const [index, setIndex] = useState(0);
  const [correct, setCorrect] = useState(0);
  const total = questions.length;
  const done = index >= total;
  const percent = Math.round((correct / total) * 100);

  return (
    <div className="mx-auto max-w-2xl space-y-4">
      <button type="button" onClick={onExit} className="flex items-center gap-2 text-sm text-muted hover:text-fg">
        <ArrowLeft size={16} /> {t('practice.back')}
      </button>

      {done ? (
        <Card className="p-10 text-center">
          <p className="text-5xl font-bold text-brand">
            {correct}/{total}
          </p>
          <p className="mt-2 font-semibold">
            {correct === total
              ? t('practice.perfectScore')
              : correct >= total / 2
                ? t('practice.goodJob')
                : t('practice.keepPracticing')}
          </p>
          <p className="mb-6 mt-1 text-sm text-muted">
            {percent}% {t('practice.accuracy')}
          </p>
          <div className="flex justify-center gap-2">
            <Button variant="secondary" onClick={onExit}>
              {t('practice.backToPractice')}
            </Button>
            <Button onClick={onRetry}>{t('practice.tryAgainBtn')}</Button>
          </div>
        </Card>
      ) : (
        <>
          <div className="flex items-center justify-between text-sm">
            <span className="text-muted">
              {t('practice.question')} {t('practice.progress', { current: index + 1, total })}
            </span>
            <span className="flex items-center gap-3">
              <span className="text-xs font-semibold uppercase tracking-wide text-muted">{title}</span>
              <span className="font-semibold text-brand">
                {correct} {t('practice.isCorrect').toLowerCase()}
              </span>
            </span>
          </div>
          <ProgressBar value={index + 1} max={total} />
          <QuestionView
            key={questions[index].id}
            question={questions[index]}
            onAnswered={(ok) => ok && setCorrect((c) => c + 1)}
            onNext={() => setIndex((i) => i + 1)}
          />
        </>
      )}
    </div>
  );
}
