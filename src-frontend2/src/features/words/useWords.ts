import { useCallback, useEffect, useState } from 'react';
import { wordsApi } from '../../api/endpoints';
import type { WordDto } from '../../types';

export const NO_FIELD = '_no_field';
const PAGE_SIZE = 6;

export interface FieldGroup {
  field: string;
  total: number;
  words: WordDto[];
}

export function useWords() {
  const [groups, setGroups] = useState<FieldGroup[]>([]);
  const [loading, setLoading] = useState(true);
  const [failed, setFailed] = useState(false);
  const [loadingMore, setLoadingMore] = useState<string | null>(null);

  const reload = useCallback(async () => {
    try {
      const { data } = await wordsApi.getFields();
      const pages = await Promise.all(
        data.fields.map((f) => wordsApi.getByField(f.field, 0, PAGE_SIZE)),
      );
      setGroups(
        data.fields.map((f, i) => ({ field: f.field, total: f.count, words: pages[i].data.words })),
      );
      setFailed(false);
    } catch {
      setFailed(true);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void reload();
  }, [reload]);

  const loadMore = useCallback(
    async (field: string) => {
      const group = groups.find((g) => g.field === field);
      if (!group) return;
      setLoadingMore(field);
      try {
        const { data } = await wordsApi.getByField(field, group.words.length, PAGE_SIZE);
        setGroups((prev) =>
          prev.map((g) => {
            if (g.field !== field) return g;
            const known = new Set(g.words.map((w) => w.id));
            return {
              ...g,
              total: data.totalCount,
              words: [...g.words, ...data.words.filter((w) => !known.has(w.id))],
            };
          }),
        );
      } finally {
        setLoadingMore(null);
      }
    },
    [groups],
  );

  return { groups, loading, failed, loadingMore, reload, loadMore };
}
