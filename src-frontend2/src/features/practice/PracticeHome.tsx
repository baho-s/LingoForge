import type { ReactNode } from 'react';
import { Brain, ListChecks, PenSquare, Sparkles } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import type { PracticeMode } from '../../types';

export type PracticeChoice = 'review' | PracticeMode;

interface PracticeHomeProps {
  disabled: boolean;
  onSelect: (choice: PracticeChoice) => void;
}

const OPTIONS: { choice: PracticeChoice; icon: ReactNode; title: string; desc: string; tone: string }[] = [
  { choice: 'review', icon: <Brain size={24} />, title: 'reviewTitle', desc: 'reviewDesc', tone: 'bg-brand/15 text-brand' },
  { choice: 'multiple_choice', icon: <ListChecks size={24} />, title: 'selectCorrectAnswer', desc: 'multipleDesc', tone: 'bg-emerald-500/15 text-emerald-500' },
  { choice: 'spelling', icon: <PenSquare size={24} />, title: 'spellingTitle', desc: 'spellingDesc', tone: 'bg-blue-500/15 text-blue-500' },
  { choice: 'ai_sentence', icon: <Sparkles size={24} />, title: 'aiTitle', desc: 'aiDesc', tone: 'bg-purple-500/15 text-purple-500' },
];

export default function PracticeHome({ disabled, onSelect }: PracticeHomeProps) {
  const { t } = useTranslation();
  return (
    <div className="space-y-5">
      <h2 className="text-2xl font-bold">{t('practice.practice')}</h2>
      <div className="grid gap-3 sm:grid-cols-2">
        {OPTIONS.map((o) => (
          <button
            key={o.choice}
            type="button"
            disabled={disabled}
            onClick={() => onSelect(o.choice)}
            className="rounded-2xl border border-line bg-surface p-5 text-left transition hover:border-brand disabled:opacity-60"
          >
            <div className={`mb-3 flex h-12 w-12 items-center justify-center rounded-xl ${o.tone}`}>{o.icon}</div>
            <h3 className="font-semibold text-fg">{t(`practice.${o.title}`)}</h3>
            <p className="mt-1 text-sm text-muted">{t(`practice.${o.desc}`)}</p>
          </button>
        ))}
      </div>
    </div>
  );
}
