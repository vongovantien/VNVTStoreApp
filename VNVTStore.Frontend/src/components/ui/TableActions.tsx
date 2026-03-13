import type { ReactNode } from 'react';
import { Eye, Edit3, Trash2 } from 'lucide-react';
import { useTranslation } from 'react-i18next';

interface TableActionsProps {
  onView?: (() => void) | undefined;
  onEdit?: (() => void) | undefined;
  onDelete?: (() => void) | undefined;
  customActions?: ReactNode | undefined;
}

export const TableActions = ({ onView, onEdit, onDelete, customActions }: TableActionsProps) => {
  const { t } = useTranslation();

  return (
    <div className="flex items-center justify-center gap-2">
      {onView && (
        <button
          onClick={onView}
          className="p-1.5 text-slate-500 hover:text-blue-600 hover:bg-blue-50 rounded transition-colors"
          title={t('common.actions.view') || 'View'}
        >
          <Eye size={18} />
        </button>
      )}

      {onEdit && (
        <button
          onClick={onEdit}
          className="p-1.5 text-slate-500 hover:text-amber-600 hover:bg-amber-50 rounded transition-colors"
          title={t('common.actions.edit') || 'Edit'}
        >
          <Edit3 size={18} />
        </button>
      )}

      {onDelete && (
        <button
          onClick={onDelete}
          className="p-1.5 text-slate-500 hover:text-rose-600 hover:bg-rose-50 rounded transition-colors"
          title={t('common.actions.delete') || 'Delete'}
        >
          <Trash2 size={18} />
        </button>
      )}
      
      {customActions}
    </div>
  );
};
