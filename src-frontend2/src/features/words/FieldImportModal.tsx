import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { predefinedWordsApi } from '../../api/endpoints';
import Badge from '../../components/ui/Badge';
import Button from '../../components/ui/Button';
import Modal from '../../components/ui/Modal';
import Skeleton from '../../components/ui/Skeleton';

type PreviewWord = Awaited<ReturnType<typeof predefinedWordsApi.getWordsByField>>['data']['words'][number];

interface FieldImportModalProps {
  open: boolean;
  onClose: () => void;
  onSuccess: (fieldName: string, importedCount: number) => void;
  onError: (message: string) => void;
}

const PREVIEW_LIMIT = 8;

function FieldImportBody({ onClose, onSuccess, onError }: Omit<FieldImportModalProps, 'open'>) {
  const { t } = useTranslation();
  const [fields, setFields] = useState<string[] | null>(null);
  const [selected, setSelected] = useState<string | null>(null);
  const [words, setWords] = useState<PreviewWord[]>([]);
  const [previewLoading, setPreviewLoading] = useState(false);
  const [importing, setImporting] = useState(false);

  useEffect(() => {
    predefinedWordsApi
      .getFields()
      .then(({ data }) => setFields(data.fields))
      .catch(() => {
        setFields([]);
        onError(t('words.fieldsLoadFailed'));
      });
  }, [onError, t]);

  const select = async (field: string) => {
    setSelected(field);
    setPreviewLoading(true);
    try {
      const { data } = await predefinedWordsApi.getWordsByField(field);
      setWords(data.words);
    } catch {
      setWords([]);
      onError(t('words.fieldWordsLoadFailed'));
    } finally {
      setPreviewLoading(false);
    }
  };

  const doImport = async () => {
    if (!selected) return;
    setImporting(true);
    try {
      const { data } = await predefinedWordsApi.importField(selected);
      onSuccess(data.fieldName, data.importedCount);
      onClose();
    } catch {
      onError(t('words.importFailed'));
      setImporting(false);
    }
  };

  return (
    <div className="space-y-4">
      <div>
        <h3 className="mb-2 text-sm font-semibold text-fg">{t('words.selectField')}</h3>
        {fields === null ? (
          <Skeleton className="h-16" />
        ) : (
          <div className="grid grid-cols-2 gap-2">
            {fields.map((f) => (
              <button
                key={f}
                type="button"
                onClick={() => select(f)}
                className={`rounded-xl border-2 p-3 text-left transition ${
                  selected === f ? 'border-brand bg-brand/10' : 'border-line hover:border-brand/50'
                }`}
              >
                <div className="font-medium text-fg">{f}</div>
                <div className="text-xs text-muted">{t('words.wordSet')}</div>
              </button>
            ))}
          </div>
        )}
      </div>

      {selected && (
        <div>
          <h3 className="mb-2 text-sm font-semibold text-fg">{t('words.preview')}</h3>
          {previewLoading ? (
            <Skeleton className="h-32" />
          ) : words.length === 0 ? (
            <p className="text-sm text-muted">{t('words.noFieldWords')}</p>
          ) : (
            <div className="max-h-56 space-y-2 overflow-y-auto">
              {words.slice(0, PREVIEW_LIMIT).map((w) => (
                <div key={w.id} className="flex items-start justify-between gap-2 rounded-xl bg-elevated p-3">
                  <div>
                    <div className="text-sm font-medium text-fg">{w.original}</div>
                    <div className="text-xs text-muted">{w.translation}</div>
                  </div>
                  {w.category && <Badge tone="amber">{w.category}</Badge>}
                </div>
              ))}
              {words.length > PREVIEW_LIMIT && (
                <p className="py-1 text-center text-xs text-muted">
                  {t('words.moreWords', { count: words.length - PREVIEW_LIMIT })}
                </p>
              )}
            </div>
          )}
        </div>
      )}

      <div className="flex gap-2">
        <Button variant="secondary" fullWidth onClick={onClose}>
          {t('words.cancel')}
        </Button>
        <Button fullWidth onClick={doImport} disabled={!selected || importing}>
          {importing ? t('words.importing') : t('words.importWords')}
        </Button>
      </div>
    </div>
  );
}

export default function FieldImportModal({ open, ...rest }: FieldImportModalProps) {
  const { t } = useTranslation();
  return (
    <Modal open={open} title={t('words.importTitle')} onClose={rest.onClose}>
      <FieldImportBody {...rest} />
    </Modal>
  );
}
