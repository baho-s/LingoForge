import { BarChart3, BookOpen, Brain, Home, type LucideIcon } from 'lucide-react';
import { useTranslation } from 'react-i18next';

export interface NavItem {
  to: string;
  label: string;
  icon: LucideIcon;
}

export function useNavItems(): NavItem[] {
  const { t } = useTranslation();
  return [
    { to: '/', label: t('common.home'), icon: Home },
    { to: '/words', label: t('words.myWords'), icon: BookOpen },
    { to: '/practice', label: t('practice.practice'), icon: Brain },
    { to: '/stats', label: t('stats.statistics'), icon: BarChart3 },
  ];
}
