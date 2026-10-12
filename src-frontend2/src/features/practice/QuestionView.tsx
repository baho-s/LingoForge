import { useRef, useState } from 'react';
import { Loader2 } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { practiceApi } from '../../api/endpoints';
import Button from '../../components/ui/Button';
import Card from '../../components/ui/Card';
import { useToast } from '../../components/ui/toast-context';
import type { PracticeAnswerResponse, PracticeQuestion } from '../../types';
import ClickableSentence from './ClickableSentence';
import { useSentenceHint } from './useSentenceHint';

interface QuestionViewProps {
  question: PracticeQuestion;
  onAnswered: (isCorrect: boolean) => void;
  onNext: () => void;
}

const normalize = (v: string) => v.trim().toLowerCase();

// Mounted with key=question.id, so all state (answer, hint, timer) resets per question.
export default function QuestionView({ question, onAnswered, onNext }: QuestionViewProps) {
  const { t } = useTranslation();
  const { addToast } = useToast();
  const { hint, show } = useSentenceHint();
  const startedAt = useRef(Date.now());
  const [answer, setAnswer] = useState('');
  const [selected, setSelected] = useState<string | null>(null);
  const [submitted, setSubmitted] = useState(false);
  const [busy, setBusy] = useState(false);
  const [feedback, setFeedback] = useState<PracticeAnswerResponse | null>(null);

  const send = async (userAnswer: string) => {
    return practiceApi.submitAnswer({
      question_id: question.id,
      type: question.type,
      user_answer: userAnswer,
      direction: question.direction,
      time_taken_ms: Date.now() - startedAt.current,
    });
  };

  // Local-judged modes show feedback instantly, then let the server response refine it.
  const submitLocal = async (userAnswer: string, correctAnswer: string) => {
    const isCorrect = normalize(userAnswer) === normalize(correctAnswer);
    setSubmitted(true);
    setFeedback({ is_correct: isCorrect });
    onAnswered(isCorrect);
    try {
      const { data } = await send(userAnswer);
      setFeedback(data);
    } catch {
      addToast(t('common.error'), 'error');
    }
  };

  const submitAi = async () => {
    setSubmitted(true);
    setBusy(true);
    try {
      const { data } = await send(answer);
      setFeedback(data);
      onAnswered(data.is_correct);
    } catch {
      setFeedback({ is_correct: false, feedback: t('practice.answerFailed') });
      addToast(t('common.error'), 'error');
    } finally {
      setBusy(false);
    }
  };

  const correctAnswer =
    feedback?.correct_answer ?? (question.type === 'ai_sentence' ? undefined : question.correct_answer);

  return (
    <div className="space-y-4">
      <Card className="p-6">
        <p className="mb-2 text-xs font-semibold text-muted">
          {question.direction === 'TR_TO_EN' ? 'TR → EN' : 'EN → TR'}
        </p>
        {question.type === 'ai_sentence' ? (
          <ClickableSentence question={question} hint={hint} onWordClick={(term) => show(question.id, term)} />
        ) : (
          <p className="text-center text-2xl font-bold text-fg">{question.prompt}</p>
        )}
      </Card>

      {question.type === 'multiple_choice' && (
        <div className="space-y-2">
          {question.options.map((option) => {
            let style = 'border-line bg-surface hover:border-brand';
            if (submitted && option === question.correct_answer) style = 'border-emerald-500 bg-emerald-500/10';
            else if (submitted && option === selected) style = 'border-red-500 bg-red-500/10';
            return (
              <button
                key={option}
                type="button"
                disabled={submitted}
                onClick={() => {
                  setSelected(option);
                  void submitLocal(option, question.correct_answer);
                }}
                className={`w-full rounded-xl border px-4 py-3.5 text-left text-sm font-medium text-fg transition disabled:cursor-default ${style}`}
              >
                {option}
              </button>
            );
          })}
        </div>
      )}

      {question.type === 'text_input' && (
        <div className="space-y-3">
          <input
            value={answer}
            onChange={(e) => setAnswer(e.target.value)}
            disabled={submitted}
            placeholder={t('practice.typeAnswer')}
            aria-label={t('practice.typeAnswer')}
            className="h-12 w-full rounded-xl border border-line bg-elevated px-4 text-sm text-fg focus:outline-none focus:ring-2 focus:ring-brand/50"
          />
          <Button onClick={() => void submitLocal(answer, question.correct_answer)} disabled={submitted || !answer.trim()}>
            {t('practice.checkAnswer')}
          </Button>
        </div>
      )}

      {question.type === 'ai_sentence' && (
        <div className="space-y-3">
          <textarea
            value={answer}
            onChange={(e) => setAnswer(e.target.value)}
            disabled={submitted}
            rows={4}
            placeholder={t('practice.writeMeaning')}
            aria-label={t('practice.writeMeaning')}
            className="w-full rounded-xl border border-line bg-elevated p-4 text-sm text-fg focus:outline-none focus:ring-2 focus:ring-brand/50"
          />
          <Button onClick={() => void submitAi()} disabled={submitted || !answer.trim() || busy}>
            {busy && <Loader2 size={16} className="animate-spin" />}
            {t('practice.submitAnswer')}
          </Button>
        </div>
      )}

      {submitted && feedback && !busy && (
        <div
          role="status"
          className={`rounded-xl px-4 py-3 text-sm ${
            feedback.is_correct
              ? 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400'
              : 'bg-red-500/10 text-red-600 dark:text-red-400'
          }`}
        >
          <p className="font-semibold">{feedback.is_correct ? t('practice.isCorrect') : t('practice.isIncorrect')}</p>
          {feedback.feedback && question.type === 'ai_sentence' && (
            <p className="mt-1 text-xs opacity-90">{feedback.feedback}</p>
          )}
          {!feedback.is_correct && correctAnswer && (
            <p className="mt-1 text-xs opacity-90">
              {t('practice.correctAnswer')}: {correctAnswer}
            </p>
          )}
          {feedback.accuracy_score !== undefined && (
            <p className="mt-1 text-xs opacity-90">
              {t('practice.accuracyScore')}: {feedback.accuracy_score}%
            </p>
          )}
        </div>
      )}

      {submitted && !busy && (
        <div className="flex justify-end">
          <Button onClick={onNext}>{t('practice.nextQuestion')}</Button>
        </div>
      )}
    </div>
  );
}
