import { useTranslation } from 'react-i18next';
import Button from '../../components/ui/Button';
import Modal from '../../components/ui/Modal';

interface ConfirmDeleteModalProps {
  open: boolean;
  title: string;
  message: string;
  busy?: boolean;
  confirmLabel: string;
  onConfirm: () => void;
  onClose: () => void;
}

export default function ConfirmDeleteModal({
  open,
  title,
  message,
  busy,
  confirmLabel,
  onConfirm,
  onClose,
}: ConfirmDeleteModalProps) {
  const { t } = useTranslation();
  return (
    <Modal open={open} title={title} onClose={onClose}>
      <p className="text-sm text-muted">{message}</p>
      <div className="mt-5 flex justify-end gap-2">
        <Button variant="secondary" onClick={onClose} disabled={busy}>
          {t('words.cancel')}
        </Button>
        <Button variant="danger" onClick={onConfirm} disabled={busy}>
          {busy ? t('words.deleting') : confirmLabel}
        </Button>
      </div>
    </Modal>
  );
}
