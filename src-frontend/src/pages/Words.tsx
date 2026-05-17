import { useEffect, useState, useMemo, Component, type ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import { Search, Plus, Sparkles, X, Loader2, ArrowUpDown, AlertTriangle, BookOpen, Trash2 } from 'lucide-react';
import { wordsApi } from '../api/endpoints';
import { useToast } from '../components/Toast';
import FieldImportModal from '../components/FieldImportModal';
import type { WordDto } from '../types';
import { SkeletonCard } from '../components/Skeleton';

const UUID_RE = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

type SortKey = 'createdAt' | 'original' | 'nextReviewAt';
type SortDir = 'asc' | 'desc';
type FilterKey = 'all' | 'due' | 'notDue';

// ─── Error Boundary ────────────────────────────────────────────────────────────
// Production React shows a blank white screen for any uncaught render error.
// This catches it and shows a human-readable message instead.
class ErrorBoundary extends Component<{ children: ReactNode }, { hasError: boolean; message: string }> {
  constructor(props: { children: ReactNode }) {
    super(props);
    this.state = { hasError: false, message: '' };
  }

  static getDerivedStateFromError(error: Error) {
    return { hasError: true, message: error?.message ?? 'Bilinmeyen hata' };
  }

  componentDidCatch(error: Error, info: { componentStack: string }) {
    console.error('[Words ErrorBoundary]', error, info);
  }

  render() {
    if (this.state.hasError) {
      return (
        <div className="max-w-5xl mx-auto py-16 text-center space-y-4">
          <div className="w-16 h-16 rounded-full bg-red-100 flex items-center justify-center mx-auto">
            <AlertTriangle size={28} className="text-red-600" />
          </div>
          <h2 className="text-xl font-semibold text-gray-900">Bir şeyler ters gitti</h2>
          <p className="text-sm text-gray-500">{this.state.message}</p>
          <button
            onClick={() => this.setState({ hasError: false, message: '' })}
            className="px-4 py-2 bg-blue-600 text-white rounded-xl text-sm font-medium hover:bg-blue-700 transition-colors"
          >
            Tekrar Dene
          </button>
        </div>
      );
    }
    return this.props.children;
  }
}

// ─── Main Component ────────────────────────────────────────────────────────────
function WordsInner() {
  const { t } = useTranslation();
  const [words, setWords] = useState<WordDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [search, setSearch] = useState('');

  // Sorting & filtering
  const [sortKey, setSortKey] = useState<SortKey>('createdAt');
  const [sortDir, setSortDir] = useState<SortDir>('desc');
  const [filter, setFilter] = useState<FilterKey>('all');

  // Add word modal
  const [showAdd, setShowAdd] = useState(false);
  const [addOriginal, setAddOriginal] = useState('');
  const [addTranslation, setAddTranslation] = useState('');
  const [addAiSentence, setAddAiSentence] = useState(true);
  const [adding, setAdding] = useState(false);

  // Delete confirmation
  const [deleteTarget, setDeleteTarget] = useState<WordDto | null>(null);

  // Bulk delete by field
  const [deleteFieldTarget, setDeleteFieldTarget] = useState<string | null>(null);
  const [deletingField, setDeletingField] = useState(false);

  // Bulk generate
  const [bulkLoading, setBulkLoading] = useState(false);

  // Field import modal
  const [showFieldImport, setShowFieldImport] = useState(false);

  // Pagination per field
  const ITEMS_PER_LOAD = 3;
  const [displayedCountByField, setDisplayedCountByField] = useState<Record<string, number>>({});
  const [loadingMoreByField, setLoadingMoreByField] = useState<Record<string, boolean>>({});

  const { addToast } = useToast();

  // ─── Single, clean fetch function (no duplicate logic) ──────────────────────
  const fetchWords = async () => {
    try {
      // Step 1: fast first paint with initial batch
      const { data: initial } = await wordsApi.getAll(0, 100);
      setWords(initial.words);
      setTotalCount(initial.totalCount);
      setDisplayedCountByField({});

      // Step 2: if there are more, fetch the rest (capped at 2 000)
      if (initial.totalCount > 100) {
        const take = Math.min(initial.totalCount, 2000);
        const { data: full } = await wordsApi.getAll(0, take);
        setWords(full.words);
        setTotalCount(full.totalCount);
        setDisplayedCountByField({});
      }
    } catch (err) {
      console.error('[fetchWords]', err);
      setError(t('common.error'));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchWords();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // ─── Derived data ────────────────────────────────────────────────────────────
  const filtered = useMemo(() => {
    const currentNow = new Date(); // fresh per computation
    let result = [...words];

    const q = search.toLowerCase();
    if (q) {
      result = result.filter(
        (w) =>
          w.original.toLowerCase().includes(q) ||
          w.translation.toLowerCase().includes(q),
      );
    }

    if (filter === 'due') {
      result = result.filter((w) => new Date(w.nextReviewAt) <= currentNow);
    } else if (filter === 'notDue') {
      result = result.filter((w) => new Date(w.nextReviewAt) > currentNow);
    }

    result.sort((a, b) => {
      let cmp = 0;
      if (sortKey === 'createdAt') cmp = new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
      else if (sortKey === 'original') cmp = a.original.localeCompare(b.original);
      else if (sortKey === 'nextReviewAt') cmp = new Date(a.nextReviewAt).getTime() - new Date(b.nextReviewAt).getTime();
      return sortDir === 'desc' ? -cmp : cmp;
    });

    return result;
  }, [words, search, sortKey, sortDir, filter]);

  const dueCount = useMemo(
    () => words.filter((w) => new Date(w.nextReviewAt) <= new Date()).length,
    [words],
  );

  const groupedByField = useMemo(() => {
    const groups: Record<string, WordDto[]> = { _no_field: [] };
    for (const word of filtered) {
      const field = word.field || '_no_field';
      if (!groups[field]) groups[field] = [];
      groups[field].push(word);
    }
    return groups;
  }, [filtered]);

  // ─── Handlers ────────────────────────────────────────────────────────────────
  const handleLoadMore = async (field: string) => {
    const currentDisplayed = displayedCountByField[field] ?? ITEMS_PER_LOAD;
    const nextDisplayed = currentDisplayed + ITEMS_PER_LOAD;

    setDisplayedCountByField((prev) => ({ ...prev, [field]: nextDisplayed }));

    // Only hit the API if we haven't loaded everything yet
    if (words.length < totalCount && words.length < 2000) {
      setLoadingMoreByField((prev) => ({ ...prev, [field]: true }));
      try {
        const smartTake = Math.min(totalCount, 2000);
        const { data } = await wordsApi.getAll(0, smartTake);
        setWords(data.words);
      } catch {
        addToast(t('common.error'), 'error');
      } finally {
        setLoadingMoreByField((prev) => ({ ...prev, [field]: false }));
      }
    }
  };

  const handleAdd = async () => {
    if (!addOriginal.trim() || !addTranslation.trim()) return;
    setAdding(true);
    try {
      await wordsApi.add({
        original: addOriginal.trim(),
        translation: addTranslation.trim(),
        generateSentenceImmediately: addAiSentence,
      });
      setShowAdd(false);
      setAddOriginal('');
      setAddTranslation('');
      setAddAiSentence(true);
      await fetchWords();
      addToast(t('words.addWordSuccess'), 'success');
    } catch {
      addToast(t('common.error'), 'error');
    } finally {
      setAdding(false);
    }
  };

  const handleBulkGenerate = async () => {
    setBulkLoading(true);
    try {
      const { data } = await wordsApi.bulkGenerate();
      await fetchWords();
      addToast(`${data.generated} cümle oluşturuldu. ${data.skipped} atlandı.`, 'success');
    } catch {
      addToast(t('common.error'), 'error');
    } finally {
      setBulkLoading(false);
    }
  };

  const handleFieldImportSuccess = async (fieldName: string, importedCount: number) => {
    await fetchWords();
    addToast(`${importedCount} kelime "${fieldName}" alanından eklendi!`, 'success');
  };

  const handleDelete = async (id: string) => {
    if (!UUID_RE.test(id)) return;
    try {
      await wordsApi.delete(id);
      setWords((prev) => prev.filter((w) => w.id !== id));
      setTotalCount((prev) => Math.max(0, prev - 1));
      addToast(t('words.deleteWordSuccess'), 'success');
    } catch {
      addToast(t('common.error'), 'error');
    }
    setDeleteTarget(null);
  };

  const handleBulkDeleteByField = async (field: string) => {
    setDeletingField(true);
    try {
      const { data } = await wordsApi.bulkDeleteByField(field);
      setWords((prev) => prev.filter((w) => (w.field || '_no_field') !== field));
      setTotalCount((prev) => Math.max(0, prev - data.deletedCount));
      setDisplayedCountByField((prev) => {
        const next = { ...prev };
        delete next[field];
        return next;
      });
      addToast(`${data.deletedCount} kelime başarıyla silindi.`, 'success');
    } catch {
      addToast(t('common.error'), 'error');
    } finally {
      setDeletingField(false);
      setDeleteFieldTarget(null);
    }
  };

  const toggleSort = (key: SortKey) => {
    if (sortKey === key) setSortDir((d) => (d === 'asc' ? 'desc' : 'asc'));
    else { setSortKey(key); setSortDir('asc'); }
  };

  // ─── Render ──────────────────────────────────────────────────────────────────
  return (
    <div className="max-w-5xl mx-auto space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h2 className="text-2xl font-bold text-gray-900">{t('words.myWords')}</h2>
          <p className="text-sm text-gray-500 mt-1">
            {totalCount} kelime toplam, {dueCount} tekrar için hazır
          </p>
        </div>
        <div className="flex gap-3 flex-wrap">
          <button
            onClick={() => setShowFieldImport(true)}
            className="flex items-center gap-2 px-4 py-2.5 bg-purple-600 text-white rounded-xl text-sm font-medium hover:bg-purple-700 transition-colors"
          >
            <BookOpen size={16} />
            Alan Kelimeleri
          </button>
          <button
            onClick={handleBulkGenerate}
            disabled={bulkLoading}
            className="flex items-center gap-2 px-4 py-2.5 bg-green-600 text-white rounded-xl text-sm font-medium hover:bg-green-700 transition-colors disabled:opacity-50"
          >
            {bulkLoading ? <Loader2 size={16} className="animate-spin" /> : <Sparkles size={16} />}
            AI Cümlesi Oluştur
          </button>
          <button
            onClick={() => setShowAdd(true)}
            className="flex items-center gap-2 px-4 py-2.5 bg-blue-600 text-white rounded-xl text-sm font-medium hover:bg-blue-700 transition-colors"
          >
            <Plus size={16} />
            {t('words.addWord')}
          </button>
        </div>
      </div>

      {error && (
        <div className="bg-red-50 text-red-700 rounded-xl px-4 py-3 text-sm flex items-center justify-between">
          <span>{error}</span>
          <button onClick={() => setError('')}><X size={16} /></button>
        </div>
      )}

      {/* Search + Sort + Filter */}
      <div className="flex flex-col sm:flex-row gap-3">
        <div className="relative flex-1">
          <Search size={18} className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400" />
          <input
            type="text"
            placeholder={t('words.searchWords')}
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="w-full pl-11 pr-4 py-3 rounded-xl border border-gray-200 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white"
          />
        </div>
        <div className="flex gap-2">
          <select
            value={filter}
            onChange={(e) => setFilter(e.target.value as FilterKey)}
            className="px-3 py-3 rounded-xl border border-gray-200 text-sm bg-white focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value="all">Tüm Kelimeler</option>
            <option value="due">Tekrar için Hazır</option>
            <option value="notDue">Hazır Değil</option>
          </select>
          <button
            onClick={() => toggleSort(sortKey)}
            className="flex items-center gap-1.5 px-3 py-3 rounded-xl border border-gray-200 text-sm bg-white hover:bg-gray-50 transition-colors"
          >
            <ArrowUpDown size={14} />
            <select
              value={sortKey}
              onChange={(e) => setSortKey(e.target.value as SortKey)}
              onClick={(e) => e.stopPropagation()}
              className="bg-transparent focus:outline-none cursor-pointer"
            >
              <option value="createdAt">Eklenme Tarihi</option>
              <option value="original">Alfabetik</option>
              <option value="nextReviewAt">Sonraki Tekrar</option>
            </select>
            <span className="text-xs text-gray-400">{sortDir === 'asc' ? 'ASC' : 'DESC'}</span>
          </button>
        </div>
      </div>

      {/* Word grid */}
      {loading ? (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {Array.from({ length: 6 }).map((_, i) => <SkeletonCard key={i} />)}
        </div>
      ) : filtered.length === 0 ? (
        <div className="text-center py-16">
          <div className="w-16 h-16 rounded-full bg-gray-100 flex items-center justify-center mx-auto mb-4">
            <Search size={24} className="text-gray-300" />
          </div>
          <p className="text-gray-500 text-sm">
            {search ? 'Aramanla eşleşen kelime yok.' : t('words.noWordsAdded')}
          </p>
        </div>
      ) : (
        <div className="space-y-8">
          {Object.entries(groupedByField).map(([field, fieldWords]) => {
            // Skip empty groups
            if (fieldWords.length === 0) return null;

            const displayedCount = displayedCountByField[field] ?? ITEMS_PER_LOAD;
            const isLoadingMore = loadingMoreByField[field] ?? false;
            const hasMore = displayedCount < fieldWords.length;
            const fieldLabel = field === '_no_field' ? 'Sizin Kelimeleriniz' : field;

            return (
              <div key={field}>
                {/* Field Header */}
                <div className="flex items-center justify-between mb-4 pb-2 border-b border-gray-200">
                  <div>
                    <h3 className="text-lg font-semibold text-gray-900">{fieldLabel}</h3>
                    <p className="text-xs text-gray-500">{fieldWords.length} kelime</p>
                  </div>
                  <button
                    onClick={() => setDeleteFieldTarget(field)}
                    className="flex items-center gap-2 px-3 py-2 text-red-600 hover:bg-red-50 rounded-lg transition-colors text-sm font-medium"
                  >
                    <Trash2 size={16} />
                    Tümünü Sil
                  </button>
                </div>

                {/* Words Grid */}
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                  {fieldWords.slice(0, displayedCount).map((word) => {
                    const isDue = new Date(word.nextReviewAt) <= new Date();
                    return (
                      <div
                        key={word.id}
                        className="bg-white rounded-xl shadow-sm p-5 border border-gray-100 hover:shadow-md transition-shadow group"
                      >
                        <div className="flex items-start justify-between mb-2">
                          <h3 className="text-lg font-semibold text-gray-900">{word.original}</h3>
                          <div className="flex items-center gap-2">
                            <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${isDue ? 'bg-orange-100 text-orange-700' : 'bg-green-100 text-green-700'}`}>
                              {isDue ? 'Hazır' : 'Planlandı'}
                            </span>
                            <button
                              onClick={() => setDeleteTarget(word)}
                              className="opacity-0 group-hover:opacity-100 text-gray-300 hover:text-red-500 transition-all"
                            >
                              <X size={16} />
                            </button>
                          </div>
                        </div>
                        <p className="text-sm text-gray-500 mb-3">{word.translation}</p>
                        {word.aiSentence && (
                          <p className="text-xs text-gray-400 italic bg-gray-50 rounded-lg px-3 py-2">
                            &ldquo;{word.aiSentence}&rdquo;
                          </p>
                        )}
                        <div className="mt-3 text-xs text-gray-400">
                          Sonraki Tekrar: {new Date(word.nextReviewAt).toLocaleDateString('tr-TR')}
                        </div>
                      </div>
                    );
                  })}
                </div>

                {/* Load More — shows per-field count, not total */}
                {hasMore && (
                  <div className="mt-6 flex justify-center">
                    <button
                      onClick={() => handleLoadMore(field)}
                      disabled={isLoadingMore}
                      className="px-6 py-3 bg-gradient-to-r from-blue-500 to-blue-600 text-white rounded-xl font-medium text-sm hover:shadow-lg hover:from-blue-600 hover:to-blue-700 transition-all flex items-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                      {isLoadingMore ? (
                        <>
                          <Loader2 size={16} className="animate-spin" />
                          Yükleniyor...
                        </>
                      ) : (
                        <>
                          <BookOpen size={16} />
                          {/* FIX: show fieldWords.length, not totalCount */}
                          Devamını Görüntüle ({displayedCount} / {fieldWords.length})
                        </>
                      )}
                    </button>
                  </div>
                )}
              </div>
            );
          })}
        </div>
      )}

      {/* ── Modals ─────────────────────────────────────────────────────────── */}

      {/* Add Word */}
      {showAdd && (
        <div className="fixed inset-0 z-50 flex items-center justify-center px-4">
          <div className="fixed inset-0 bg-black/30" onClick={() => setShowAdd(false)} />
          <div className="relative bg-white rounded-2xl shadow-xl w-full max-w-md p-6 space-y-5">
            <div className="flex items-center justify-between">
              <h3 className="text-lg font-semibold text-gray-900">{t('words.addNewWord')}</h3>
              <button onClick={() => setShowAdd(false)} className="text-gray-400 hover:text-gray-600">
                <X size={20} />
              </button>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1.5">{t('words.english')}</label>
              <input
                type="text"
                value={addOriginal}
                onChange={(e) => setAddOriginal(e.target.value)}
                className="w-full px-4 py-3 rounded-xl border border-gray-200 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="örn. serendipity"
                autoFocus
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1.5">{t('words.translation')}</label>
              <input
                type="text"
                value={addTranslation}
                onChange={(e) => setAddTranslation(e.target.value)}
                className="w-full px-4 py-3 rounded-xl border border-gray-200 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="örn. tesadüfi iyi şans"
              />
            </div>
            <label className="flex items-center gap-3 cursor-pointer">
              <input
                type="checkbox"
                checked={addAiSentence}
                onChange={(e) => setAddAiSentence(e.target.checked)}
                className="w-4 h-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
              />
              <span className="text-sm text-gray-700">AI Cümlesi Oluştur</span>
            </label>
            <button
              onClick={handleAdd}
              disabled={adding || !addOriginal.trim() || !addTranslation.trim()}
              className="w-full py-3 bg-blue-600 text-white rounded-xl font-medium text-sm hover:bg-blue-700 transition-colors disabled:opacity-50"
            >
              {adding ? 'Ekleniyor...' : t('words.addWord')}
            </button>
          </div>
        </div>
      )}

      {/* Delete Word */}
      {deleteTarget && (
        <div className="fixed inset-0 z-50 flex items-center justify-center px-4">
          <div className="fixed inset-0 bg-black/30" onClick={() => setDeleteTarget(null)} />
          <div className="relative bg-white rounded-2xl shadow-xl w-full max-w-sm p-6 space-y-4">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-full bg-red-50 flex items-center justify-center flex-shrink-0">
                <AlertTriangle size={20} className="text-red-600" />
              </div>
              <div>
                <h3 className="text-lg font-semibold text-gray-900">Kelime Sil</h3>
                <p className="text-sm text-gray-500">
                  &ldquo;<span className="font-medium text-gray-700">{deleteTarget.original}</span>&rdquo; kelimesini silmek istediğine emin misin? Bu işlem geri alınamaz.
                </p>
              </div>
            </div>
            <div className="flex gap-3 justify-end">
              <button
                onClick={() => setDeleteTarget(null)}
                className="px-4 py-2.5 bg-gray-100 text-gray-700 rounded-xl text-sm font-medium hover:bg-gray-200 transition-colors"
              >
                {t('words.cancel')}
              </button>
              <button
                onClick={() => handleDelete(deleteTarget.id)}
                className="px-4 py-2.5 bg-red-600 text-white rounded-xl text-sm font-medium hover:bg-red-700 transition-colors"
              >
                Sil
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Bulk Delete by Field */}
      {deleteFieldTarget && (
        <div className="fixed inset-0 z-50 flex items-center justify-center px-4">
          <div className="fixed inset-0 bg-black/30" onClick={() => setDeleteFieldTarget(null)} />
          <div className="relative bg-white rounded-2xl shadow-xl w-full max-w-sm p-6 space-y-4">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-full bg-red-50 flex items-center justify-center flex-shrink-0">
                <AlertTriangle size={20} className="text-red-600" />
              </div>
              <div>
                <h3 className="text-lg font-semibold text-gray-900">
                  {deleteFieldTarget === '_no_field' ? 'Sizin Kelimeleriniz' : deleteFieldTarget} Alanındaki Kelimeleri Sil
                </h3>
                <p className="text-sm text-gray-500">
                  &ldquo;<span className="font-medium text-gray-700">
                    {deleteFieldTarget === '_no_field' ? 'Sizin Kelimeleriniz' : deleteFieldTarget}
                  </span>&rdquo; alanındaki{' '}
                  <span className="font-medium text-gray-700">
                    {filtered.filter((w) => (w.field || '_no_field') === deleteFieldTarget).length}
                  </span>{' '}
                  kelime silinecektir. Bu işlem geri alınamaz.
                </p>
              </div>
            </div>
            <div className="flex gap-3 justify-end">
              <button
                onClick={() => setDeleteFieldTarget(null)}
                disabled={deletingField}
                className="px-4 py-2.5 bg-gray-100 text-gray-700 rounded-xl text-sm font-medium hover:bg-gray-200 transition-colors disabled:opacity-50"
              >
                {t('words.cancel')}
              </button>
              <button
                onClick={() => handleBulkDeleteByField(deleteFieldTarget)}
                disabled={deletingField}
                className="px-4 py-2.5 bg-red-600 text-white rounded-xl text-sm font-medium hover:bg-red-700 transition-colors disabled:opacity-50 flex items-center gap-2"
              >
                {deletingField ? <Loader2 size={16} className="animate-spin" /> : <Trash2 size={16} />}
                {deletingField ? 'Siliniyor...' : 'Tümünü Sil'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Bulk AI loading overlay */}
      {bulkLoading && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/20">
          <div className="bg-white rounded-2xl shadow-xl p-8 flex flex-col items-center gap-4">
            <Loader2 size={32} className="animate-spin text-blue-600" />
            <p className="text-sm text-gray-600 font-medium">AI Cümleler oluşturuluyor...</p>
          </div>
        </div>
      )}

      {/* Field Import Modal */}
      <FieldImportModal
        isOpen={showFieldImport}
        onClose={() => setShowFieldImport(false)}
        onImportSuccess={handleFieldImportSuccess}
        onError={(msg) => addToast(msg, 'error')}
      />
    </div>
  );
}

// ─── Export wrapped in ErrorBoundary ──────────────────────────────────────────
export default function Words() {
  return (
    <ErrorBoundary>
      <WordsInner />
    </ErrorBoundary>
  );
}