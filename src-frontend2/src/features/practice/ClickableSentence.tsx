import { useTranslation } from 'react-i18next';
import type { PracticeAiSentenceQuestion } from '../../types';
import type { SentenceHint } from './useSentenceHint';

const SPLIT = /([A-Za-z]+(?:['’-][A-Za-z]+)*)/g;
const IS_WORD = /^[A-Za-z]+(?:['’-][A-Za-z]+)*$/;

interface ClickableSentenceProps {
  question: PracticeAiSentenceQuestion;
  hint: SentenceHint | null;
  onWordClick: (term: string) => void;
}

export default function ClickableSentence({ question, hint, onWordClick }: ClickableSentenceProps) {
  const { t } = useTranslation();
  const sentence = question.english_sentence || question.prompt || '';

  return (
    <div>
      <p className="text-xl font-semibold leading-relaxed text-fg">
        {sentence
          .split(SPLIT)
          .filter(Boolean)
          .map((part, i) =>
            IS_WORD.test(part) ? (
              <button
                key={`${i}-${part}`}
                type="button"
                onClick={() => onWordClick(part)}
                aria-label={`${t('practice.wordHint.show')}: ${part}`}
                aria-pressed={hint?.term === part}
                className={`rounded-sm underline decoration-dotted underline-offset-4 hover:text-brand focus:outline-none focus:ring-2 focus:ring-brand/50 ${
                  hint?.term === part ? 'text-brand' : ''
                }`}
              >
                {part}
              </button>
            ) : (
              <span key={`${i}-${part}`}>{part}</span>
            ),
          )}
      </p>
      <p aria-live="polite" className="mt-3 min-h-5 text-sm text-brand">
        {hint ? (
          <>
            <span className="font-semibold">{hint.term}:</span>{' '}
            {hint.loading ? t('practice.wordHint.loading') : (hint.translation ?? hint.error)}
          </>
        ) : (
          <span className="text-muted">{t('practice.tapWordsHint')}</span>
        )}
      </p>
      {question.target_words_used?.length ? (
        <p className="mt-2 text-xs text-muted">
          {t('practice.targetWords')}: {question.target_words_used.join(', ')}
        </p>
      ) : null}
    </div>
  );
}
