import type { ReactNode } from 'react';

type Tone = 'amber' | 'emerald' | 'blue' | 'slate' | 'red';

const tones: Record<Tone, string> = {
  amber: 'bg-amber-500/15 text-amber-600 dark:text-amber-400',
  emerald: 'bg-emerald-500/15 text-emerald-600 dark:text-emerald-400',
  blue: 'bg-blue-500/15 text-blue-600 dark:text-blue-400',
  slate: 'bg-slate-500/15 text-muted',
  red: 'bg-red-500/15 text-red-600 dark:text-red-400',
};

export default function Badge({ tone = 'slate', children }: { tone?: Tone; children: ReactNode }) {
  return (
    <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-semibold ${tones[tone]}`}>
      {children}
    </span>
  );
}
