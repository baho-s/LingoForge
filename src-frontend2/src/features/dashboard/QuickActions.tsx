import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Brain, ChevronRight, Plus, Wand2, type LucideIcon } from 'lucide-react';
import Card from '../../components/ui/Card';

interface ActionProps {
  icon: LucideIcon;
  title: string;
  description: string;
  tone: string;
  onClick: () => void;
}

function Action({ icon: Icon, title, description, tone, onClick }: ActionProps) {
  return (
    <button type="button" onClick={onClick} className="text-left">
      <Card className="flex h-full items-center gap-4 transition-colors hover:border-brand">
        <div className={`flex h-12 w-12 flex-shrink-0 items-center justify-center rounded-xl ${tone}`}>
          <Icon size={24} />
        </div>
        <div className="min-w-0 flex-1">
          <p className="text-sm font-semibold">{title}</p>
          <p className="text-xs text-muted">{description}</p>
        </div>
        <ChevronRight size={18} className="text-muted" />
      </Card>
    </button>
  );
}

export default function QuickActions() {
  const { t } = useTranslation();
  const navigate = useNavigate();

  return (
    <div className="grid grid-cols-1 gap-3 md:grid-cols-3 md:gap-4">
      <Action
        icon={Brain}
        title={t('dashboard.startReview')}
        description={t('dashboard.practiceYourDueWords')}
        tone="bg-blue-500/15 text-blue-500"
        onClick={() => navigate('/practice')}
      />
      <Action
        icon={Wand2}
        title={t('dashboard.aiCoachTitle')}
        description={t('dashboard.aiCoachDesc')}
        tone="bg-brand/15 text-brand"
        onClick={() => navigate('/practice')}
      />
      <Action
        icon={Plus}
        title={t('dashboard.addNewWord')}
        description={t('dashboard.expandYourVocabulary')}
        tone="bg-emerald-500/15 text-emerald-500"
        onClick={() => navigate('/words')}
      />
    </div>
  );
}
