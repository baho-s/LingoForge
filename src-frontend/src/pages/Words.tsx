import { useEffect, useState, useMemo } from 'react';
import { Search, Plus, Sparkles, X, Loader2, ArrowUpDown, AlertTriangle } from 'lucide-react';
import { wordsApi } from '../api/endpoints';
import { useToast } from '../components/Toast';
import type { WordDto, BulkGenerateResult } from '../types';
import { SkeletonCard } from '../components/Skeleton';

const UUID_RE = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

type SortKey = 'createdAt' | 'original' | 'nextReviewAt';
type SortDir = 'asc' | 'desc';
type FilterKey = 'all' | 'due' | 'notDue';

export default function Words() {
  const [words, setWords] = useState<WordDto[]>([]);
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

  // Bulk generate
  const [bulkLoading, setBulkLoading] = useState(false);

  const { addToast } = useToast();

  const fetchWords = async () => {
    try {
      const { data } = await wordsApi.getAll();
      setWords(data);
    } catch {
      setError('Failed to load words.');
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
      addToast('Word added successfully!', 'success');
    } catch {
      addToast('Failed to add word.', 'error');
    } finally {
      setAdding(false);
    }
  };

  const handleBulkGenerate = async () => {
    setBulkLoading(true);
    try {
      const { data } = await wordsApi.bulkGenerate();
      await fetchWords();
      addToast(`Generated ${data.generated} sentences. Skipped ${data.skipped}.`, 'success');
    } catch {
      addToast('Bulk generation failed.', 'error');
    } finally {
      setBulkLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!UUID_RE.test(id)) return;
    try {
      await wordsApi.delete(id);
      setWords((prev) => prev.filter((w) => w.id !== id));
      addToast('Word deleted.', 'success');
    } catch {
      addToast('Failed to delete word.', 'error');
    }
    setDeleteTarget(null);
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
          <h2 className="text-2xl font-bold text-gray-900">My Words</h2>
          <p className="text-sm text-gray-500 mt-1">{words.length} words total, {dueCount} due for review</p>
        </div>
        <div className="flex gap-3">
          <button
            onClick={handleBulkGenerate}
            disabled={bulkLoading}
            className="flex items-center gap-2 px-4 py-2.5 bg-green-600 text-white rounded-xl text-sm font-medium hover:bg-green-700 transition-colors disabled:opacity-50"
          >
            {bulkLoading ? <Loader2 size={16} className="animate-spin" /> : <Sparkles size={16} />}
            Generate AI Sentences
          </button>
          <button
            onClick={() => setShowAdd(true)}
            className="flex items-center gap-2 px-4 py-2.5 bg-blue-600 text-white rounded-xl text-sm font-medium hover:bg-blue-700 transition-colors"
          >
            <Plus size={16} />
            Add Word
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
            placeholder="Search words..."
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
            <option value="all">All words</option>
            <option value="due">Due for review</option>
            <option value="notDue">Not due</option>
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
              <option value="createdAt">Date added</option>
              <option value="original">Alphabetical</option>
              <option value="nextReviewAt">Next review</option>
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
            {search ? 'No words match your search.' : 'No words yet. Add your first word!'}
          </p>
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {filtered.map((word) => {
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
                      {isDue ? 'Due' : 'Scheduled'}
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
                  <span>Next review: {new Date(word.nextReviewAt).toLocaleDateString()}</span>
                </div>
              </div>
            );
          })}
        </div>
      )}

      {/* Add Word Modal */}
      {showAdd && (
        <div className="fixed inset-0 z-50 flex items-center justify-center px-4">
          <div className="fixed inset-0 bg-black/30" onClick={() => setShowAdd(false)} />
          <div className="relative bg-white rounded-2xl shadow-xl w-full max-w-md p-6 space-y-5">
            <div className="flex items-center justify-between">
              <h3 className="text-lg font-semibold text-gray-900">Add New Word</h3>
              <button onClick={() => setShowAdd(false)} className="text-gray-400 hover:text-gray-600">
                <X size={20} />
              </button>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1.5">English Word</label>
              <input
                type="text"
                value={addOriginal}
                onChange={(e) => setAddOriginal(e.target.value)}
                className="w-full px-4 py-3 rounded-xl border border-gray-200 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="e.g. serendipity"
                autoFocus
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1.5">Translation</label>
              <input
                type="text"
                value={addTranslation}
                onChange={(e) => setAddTranslation(e.target.value)}
                className="w-full px-4 py-3 rounded-xl border border-gray-200 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="e.g. serendipidad"
              />
            </div>
            <label className="flex items-center gap-3 cursor-pointer">
              <input
                type="checkbox"
                checked={addAiSentence}
                onChange={(e) => setAddAiSentence(e.target.checked)}
                className="w-4 h-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
              />
              <span className="text-sm text-gray-700">Generate AI Sentence</span>
            </label>
            <button
              onClick={handleAdd}
              disabled={adding || !addOriginal.trim() || !addTranslation.trim()}
              className="w-full py-3 bg-blue-600 text-white rounded-xl font-medium text-sm hover:bg-blue-700 transition-colors disabled:opacity-50"
            >
              {adding ? 'Adding...' : 'Add Word'}
            </button>
          </div>
        </div>
      )}

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
                <h3 className="text-lg font-semibold text-gray-900">Delete Word</h3>
                <p className="text-sm text-gray-500">
                  Are you sure you want to delete "<span className="font-medium text-gray-700">{deleteTarget.original}</span>"? This cannot be undone.
                </p>
              </div>
            </div>
            <div className="flex gap-3 justify-end">
              <button
                onClick={() => setDeleteTarget(null)}
                className="px-4 py-2.5 bg-gray-100 text-gray-700 rounded-xl text-sm font-medium hover:bg-gray-200 transition-colors"
              >
                Cancel
              </button>
              <button
                onClick={() => handleDelete(deleteTarget.id)}
                className="px-4 py-2.5 bg-red-600 text-white rounded-xl text-sm font-medium hover:bg-red-700 transition-colors"
              >
                Delete
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
            <p className="text-sm text-gray-600 font-medium">Generating AI sentences...</p>
          </div>
        </div>
      )}
    </div>
  );
}
