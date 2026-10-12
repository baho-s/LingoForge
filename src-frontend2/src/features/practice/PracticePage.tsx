import { useState } from 'react';
import { AlertCircle, Loader2 } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { practiceApi, wordsApi } from '../../api/endpoints';
import Button from '../../components/ui/Button';
import type { PracticeMode, PracticeQuestion, WordDto } from '../../types';
import PracticeHome, { type PracticeChoice } from './PracticeHome';
import QuestionSession from './QuestionSession';
import ReviewSession from './ReviewSession';

const REVIEW_LIMIT = 8;
const QUESTION_LIMIT = 8;

type View =
  | { kind: 'home' }
  | { kind: 'review'; words: WordDto[]; runId: number }
  | { kind: 'questions'; mode: PracticeMode; questions: PracticeQuestion[]; runId: number };

const MODE_TITLE: Record<PracticeMode, string> = {
  multiple_choice: 'selectCorrectAnswer',
  spelling: 'spellingTitle',
  ai_sentence: 'aiTitle',
};

export default function PracticePage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [view, setView] = useState<View>({ kind: 'home' });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<{ message: string; needsWords: boolean } | null>(null);
  const [runId, setRunId] = useState(0);

  const startReview = async (includeAll = false) => {
    setLoading(true);
    setError(null);
    try {
      const { data } = await wordsApi.getReviewSessionWords(REVIEW_LIMIT, includeAll);
      if (data.length === 0) {
        setError({
          message: t(includeAll ? 'practice.noWordsAvailable' : 'practice.noDueWords'),
          needsWords: includeAll,
        });
        setView({ kind: 'home' });
        return;
      }
      setRunId((n) => n + 1);
      setView({ kind: 'review', words: data, runId: runId + 1 });
    } catch {
      setError({ message: t('common.error'), needsWords: false });
      setView({ kind: 'home' });
    } finally {
      setLoading(false);
    }
  };

  const startQuestions = async (mode: PracticeMode) => {
    setLoading(true);
    setError(null);
    try {
      const { data } = await practiceApi.getQuestions([mode], QUESTION_LIMIT);
      if (!data.questions.length) {
        setError({ message: t('practice.noPracticeWordsAvailable'), needsWords: true });
        setView({ kind: 'home' });
        return;
      }
      setRunId((n) => n + 1);
      setView({ kind: 'questions', mode, questions: data.questions, runId: runId + 1 });
    } catch {
      setError({ message: t('common.error'), needsWords: false });
      setView({ kind: 'home' });
    } finally {
      setLoading(false);
    }
  };

  const select = (choice: PracticeChoice) => (choice === 'review' ? startReview() : startQuestions(choice));
  const exit = () => setView({ kind: 'home' });

  if (view.kind === 'review') {
    return <ReviewSession key={view.runId} words={view.words} onExit={exit} onRestart={startReview} />;
  }

  if (view.kind === 'questions') {
    return (
      <QuestionSession
        key={view.runId}
        questions={view.questions}
        title={t(`practice.${MODE_TITLE[view.mode]}`)}
        onExit={exit}
        onRetry={() => startQuestions(view.mode)}
      />
    );
  }

  return (
    <div className="space-y-4">
      {error && (
        <div role="alert" className="flex items-center gap-2 rounded-xl bg-amber-500/10 px-4 py-3 text-sm text-amber-600 dark:text-amber-400">
          <AlertCircle size={16} />
          {error.message}
          {error.needsWords && (
            <Button size="sm" className="ml-auto" onClick={() => navigate('/words')}>
              {t('practice.addWordsCta')}
            </Button>
          )}
        </div>
      )}
      <PracticeHome disabled={loading} onSelect={select} />
      {loading && <Loader2 size={24} className="mx-auto animate-spin text-brand" />}
    </div>
  );
}
