import { memo } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { ChevronRight } from 'lucide-react';

export interface SectionHeaderProps {
  title: string;
  icon?: React.ReactNode;
  viewAllLink?: string;
  extra?: React.ReactNode;
}

export const SectionHeader = memo(({ title, icon, viewAllLink, extra }: SectionHeaderProps) => {
  const { t } = useTranslation();

  return (
    <div className="flex items-center justify-between mb-6">
      <h2 className="flex items-center gap-2 text-xl md:text-2xl font-bold text-primary">
        {icon}
        {title}
      </h2>
      <div className="flex items-center gap-4">
        {extra}
        {viewAllLink && (
          <Link
            to={viewAllLink}
            className="flex items-center gap-1 text-primary font-semibold hover:gap-2 transition-all"
          >
            {t('common.viewAll')} <ChevronRight size={18} />
          </Link>
        )}
      </div>
    </div>
  );
});

SectionHeader.displayName = 'SectionHeader';
