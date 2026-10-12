import { useCallback, useRef, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { practiceApi } from '../../api/endpoints';

export interface SentenceHint {
  term: string;
  translation?: string;
  loading: boolean;
  error?: string;
}

const normalize = (term: string) => term.trim().toLowerCase().replace(/\s+/g, ' ');

// Caches translations per question/term, dedupes in-flight calls and ignores stale responses.
export function useSentenceHint() {
  const { t } = useTranslation();
  const [hint, setHint] = useState<SentenceHint | null>(null);
  const cache = useRef(new Map<string, string>());
  const inFlight = useRef(new Map<string, Promise<string>>());
  const requestId = useRef(0);

  const clear = useCallback(() => {
    requestId.current += 1;
    setHint(null);
  }, []);

  const show = useCallback(
    async (questionId: string, term: string) => {
      const key = `${questionId}:${normalize(term)}`;
      const id = ++requestId.current;

      const cached = cache.current.get(key);
      if (cached) {
        setHint({ term, translation: cached, loading: false });
        return;
      }

      setHint({ term, loading: true });
      try {
        let request = inFlight.current.get(key);
        if (!request) {
          request = practiceApi
            .getSentenceWordHint({ question_id: questionId, term })
            .then(({ data }) => {
              cache.current.set(key, data.translation);
              return data.translation;
            })
            .finally(() => inFlight.current.delete(key));
          inFlight.current.set(key, request);
        }
        const translation = await request;
        if (id === requestId.current) setHint({ term, translation, loading: false });
      } catch {
        if (id === requestId.current) setHint({ term, loading: false, error: t('practice.wordHint.error') });
      }
    },
    [t],
  );

  return { hint, show, clear };
}
