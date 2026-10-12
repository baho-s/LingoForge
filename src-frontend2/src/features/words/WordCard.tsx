import { Trash2 } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import Badge from '../../components/ui/Badge';
import type { WordDto } from '../../types';
import { getWordStatus } from './wordStatus';

const TONES = { new: 'blue', due: 'amber', ready: 'emerald' } as const;

interface WordCardProps {
  word: WordDto;
  onDelete: (word: WordDto) => void;
}

export default function WordCard({ word, onDelete }: WordCardProps) {
  const { t, i18n } = useTranslation();
  const status = getWordStatus(word);

  return (
    <div className="rounded-2xl border border-line bg-surface p-4">
      <div className="flex items-start justify-between gap-2">
        <div className="min-w-0">
          <p className="truncate text-lg font-bold text-fg">{word.original}</p>
          <p className="truncate text-sm text-muted">{word.translation}</p>
        </div>
        <div className="flex shrink-0 items-center gap-1">
          <Badge tone={TONES[status]}>{t(`words.status.${status}`)}</Badge>
          <button
            type="button"
            onClick={() => onDelete(word)}
            aria-label={t('words.delete')}
            className="rounded-lg p-1.5 text-muted hover:bg-red-500/10 hover:text-red-500"
          >
            <Trash2 size={16} />
          </button>
        </div>
      </div>
      {word.aiSentence && (
        <p className="mt-3 rounded-lg bg-elevated px-3 py-2 text-xs italic text-muted">“{word.aiSentence}”</p>
      )}
      <p className="mt-3 text-xs text-muted">
        {t('words.nextReviewOn', { date: new Date(word.nextReviewAt).toLocaleDateString(i18n.language) })}
      </p>
    </div>
  );
}
