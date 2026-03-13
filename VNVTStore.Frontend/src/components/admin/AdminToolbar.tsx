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
  onClick?: (() => void) | undefined;
  title?: string | undefined;
  disabled?: boolean | undefined;
  className?: string | undefined;
  label?: string | undefined;
}

// Specific styled versions based on screenshot (Blue icons)
const BlueItem = React.forwardRef<HTMLButtonElement, AdminToolbarAction>(({ icon, onClick, title, disabled, className: itemClassName, label }, ref) => (
  <button
    type="button"
    ref={ref}
    onClick={onClick ? onClick : undefined}
    disabled={disabled || !onClick}
    title={title}
    className={cn(
      "p-2 hover:bg-tertiary rounded transition-colors disabled:opacity-30 disabled:cursor-not-allowed flex items-center gap-2",
      itemClassName
    )}
  >
    <div className="w-5 h-5 flex items-center justify-center text-[#2d7ad6]">
      {icon}
    </div>
    {label && <span className="text-[#2d7ad6] font-medium text-sm whitespace-nowrap">{label}</span>}
  </button>
));
BlueItem.displayName = "BlueItem";

interface AdminToolbarProps {
  onAdd?: (() => void) | undefined;
  onViewDetails?: (() => void) | undefined;
  onEdit?: (() => void) | undefined;
  onDelete?: (() => void) | undefined;
  onSearchClick?: (() => void) | undefined;
  onRefresh?: (() => void) | undefined;
  onImport?: (() => void) | undefined;
  onExport?: (() => void) | undefined;
  onReset?: (() => void) | undefined;
  isSearchActive?: boolean | undefined;
  isExporting?: boolean | undefined;
  selectedCount?: number | undefined;
  className?: string | undefined;
  searchRef?: React.Ref<HTMLButtonElement> | undefined;
  children?: React.ReactNode | undefined;
  searchInput?: React.ReactNode | undefined;
}

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
