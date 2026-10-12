import { useCallback, useMemo, useState } from 'react';
import { BookOpen, Download, Loader2, Plus, Search, Sparkles, Trash2 } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { wordsApi } from '../../api/endpoints';
import Button from '../../components/ui/Button';
import Input from '../../components/ui/Input';
import Select from '../../components/ui/Select';
import Skeleton from '../../components/ui/Skeleton';
import { useToast } from '../../components/ui/toast-context';
import type { WordDto } from '../../types';
import ConfirmDeleteModal from './ConfirmDeleteModal';
import FieldImportModal from './FieldImportModal';
import WordAddModal, { type WordAddResult } from './WordAddModal';
import WordCard from './WordCard';
import { compareWords, getWordStatus, type SortKey, type WordStatus } from './wordStatus';
import { NO_FIELD, useWords } from './useWords';

type Filter = 'all' | WordStatus;
const FILTERS: Filter[] = ['all', 'new', 'due', 'ready'];
const SORT_KEYS: SortKey[] = ['createdAt', 'original', 'nextReviewAt'];

export default function WordsPage() {
  const { t } = useTranslation();
  const { addToast } = useToast();
  const { groups, loading, failed, loadingMore, reload, loadMore } = useWords();

  const [search, setSearch] = useState('');
  const [filter, setFilter] = useState<Filter>('all');
  const [sortKey, setSortKey] = useState<SortKey>('createdAt');
  const [showAdd, setShowAdd] = useState(false);
  const [showImport, setShowImport] = useState(false);
  const [wordToDelete, setWordToDelete] = useState<WordDto | null>(null);
  const [fieldToDelete, setFieldToDelete] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const [generating, setGenerating] = useState(false);

  const showError = useCallback((msg: string) => addToast(msg, 'error'), [addToast]);
  const fieldLabel = (f: string) => (f === NO_FIELD ? t('words.myOwnWords') : f);

  const visibleGroups = useMemo(() => {
    const q = search.trim().toLowerCase();
    return groups
      .map((g) => ({
        ...g,
        words: g.words
          .filter(
            (w) =>
              (!q || w.original.toLowerCase().includes(q) || w.translation.toLowerCase().includes(q)) &&
              (filter === 'all' || getWordStatus(w) === filter),
          )
          .sort((a, b) => compareWords(a, b, sortKey)),
      }))
      .filter((g) => g.words.length > 0);
  }, [groups, search, filter, sortKey]);

  const totalCount = groups.reduce((s, g) => s + g.total, 0);
  const dueCount = groups.reduce((s, g) => s + g.words.filter((w) => getWordStatus(w) === 'due').length, 0);

  const handleAdded = async (r: WordAddResult) => {
    await reload();
    if (r.mode === 'single') {
      addToast(t('words.addWordSuccess'), 'success');
      return;
    }
    const parts = [t('words.addedMany', { created: r.createdCount })];
    if (r.generatedSentenceCount) parts.push(t('words.addedSentences', { count: r.generatedSentenceCount }));
    if (r.skippedLineCount) parts.push(t('words.addedSkipped', { count: r.skippedLineCount }));
    addToast(parts.join(', '), 'success');
  };

  const handleImported = async (field: string, count: number) => {
    await reload();
    addToast(t('words.importedFrom', { count, field }), 'success');
  };

  const handleGenerate = async () => {
    setGenerating(true);
    try {
      const { data } = await wordsApi.bulkGenerate();
      await reload();
      addToast(t('words.generatedResult', { generated: data.generated, skipped: data.skipped }), 'success');
    } catch {
      showError(t('common.error'));
    } finally {
      setGenerating(false);
    }
  };

  const confirmDeleteWord = async () => {
    if (!wordToDelete) return;
    setBusy(true);
    try {
      await wordsApi.delete(wordToDelete.id);
      addToast(t('words.deleteWordSuccess'), 'success');
      setWordToDelete(null);
      await reload();
    } catch {
      showError(t('common.error'));
    } finally {
      setBusy(false);
    }
  };

  const confirmDeleteField = async () => {
    if (!fieldToDelete) return;
    setBusy(true);
    try {
      const { data } = await wordsApi.bulkDeleteByField(fieldToDelete);
      addToast(t('words.deletedMany', { count: data.deletedCount }), 'success');
      setFieldToDelete(null);
      await reload();
    } catch {
      showError(t('common.error'));
    } finally {
      setBusy(false);
    }
  };

  if (failed) {
    return (
      <div role="alert" className="rounded-xl bg-red-500/10 px-4 py-3 text-sm text-red-600 dark:text-red-400">
        {t('common.error')}
      </div>
    );
  }

  return (
    <div className="space-y-5">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h2 className="text-2xl font-bold">{t('words.myWords')}</h2>
          {!loading && <p className="text-sm text-muted">{t('words.summary', { total: totalCount, due: dueCount })}</p>}
        </div>
        <div className="flex flex-wrap gap-2">
          <Button variant="secondary" size="sm" onClick={() => setShowImport(true)}>
            <Download size={16} />
            {t('words.importField')}
          </Button>
          <Button variant="secondary" size="sm" onClick={handleGenerate} disabled={generating}>
            {generating ? <Loader2 size={16} className="animate-spin" /> : <Sparkles size={16} />}
            {generating ? t('words.generatingSentences') : t('words.generateSentences')}
          </Button>
          <Button size="sm" onClick={() => setShowAdd(true)}>
            <Plus size={16} />
            {t('words.addWord')}
          </Button>
        </div>
      </div>

      <div className="flex flex-col gap-3 md:flex-row md:items-center">
        <div className="relative flex-1">
          <Search size={16} className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-muted" />
          <Input
            aria-label={t('words.searchWords')}
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder={t('words.searchWords')}
            className="pl-9"
          />
        </div>
        <Select
          aria-label={t('words.sortLabel')}
          value={sortKey}
          onChange={setSortKey}
          options={SORT_KEYS.map((k) => ({ value: k, label: t(`words.sort.${k}`) }))}
          className="md:w-48"
        />
      </div>

      <div className="flex gap-2 overflow-x-auto">
        {FILTERS.map((f) => (
          <button
            key={f}
            type="button"
            onClick={() => setFilter(f)}
            className={`whitespace-nowrap rounded-full border px-4 py-1.5 text-sm font-medium transition ${
              filter === f
                ? 'border-brand bg-brand/15 text-brand'
                : 'border-line text-muted hover:text-fg'
            }`}
          >
            {t(`words.filter.${f}`)}
          </button>
        ))}
      </div>

      {loading ? (
        <div className="grid gap-3 md:grid-cols-2">
          {[0, 1, 2, 3].map((i) => (
            <Skeleton key={i} className="h-28" />
          ))}
        </div>
      ) : groups.length === 0 ? (
        <div className="rounded-2xl border border-dashed border-line p-10 text-center">
          <BookOpen className="mx-auto mb-3 text-muted" />
          <p className="font-semibold">{t('words.noWordsAdded')}</p>
          <p className="text-sm text-muted">{t('words.startLearning')}</p>
        </div>
      ) : visibleGroups.length === 0 ? (
        <p className="py-8 text-center text-sm text-muted">{t('words.noMatch')}</p>
      ) : (
        visibleGroups.map((g) => (
          <section key={g.field} className="space-y-3">
            <div className="flex items-center justify-between">
              <h3 className="font-semibold text-fg">
                {fieldLabel(g.field)} <span className="text-sm font-normal text-muted">({g.total})</span>
              </h3>
              <button
                type="button"
                onClick={() => setFieldToDelete(g.field)}
                className="flex items-center gap-1 text-xs text-muted hover:text-red-500"
              >
                <Trash2 size={14} />
                {t('words.deleteAll')}
              </button>
            </div>
            <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-3">
              {g.words.map((w) => (
                <WordCard key={w.id} word={w} onDelete={setWordToDelete} />
              ))}
            </div>
            {g.total > groups.find((x) => x.field === g.field)!.words.length && (
              <div className="flex justify-center">
                <Button variant="secondary" size="sm" onClick={() => loadMore(g.field)} disabled={loadingMore === g.field}>
                  {loadingMore === g.field && <Loader2 size={16} className="animate-spin" />}
                  {t('words.loadMore', {
                    shown: groups.find((x) => x.field === g.field)!.words.length,
                    total: g.total,
                  })}
                </Button>
              </div>
            )}
          </section>
        ))
      )}

      <WordAddModal open={showAdd} onClose={() => setShowAdd(false)} onSuccess={handleAdded} onError={showError} />
      <FieldImportModal
        open={showImport}
        onClose={() => setShowImport(false)}
        onSuccess={handleImported}
        onError={showError}
      />
      <ConfirmDeleteModal
        open={!!wordToDelete}
        title={t('words.deleteWordTitle')}
        message={t('words.deleteWordConfirm', { word: wordToDelete?.original ?? '' })}
        confirmLabel={t('words.delete')}
        busy={busy}
        onConfirm={confirmDeleteWord}
        onClose={() => setWordToDelete(null)}
      />
      <ConfirmDeleteModal
        open={!!fieldToDelete}
        title={t('words.deleteFieldTitle', { field: fieldToDelete ? fieldLabel(fieldToDelete) : '' })}
        message={t('words.deleteFieldConfirm', {
          field: fieldToDelete ? fieldLabel(fieldToDelete) : '',
          count: groups.find((g) => g.field === fieldToDelete)?.total ?? 0,
        })}
        confirmLabel={t('words.deleteAll')}
        busy={busy}
        onConfirm={confirmDeleteField}
        onClose={() => setFieldToDelete(null)}
      />
    </div>
  );
}
