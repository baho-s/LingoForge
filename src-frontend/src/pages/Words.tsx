import { useEffect, useState, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { Search, Plus, Sparkles, X, Loader2, ArrowUpDown, AlertTriangle, BookOpen, Trash2 } from 'lucide-react';
import { wordsApi } from '../api/endpoints';
import { useToast } from '../components/Toast';
import WordAddModal, { type WordAddSuccessResult } from '../components/WordAddModal';
import FieldImportModal from '../components/FieldImportModal';
import type { WordDto } from '../types';
import { SkeletonCard } from '../components/Skeleton';

const UUID_RE = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

type SortKey = 'createdAt' | 'original' | 'nextReviewAt';
type SortDir = 'asc' | 'desc';
type FilterKey = 'all' | 'due' | 'notDue';

export default function Words() {
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
  const [displayedCountByField, setDisplayedCountByField] = useState<Record<string, number>>({});
  const [loadingMoreByField, setLoadingMoreByField] = useState<Record<string, boolean>>({});
  const [totalCountByField, setTotalCountByField] = useState<Record<string, number>>({});

  const { addToast } = useToast();

  const fetchWords = async () => {
    try {
      const { data: fieldsData } = await wordsApi.getFields();
      const fieldKeys = fieldsData.fields.map((field) => field.field);
      const totalFromFields = fieldsData.fields.reduce((sum, field) => sum + field.count, 0);
      setTotalCount(totalFromFields);

      if (fieldKeys.length === 0) {
        setWords([]);
        setDisplayedCountByField({});
        setTotalCountByField({});
        return;
      }

      const fieldResults = await Promise.all(
        fieldKeys.map(async (fieldKey) => {
          const fieldParam = fieldKey === '_no_field' ? '_no_field' : fieldKey;
          const { data } = await wordsApi.getByField(fieldParam, 0, ITEMS_PER_LOAD);
          return { fieldKey, data };
        }),
      );

      const merged: WordDto[] = [];
      const seenIds = new Set<string>();
      const nextDisplayed: Record<string, number> = {};
      const nextTotals: Record<string, number> = {};

      fieldsData.fields.forEach((field) => {
        nextTotals[field.field] = field.count;
      });

      fieldResults.forEach(({ fieldKey, data }) => {
        nextDisplayed[fieldKey] = data.words.length;
        data.words.forEach((word) => {
          if (!seenIds.has(word.id)) {
            seenIds.add(word.id);
            merged.push(word);
          }
        });
      });

      setWords(merged);
      setDisplayedCountByField(nextDisplayed);
      setTotalCountByField(nextTotals);
    } catch {
      setError(t('common.error'));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchWords();
  }, []);

  const now = new Date();

  const filtered = useMemo(() => {
    let result = [...words];

    // Search
    const q = search.toLowerCase();
    if (q) {
      result = result.filter(
        (w) =>
          w.original.toLowerCase().includes(q) ||
          w.translation.toLowerCase().includes(q),
      );
    }

    // Filter
    if (filter === 'due') {
      result = result.filter((w) => new Date(w.nextReviewAt) <= now);
    } else if (filter === 'notDue') {
      result = result.filter((w) => new Date(w.nextReviewAt) > now);
    }

    // Sort
    result.sort((a, b) => {
      let cmp = 0;
      if (sortKey === 'createdAt') cmp = new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
      else if (sortKey === 'original') cmp = a.original.localeCompare(b.original);
      else if (sortKey === 'nextReviewAt') cmp = new Date(a.nextReviewAt).getTime() - new Date(b.nextReviewAt).getTime();
      return sortDir === 'desc' ? -cmp : cmp;
    });

    return result;
  }, [words, search, sortKey, sortDir, filter]);

  const dueCount = words.filter((w) => new Date(w.nextReviewAt) <= now).length;

  const ITEMS_PER_LOAD = 3;

  const groupedByField = useMemo(() => {
    const groups: Record<string, WordDto[]> = { '_no_field': [] };
    
    filtered.forEach((word) => {
      const field = word.field || '_no_field';
      if (!groups[field]) groups[field] = [];
      groups[field].push(word);
    });

    return groups;
  }, [filtered]);

  const handleLoadMore = async (field: string) => {
    const currentDisplayed = displayedCountByField[field] || ITEMS_PER_LOAD;
    const nextDisplayed = currentDisplayed + ITEMS_PER_LOAD;

    // Update displayed count first
    setDisplayedCountByField((prev) => ({
      ...prev,
      [field]: nextDisplayed,
    }));

    // Fetch more data for this specific field
    setLoadingMoreByField((prev) => ({
      ...prev,
      [field]: true,
    }));

    try {
      // Fetch next batch for this field
      const fieldName = field === '_no_field' ? '_no_field' : field;
      const { data } = await wordsApi.getByField(fieldName, currentDisplayed, ITEMS_PER_LOAD);
      
      if (data && data.words) {
        // Append words for this field
        setWords((prev) => [...prev, ...data.words]);
        setTotalCountByField((prev) => ({
          ...prev,
          [field]: data.totalCount,
        }));
      }
    } catch {
      addToast(t('common.error'), 'error');
      // Reset the displayed count on error
      setDisplayedCountByField((prev) => ({
        ...prev,
        [field]: currentDisplayed,
      }));
    } finally {
      setLoadingMoreByField((prev) => ({
        ...prev,
        [field]: false,
      }));
    }
  };

  const handleWordAddSuccess = async (result: WordAddSuccessResult) => {
    await fetchWords();

    if (result.mode === 'single') {
      addToast(t('words.addWordSuccess'), 'success');
      return;
    }

    const skippedPart = result.skippedLineCount ? `, ${result.skippedLineCount} satır atlandı` : '';
    const sentencePart = result.generatedSentenceCount ? `, ${result.generatedSentenceCount} AI cümlesi üretildi` : '';
    addToast(`${result.createdCount} kelime başarıyla eklendi${sentencePart}${skippedPart}.`, 'success');
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
      setWords((prev) => prev.filter((w) => {
        const wordField = w.field || '_no_field';
        return wordField !== field;
      }));
      setTotalCount((prev) => Math.max(0, prev - data.deletedCount));
      // Reset pagination for this field
      setDisplayedCountByField((prev) => {
        const newObj = { ...prev };
        delete newObj[field];
        return newObj;
      });
      setTotalCountByField((prev) => {
        const newObj = { ...prev };
        delete newObj[field];
        return newObj;
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

  return (
    <div className="max-w-5xl mx-auto space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h2 className="text-2xl font-bold text-gray-900">{t('words.myWords')}</h2>
          <p className="text-sm text-gray-500 mt-1">{totalCount} kelime toplam, {dueCount} tekrar için hazır</p>
        </div>
        <div className="flex gap-3">
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
              onChange={(e) => { setSortKey(e.target.value as SortKey); }}
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
          {Object.entries(groupedByField).map(([field, fieldWords]) => (
            <div key={field}>
              {/* Field Header with Bulk Delete */}
              {field !== '_no_field' && (
                <div className="flex items-center justify-between mb-4 pb-2 border-b border-gray-200">
                  <div>
                    <h3 className="text-lg font-semibold text-gray-900">{field}</h3>
                    <p className="text-xs text-gray-500">{totalCountByField[field] ?? fieldWords.length} kelime</p>
                  </div>
                  <button
                    onClick={() => setDeleteFieldTarget(field)}
                    className="flex items-center gap-2 px-3 py-2 text-red-600 hover:bg-red-50 rounded-lg transition-colors text-sm font-medium"
                  >
                    <Trash2 size={16} />
                    Tümünü Sil
                  </button>
                </div>
              )}
              {field === '_no_field' && fieldWords.length > 0 && (
                <div className="flex items-center justify-between mb-4 pb-2 border-b border-gray-200">
                  <div>
                    <h3 className="text-lg font-semibold text-gray-900">Benim Kelimelerim</h3>
                    <p className="text-xs text-gray-500">{totalCountByField[field] ?? fieldWords.length} kelime</p>
                  </div>
                  <button
                    onClick={() => setDeleteFieldTarget(field)}
                    className="flex items-center gap-2 px-3 py-2 text-red-600 hover:bg-red-50 rounded-lg transition-colors text-sm font-medium"
                  >
                    <Trash2 size={16} />
                    Tümünü Sil
                  </button>
                </div>
              )}

              {/* Words Grid */}
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                {fieldWords
                  .slice(0, displayedCountByField[field] || ITEMS_PER_LOAD)
                  .map((word: WordDto) => {
                    const isDue = new Date(word.nextReviewAt) <= now;
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
                            "{word.aiSentence}"
                          </p>
                        )}
                        <div className="mt-3 flex items-center gap-2 text-xs text-gray-400">
                          <span>Sonraki Tekrar: {new Date(word.nextReviewAt).toLocaleDateString('tr-TR')}</span>
                        </div>
                      </div>
                    );
                  })}
              </div>

              {/* Load More Button */}
              {(displayedCountByField[field] || ITEMS_PER_LOAD) < (totalCountByField[field] ?? fieldWords.length) && (
                <div className="mt-6 flex justify-center">
                  <button
                    onClick={() => handleLoadMore(field)}
                    disabled={loadingMoreByField[field] || false}
                    className="px-6 py-3 bg-gradient-to-r from-blue-500 to-blue-600 text-white rounded-xl font-medium text-sm hover:shadow-lg hover:from-blue-600 hover:to-blue-700 transition-all flex items-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                  {loadingMoreByField[field] ? (
                      <>
                        <Loader2 size={16} className="animate-spin" />
                        Yükleniyor...
                      </>
                    ) : (
                      <>
                        <BookOpen size={16} />
                          Devamını Görüntüle ({(displayedCountByField[field] || ITEMS_PER_LOAD)} / {(totalCountByField[field] ?? fieldWords.length)})
                      </>
                    )}
                  </button>
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      <WordAddModal
        isOpen={showAdd}
        onClose={() => setShowAdd(false)}
        onSuccess={handleWordAddSuccess}
        onError={(msg) => addToast(msg, 'error')}
      />

      {/* Delete Confirmation Modal */}
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
                  "<span className="font-medium text-gray-700">{deleteTarget.original}</span>" kelimesini silmek istediğine emin misin? Bu işlem geri alınamaz.
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

      {/* Bulk Delete by Field Modal */}
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
                  {deleteFieldTarget === '_no_field' ? 'Benim Kelimelerim' : deleteFieldTarget} Alanındaki Kelimeleri Sil
                </h3>
                <p className="text-sm text-gray-500">
                  "<span className="font-medium text-gray-700">{deleteFieldTarget === '_no_field' ? 'Benim Kelimelerim' : deleteFieldTarget}</span>" alanındaki <span className="font-medium text-gray-700">{totalCountByField[deleteFieldTarget] ?? filtered.filter(w => (w.field || '_no_field') === deleteFieldTarget).length}</span> kelime silinecektir. Bu işlem geri alınamaz.
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

      {/* Bulk loading overlay */}
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
