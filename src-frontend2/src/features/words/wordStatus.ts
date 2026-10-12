import type { WordDto } from '../../types';

export type WordStatus = 'new' | 'due' | 'ready';
export type SortKey = 'createdAt' | 'original' | 'nextReviewAt';

export function getWordStatus(word: WordDto, now = Date.now()): WordStatus {
  if (word.repetitions === 0) return 'new';
  return new Date(word.nextReviewAt).getTime() <= now ? 'due' : 'ready';
}

export function compareWords(a: WordDto, b: WordDto, key: SortKey): number {
  if (key === 'original') return a.original.localeCompare(b.original);
  if (key === 'nextReviewAt') return new Date(a.nextReviewAt).getTime() - new Date(b.nextReviewAt).getTime();
  return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
}
