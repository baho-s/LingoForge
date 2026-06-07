import { useEffect, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Loader2, Plus, Sparkles, X } from 'lucide-react';
import { wordsApi } from '../api/endpoints';
import type { BulkCreateWordsResult } from '../types';

type AddMode = 'single' | 'bulk';

interface ParsedBulkWord {
  original: string;
  translation: string;
  lineNumber: number;
}

interface InvalidBulkLine {
  lineNumber: number;
  value: string;
}

interface WordAddModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: (result: WordAddSuccessResult) => Promise<void> | void;
  onError: (message: string) => void;
}

export interface WordAddSuccessResult {
  mode: AddMode;
  createdCount: number;
  generatedSentenceCount: number;
  skippedLineCount?: number;
}

const BULK_LINE_PATTERN = /^(.+?)\s*(?:\t+|\s+\|\s+|\s*;\s*|\s*,\s*|\s+[-–—]\s+|\s*:\s*)(.+)$/;

function parseBulkText(value: string): { items: ParsedBulkWord[]; invalidLines: InvalidBulkLine[] } {
  const items: ParsedBulkWord[] = [];
  const invalidLines: InvalidBulkLine[] = [];

  value.split(/\r?\n/).forEach((line, index) => {
    const trimmed = line.trim();
    if (!trimmed) {
      return;
    }

    const match = trimmed.match(BULK_LINE_PATTERN);
    if (!match) {
      invalidLines.push({ lineNumber: index + 1, value: trimmed });
      return;
    }

    const original = match[1].trim();
    const translation = match[2].trim();

    if (!original || !translation) {
      invalidLines.push({ lineNumber: index + 1, value: trimmed });
      return;
    }

    items.push({ original, translation, lineNumber: index + 1 });
  });

  return { items, invalidLines };
}

