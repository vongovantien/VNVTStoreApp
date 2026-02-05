import {
  Plus,
  MoreHorizontal,
  Edit,
  Search,
  Download,
  Upload,
  Trash2,
  Loader2,
  RefreshCw
} from 'lucide-react';
import React from 'react';
import { useTranslation } from 'react-i18next';
import { cn } from '@/utils/cn';

interface AdminToolbarAction {
  icon: React.ReactNode;
  onClick?: () => void;
  title?: string;
  disabled?: boolean;
  className?: string; // Additional classes for specific buttons (e.g., active state)
}

interface AdminToolbarProps {
  onAdd?: () => void;
  onViewDetails?: () => void; // Converted from onMore
  onEdit?: () => void;
  onDelete?: () => void;
  // Removed onCheck

  onSearchClick?: () => void;
  onRefresh?: () => void;


  onImport?: () => void;
  onExport?: () => void;
  // Removed onPrint

  // Removed onHelp

  // States
  isSearchActive?: boolean;
  isExporting?: boolean;
  selectedCount?: number;
  className?: string;
  searchRef?: React.Ref<HTMLButtonElement>;
  children?: React.ReactNode;
}

// Specific styled versions based on screenshot (Blue icons)
const BlueItem = React.forwardRef<HTMLButtonElement, AdminToolbarAction>(({ icon, onClick, title, disabled, className: itemClassName }, ref) => (
  <button
    type="button"
    ref={ref}
    onClick={onClick ? onClick : undefined}
    disabled={disabled || !onClick}
    title={title}
    className={cn(
      "p-2 hover:bg-blue-50 dark:hover:bg-slate-700 rounded transition-colors disabled:opacity-30 disabled:cursor-not-allowed",
      itemClassName
    )}
  >
    <div className="w-5 h-5 flex items-center justify-center text-[#2d7ad6]">
      {icon}
    </div>
  </button>
));
BlueItem.displayName = "BlueItem";

export const AdminToolbar = ({
  onAdd,
  onViewDetails,
  onEdit,
  onDelete,
  // onCheck removed
  onSearchClick,
  onRefresh,
  onImport,
  onExport,
  // onPrint removed
  // onHelp removed
  isSearchActive,
  isExporting = false,
  selectedCount = 0,
  className,
  searchRef,
  searchInput, // New prop
  children,
}: AdminToolbarProps & { searchInput?: React.ReactNode }) => {
  const { t } = useTranslation();


  return (
    <div className={cn("flex flex-wrap items-center bg-transparent gap-1 relative z-20", className)}>

      {/* Group 1: CRUD */}
      <div className="flex items-center gap-1">
        <BlueItem icon={<Plus size={20} className="stroke-[2.5]" />} onClick={onAdd} title={t('admin.createNew')} />
        <BlueItem icon={<MoreHorizontal size={20} className="stroke-[2.5]" />} onClick={onViewDetails} disabled={selectedCount !== 1} title={t('admin.viewDetails')} />
        <BlueItem icon={<Edit size={18} className="stroke-[2.5]" />} onClick={onEdit} disabled={selectedCount !== 1} title={t('common.actions.edit')} />
        <BlueItem icon={<Trash2 size={20} className="stroke-[2.5]" />} onClick={onDelete} disabled={selectedCount === 0} title={t('common.actions.delete')} />
      </div>

      <div className="h-6 w-px bg-gray-200 dark:bg-slate-600 mx-2" />

      {/* Group 2: Search & Refresh */}
      <div className="flex items-center gap-1">
        {searchInput}
        <BlueItem
          ref={searchRef}
          icon={<Search size={20} className="stroke-[2.5]" />}
          onClick={onSearchClick}
          title={t('common.search')}
          className={isSearchActive ? "bg-blue-100 dark:bg-blue-900/30" : ""}
        />
        <BlueItem icon={<RefreshCw size={18} className="stroke-[2.5]" />} onClick={onRefresh} title={t('admin.refreshData')} />
      </div>

      <div className="h-6 w-px bg-gray-200 dark:bg-slate-600 mx-2" />

      {/* Group 3: File Ops */}
      <div className="flex items-center gap-1">
        <BlueItem icon={<Upload size={20} className="stroke-[2.5]" />} onClick={onImport} title={t('admin.import')} />
        <BlueItem 
          icon={isExporting ? <Loader2 size={20} className="stroke-[2.5] animate-spin" /> : <Download size={20} className="stroke-[2.5]" />} 
          onClick={!isExporting ? onExport : undefined} 
          disabled={isExporting}
          title={isExporting ? t('admin.exporting') : t('admin.export')} 
        />
      </div>

      {children && (
        <>
          <div className="flex-1" /> {/* Spacer to push children to right */}
          <div className="h-6 w-px bg-gray-200 dark:bg-slate-600 mx-2" />
          {children}
        </>
      )}

    </div>
  );
};
