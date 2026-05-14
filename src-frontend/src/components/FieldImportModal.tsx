import { useEffect, useState } from 'react';
import { X, BookOpen, Loader2 } from 'lucide-react';
import { predefinedWordsApi } from '../api/endpoints';

interface PredefinedWordDto {
  field: string;
  category: string;
  original: string;
  translation: string;
  aiSentence?: string;
}

interface FieldImportModalProps {
  isOpen: boolean;
  onClose: () => void;
  onImportSuccess: (fieldName: string, importedCount: number) => void;
  onError: (message: string) => void;
}

export default function FieldImportModal({
  isOpen,
  onClose,
  onImportSuccess,
  onError,
}: FieldImportModalProps) {
  const [fields, setFields] = useState<string[]>([]);
  const [selectedField, setSelectedField] = useState<string | null>(null);
  const [fieldWords, setFieldWords] = useState<PredefinedWordDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [previewLoading, setPreviewLoading] = useState(false);
  const [importing, setImporting] = useState(false);

  // Load available fields when modal opens
  useEffect(() => {
    if (isOpen && fields.length === 0) {
      loadFields();
    }
  }, [isOpen]);

  const loadFields = async () => {
    try {
      setLoading(true);
      const { data } = await predefinedWordsApi.getFields();
      setFields(data.fields);
    } catch (error) {
      console.error('Alanlar yüklenme hatası:', error);
      onError('Alanlar yüklenemedi');
    } finally {
      setLoading(false);
    }
  };

  const handleFieldSelect = async (field: string) => {
    setSelectedField(field);
    try {
      setPreviewLoading(true);
      const { data } = await predefinedWordsApi.getWordsByField(field);
      setFieldWords(data.words);
    } catch (error) {
      onError('Kelimeler yüklenemedi');
      setFieldWords([]);
    } finally {
      setPreviewLoading(false);
    }
  };

  const handleImport = async () => {
    if (!selectedField) return;

    try {
      setImporting(true);
      const { data } = await predefinedWordsApi.importField(selectedField);
      onImportSuccess(data.fieldName, data.importedCount);
      onClose();
    } catch (error) {
      onError('Kelimeler içe aktarılamadı');
    } finally {
      setImporting(false);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg shadow-xl max-w-2xl w-full mx-4 max-h-[90vh] overflow-y-auto">
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b border-gray-200">
          <div className="flex items-center gap-2">
            <BookOpen className="text-purple-600" size={24} />
            <h2 className="text-xl font-bold text-gray-900">Alan Kelimeleri İçe Aktar</h2>
          </div>
          <button
            onClick={onClose}
            className="text-gray-500 hover:text-gray-700 transition"
          >
            <X size={24} />
          </button>
        </div>

        {/* Content */}
        <div className="p-6 space-y-6">
          {/* Fields Selection */}
          <div>
            <h3 className="text-sm font-semibold text-gray-900 mb-3">Alan Seç</h3>
            {loading ? (
              <div className="flex items-center justify-center py-8">
                <Loader2 className="animate-spin text-purple-600" size={24} />
              </div>
            ) : (
              <div className="grid grid-cols-2 gap-3">
                {fields.map((field) => (
                  <button
                    key={field}
                    onClick={() => handleFieldSelect(field)}
                    className={`p-3 rounded-lg border-2 transition ${
                      selectedField === field
                        ? 'border-purple-600 bg-purple-50'
                        : 'border-gray-200 bg-white hover:border-gray-300'
                    }`}
                  >
                    <div className="font-medium text-gray-900">{field}</div>
                    <div className="text-xs text-gray-500">Kelime seti</div>
                  </button>
                ))}
              </div>
            )}
          </div>

          {/* Words Preview */}
          {selectedField && (
            <div>
              <h3 className="text-sm font-semibold text-gray-900 mb-3">Önizleme</h3>
              {previewLoading ? (
                <div className="flex items-center justify-center py-8">
                  <Loader2 className="animate-spin text-purple-600" size={24} />
                </div>
              ) : fieldWords.length > 0 ? (
                <div className="space-y-2 max-h-64 overflow-y-auto">
                  {fieldWords.slice(0, 12).map((word, idx) => (
                    <div
                      key={idx}
                      className="p-3 bg-gray-50 rounded-lg border border-gray-200"
                    >
                      <div className="flex justify-between items-start">
                        <div>
                          <div className="font-medium text-gray-900">{word.original}</div>
                          <div className="text-sm text-gray-600">{word.translation}</div>
                        </div>
                        <span className="text-xs bg-purple-100 text-purple-700 px-2 py-1 rounded">
                          {word.category}
                        </span>
                      </div>
                      {word.aiSentence && (
                        <div className="text-xs text-gray-500 mt-2 italic">
                          "{word.aiSentence}"
                        </div>
                      )}
                    </div>
                  ))}
                  {fieldWords.length > 12 && (
                    <div className="text-sm text-gray-600 text-center py-2">
                      +{fieldWords.length - 12} daha kelime var
                    </div>
                  )}
                </div>
              ) : (
                <div className="text-center text-gray-600 py-8">
                  Bu alan için kelime bulunamadı
                </div>
              )}
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="flex gap-3 p-6 border-t border-gray-200 bg-gray-50">
          <button
            onClick={onClose}
            className="flex-1 px-4 py-2 border border-gray-300 rounded-lg text-gray-700 font-medium hover:bg-gray-100 transition"
          >
            İptal
          </button>
          <button
            onClick={handleImport}
            disabled={!selectedField || importing}
            className="flex-1 px-4 py-2 bg-purple-600 text-white rounded-lg font-medium hover:bg-purple-700 disabled:bg-gray-400 disabled:cursor-not-allowed transition flex items-center justify-center gap-2"
          >
            {importing && <Loader2 size={18} className="animate-spin" />}
            Kelimeleri Ekle
          </button>
        </div>
      </div>
    </div>
  );
}