export default function WordAddModal({ isOpen, onClose, onSuccess, onError }: WordAddModalProps) {
  const { t } = useTranslation();
  const [mode, setMode] = useState<AddMode>('single');
  const [addOriginal, setAddOriginal] = useState('');
  const [addTranslation, setAddTranslation] = useState('');
  const [addAiSentence, setAddAiSentence] = useState(true);
  const [adding, setAdding] = useState(false);
  const [bulkText, setBulkText] = useState('');
  const [bulkAiSentence, setBulkAiSentence] = useState(false);
  const [bulkAdding, setBulkAdding] = useState(false);

  useEffect(() => {
    if (!isOpen) {
      return;
    }

    setMode('single');
    setAddOriginal('');
    setAddTranslation('');
    setAddAiSentence(true);
    setBulkText('');
    setBulkAiSentence(false);
    setAdding(false);
    setBulkAdding(false);
  }, [isOpen]);

  const parsedBulk = useMemo(() => parseBulkText(bulkText), [bulkText]);

  const handleSingleAdd = async () => {
    if (!addOriginal.trim() || !addTranslation.trim()) return;

    setAdding(true);
    try {
      await wordsApi.add({
        original: addOriginal.trim(),
        translation: addTranslation.trim(),
        generateSentenceImmediately: addAiSentence,
      });

      await onSuccess({
        mode: 'single',
        createdCount: 1,
        generatedSentenceCount: addAiSentence ? 1 : 0,
      });
      onClose();
    } catch {
      onError(t('common.error'));
    } finally {
      setAdding(false);
    }
  };

  const handleBulkAdd = async () => {
    if (parsedBulk.items.length === 0) {
      onError(t('words.bulkNoValidRows'));
      return;
    }

    setBulkAdding(true);
    try {
      const payload = {
        items: parsedBulk.items.map((item) => ({
          original: item.original,
          translation: item.translation,
        })),
        generateSentenceImmediately: bulkAiSentence,
      };

      const { data }: { data: BulkCreateWordsResult } = await wordsApi.bulkAdd(payload);

      await onSuccess({
        mode: 'bulk',
        createdCount: data.createdCount,
        generatedSentenceCount: data.generatedSentenceCount,
        skippedLineCount: parsedBulk.invalidLines.length,
      });
      onClose();
    } catch {
      onError(t('common.error'));
    } finally {
      setBulkAdding(false);
    }
  };

  if (!isOpen) return null;

  const bulkReadyCount = parsedBulk.items.length;
  const bulkInvalidCount = parsedBulk.invalidLines.length;
  const canSubmitBulk = bulkReadyCount > 0 && !bulkAdding;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center px-4">
      <div className="fixed inset-0 bg-black/30" onClick={onClose} />
      <div className="relative bg-white rounded-2xl shadow-xl w-full max-w-2xl max-h-[90vh] overflow-y-auto">
        <div className="p-6 border-b border-gray-100 flex items-start justify-between gap-4">
          <div>
            <h3 className="text-lg font-semibold text-gray-900">{t('words.addNewWord')}</h3>
            <p className="text-sm text-gray-500 mt-1">{t('words.addWordModalSubtitle')}</p>
          </div>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600 transition-colors">
            <X size={20} />
          </button>
        </div>

        <div className="px-6 pt-5">
          <div className="inline-flex rounded-xl bg-gray-100 p-1">
            <button
              type="button"
              onClick={() => setMode('single')}
              className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors ${mode === 'single' ? 'bg-white text-gray-900 shadow-sm' : 'text-gray-500 hover:text-gray-700'}`}
            >
              {t('words.singleAdd')}
            </button>
            <button
              type="button"
              onClick={() => setMode('bulk')}
              className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors ${mode === 'bulk' ? 'bg-white text-gray-900 shadow-sm' : 'text-gray-500 hover:text-gray-700'}`}
            >
              {t('words.bulkAdd')}
            </button>
          </div>
        </div>

        <div className="p-6 space-y-6">
          {mode === 'single' ? (
            <div className="space-y-5">
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
                <span className="text-sm text-gray-700">{t('words.addAiSentence')}</span>
              </label>

              <button
                onClick={handleSingleAdd}
                disabled={adding || !addOriginal.trim() || !addTranslation.trim()}
                className="w-full py-3 bg-blue-600 text-white rounded-xl font-medium text-sm hover:bg-blue-700 transition-colors disabled:opacity-50 flex items-center justify-center gap-2"
              >
                {adding ? <Loader2 size={16} className="animate-spin" /> : <Plus size={16} />}
                {adding ? t('words.adding') : t('words.addWord')}
              </button>
            </div>
          ) : (
            <div className="space-y-5">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1.5">{t('words.bulkPasteLabel')}</label>
                <textarea
                  value={bulkText}
                  onChange={(e) => setBulkText(e.target.value)}
                  rows={10}
                  className="w-full px-4 py-3 rounded-xl border border-gray-200 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent resize-none"
                  placeholder={t('words.bulkPastePlaceholder')}
                />
                <p className="text-xs text-gray-500 mt-2">{t('words.bulkPasteHint')}</p>
              </div>

              <label className="flex items-center gap-3 cursor-pointer">
                <input
                  type="checkbox"
                  checked={bulkAiSentence}
                  onChange={(e) => setBulkAiSentence(e.target.checked)}
                  className="w-4 h-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                />
                <span className="text-sm text-gray-700">{t('words.bulkAiSentence')}</span>
              </label>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="rounded-xl border border-gray-200 bg-gray-50 p-4">
                  <div className="flex items-center justify-between mb-2">
                    <h4 className="text-sm font-semibold text-gray-900">{t('words.bulkPreview')}</h4>
                    <span className="text-xs text-gray-500">{bulkReadyCount} {t('words.wordsCount')}</span>
                  </div>
                  <div className="space-y-2 max-h-56 overflow-y-auto pr-1">
                    {parsedBulk.items.length === 0 ? (
                      <p className="text-sm text-gray-500">{t('words.bulkPreviewEmpty')}</p>
                    ) : (
                      parsedBulk.items.slice(0, 8).map((item) => (
                        <div key={`${item.lineNumber}-${item.original}`} className="rounded-lg bg-white border border-gray-200 px-3 py-2">
                          <div className="text-sm font-medium text-gray-900">{item.original}</div>
                          <div className="text-xs text-gray-500">{item.translation}</div>
                        </div>
                      ))
                    )}
                    {parsedBulk.items.length > 8 && (
                      <div className="text-xs text-gray-500 text-center pt-1">+{parsedBulk.items.length - 8} {t('words.moreRows')}</div>
                    )}
                  </div>
                </div>

                <div className="rounded-xl border border-gray-200 bg-white p-4">
                  <h4 className="text-sm font-semibold text-gray-900 mb-2">{t('words.bulkSummary')}</h4>
                  <div className="space-y-2 text-sm text-gray-600">
                    <div>{t('words.bulkReadyRows')}: <span className="font-semibold text-gray-900">{bulkReadyCount}</span></div>
                    <div>{t('words.bulkInvalidRows')}: <span className="font-semibold text-gray-900">{bulkInvalidCount}</span></div>
                    <p className="text-xs text-gray-500 leading-5">
                      {t('words.bulkSummaryHint')}
                    </p>
                  </div>
                  {bulkInvalidCount > 0 && (
                    <div className="mt-4 rounded-lg bg-amber-50 border border-amber-200 p-3">
                      <p className="text-xs font-medium text-amber-800 mb-1">{t('words.bulkInvalidRows')}</p>
                      <div className="space-y-1 max-h-32 overflow-y-auto pr-1">
                        {parsedBulk.invalidLines.slice(0, 5).map((line) => (
                          <div key={`${line.lineNumber}-${line.value}`} className="text-xs text-amber-700">
                            {line.lineNumber}. {line.value}
                          </div>
                        ))}
                        {parsedBulk.invalidLines.length > 5 && (
                          <div className="text-xs text-amber-700">+{parsedBulk.invalidLines.length - 5} {t('words.moreInvalidRows')}</div>
                        )}
                      </div>
                    </div>
                  )}
                </div>
              </div>

              <button
                onClick={handleBulkAdd}
                disabled={!canSubmitBulk}
                className="w-full py-3 bg-gradient-to-r from-blue-600 to-indigo-600 text-white rounded-xl font-medium text-sm hover:from-blue-700 hover:to-indigo-700 transition-colors disabled:opacity-50 flex items-center justify-center gap-2"
              >
                {bulkAdding ? <Loader2 size={16} className="animate-spin" /> : <Sparkles size={16} />}
                {bulkAdding ? t('words.bulkAdding') : t('words.bulkAdd')}
              </button>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}