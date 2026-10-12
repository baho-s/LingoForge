import { useMemo, useState } from 'react';
import { Plus } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { wordsApi } from '../../api/endpoints';
import Button from '../../components/ui/Button';
import Input from '../../components/ui/Input';
import Modal from '../../components/ui/Modal';
import { parseBulkText } from './parseBulk';

export interface WordAddResult {
  mode: 'single' | 'bulk';
  createdCount: number;
  generatedSentenceCount: number;
  skippedLineCount: number;
}

interface WordAddModalProps {
  open: boolean;
  onClose: () => void;
  onSuccess: (result: WordAddResult) => void;
  onError: (message: string) => void;
}

type Mode = 'single' | 'bulk';

function Checkbox({ checked, onChange, label }: { checked: boolean; onChange: (v: boolean) => void; label: string }) {
  return (
    <label className="flex cursor-pointer items-center gap-3 text-sm text-fg">
      <input
        type="checkbox"
        checked={checked}
        onChange={(e) => onChange(e.target.checked)}
        className="h-4 w-4 accent-amber-500"
      />
      {label}
    </label>
  );
}

// Parent mounts this only while open (key-based), so state resets on every open.
function WordAddForm({ onClose, onSuccess, onError }: Omit<WordAddModalProps, 'open'>) {
  const { t } = useTranslation();
  const [mode, setMode] = useState<Mode>('single');
  const [original, setOriginal] = useState('');
  const [translation, setTranslation] = useState('');
  const [singleAi, setSingleAi] = useState(true);
  const [bulkText, setBulkText] = useState('');
  const [bulkAi, setBulkAi] = useState(false);
  const [busy, setBusy] = useState(false);

  const parsed = useMemo(() => parseBulkText(bulkText), [bulkText]);

  const submit = async () => {
    setBusy(true);
    try {
      if (mode === 'single') {
        await wordsApi.add({
          original: original.trim(),
          translation: translation.trim(),
          generateSentenceImmediately: singleAi,
        });
        onSuccess({ mode, createdCount: 1, generatedSentenceCount: singleAi ? 1 : 0, skippedLineCount: 0 });
      } else {
        const { data } = await wordsApi.bulkAdd({
          items: parsed.items.map(({ original: o, translation: tr }) => ({ original: o, translation: tr })),
          generateSentenceImmediately: bulkAi,
        });
        onSuccess({
          mode,
          createdCount: data.createdCount,
          generatedSentenceCount: data.generatedSentenceCount,
          skippedLineCount: parsed.invalidCount,
        });
      }
      onClose();
    } catch {
      onError(t('common.error'));
      setBusy(false);
    }
  };

  const canSubmit = mode === 'single' ? !!original.trim() && !!translation.trim() : parsed.items.length > 0;

  return (
    <div className="space-y-4">
      <p className="text-sm text-muted">{t('words.addWordModalSubtitle')}</p>

      <div className="inline-flex rounded-xl bg-elevated p-1">
        {(['single', 'bulk'] as const).map((m) => (
          <button
            key={m}
            type="button"
            onClick={() => setMode(m)}
            className={`rounded-lg px-4 py-1.5 text-sm font-medium transition ${
              mode === m ? 'bg-surface text-fg shadow-sm' : 'text-muted hover:text-fg'
            }`}
          >
            {t(m === 'single' ? 'words.singleAdd' : 'words.bulkAdd')}
          </button>
        ))}
      </div>

      {mode === 'single' ? (
        <>
          <Input
            label={t('words.english')}
            value={original}
            onChange={(e) => setOriginal(e.target.value)}
            placeholder={t('words.englishPlaceholder')}
            autoFocus
          />
          <Input
            label={t('words.turkish')}
            value={translation}
            onChange={(e) => setTranslation(e.target.value)}
            placeholder={t('words.translationPlaceholder')}
          />
          <Checkbox checked={singleAi} onChange={setSingleAi} label={t('words.addAiSentence')} />
        </>
      ) : (
        <>
          <div>
            <label htmlFor="bulk-text" className="mb-1.5 block text-sm font-medium text-muted">
              {t('words.bulkPasteLabel')}
            </label>
            <textarea
              id="bulk-text"
              value={bulkText}
              onChange={(e) => setBulkText(e.target.value)}
              rows={7}
              placeholder={t('words.bulkPastePlaceholder')}
              className="w-full resize-none rounded-xl border border-line bg-elevated p-3 text-sm text-fg placeholder:text-muted focus:outline-none focus:ring-2 focus:ring-brand/50"
            />
            <p className="mt-1.5 text-xs text-muted">{t('words.bulkPasteHint')}</p>
          </div>
          <Checkbox checked={bulkAi} onChange={setBulkAi} label={t('words.bulkAiSentence')} />
          <div className="rounded-xl border border-line bg-elevated p-3 text-sm text-muted">
            {parsed.items.length === 0 ? (
              t('words.bulkPreviewEmpty')
            ) : (
              <ul className="max-h-32 space-y-1 overflow-y-auto">
                {parsed.items.slice(0, 8).map((i) => (
                  <li key={i.lineNumber}>
                    <span className="font-medium text-fg">{i.original}</span> — {i.translation}
                  </li>
                ))}
                {parsed.items.length > 8 && (
                  <li>
                    +{parsed.items.length - 8} {t('words.moreRows')}
                  </li>
                )}
              </ul>
            )}
            <p className="mt-2 text-xs">
              {t('words.bulkReadyRows')}: <b className="text-fg">{parsed.items.length}</b> ·{' '}
              {t('words.bulkInvalidRows')}: <b className="text-fg">{parsed.invalidCount}</b>
            </p>
          </div>
        </>
      )}

      <Button fullWidth onClick={submit} disabled={busy || !canSubmit}>
        <Plus size={16} />
        {busy ? t('words.adding') : t('words.addWord')}
      </Button>
    </div>
  );
}

export default function WordAddModal({ open, ...rest }: WordAddModalProps) {
  const { t } = useTranslation();
  return (
    <Modal open={open} title={t('words.addNewWord')} onClose={rest.onClose}>
      <WordAddForm {...rest} />
    </Modal>
  );
}
