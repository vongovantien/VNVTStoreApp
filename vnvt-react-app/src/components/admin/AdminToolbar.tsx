import {
  Plus,
  MoreHorizontal,
  Edit,
  Minus,
  Check,
  Search,
  RotateCcw, // Reset
  Download,
  Upload,
  Printer,
  HelpCircle
} from 'lucide-react';
import React from 'react';
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
  onMore?: () => void;
  onEdit?: () => void;
  onDelete?: () => void; // Minus icon
  onCheck?: () => void; // Check icon

  onSearchClick?: () => void;
  onReset?: () => void;

  onImport?: () => void;
  onExport?: () => void;
  onPrint?: () => void;

  onHelp?: () => void;

  // States
  isSearchActive?: boolean;
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
  onMore,
  onEdit,
  onDelete,
  onCheck,
  onSearchClick,
  onReset,
  onImport,
  onExport,
  onPrint,
  onHelp,
  isSearchActive,
  selectedCount = 0,
  className,
  searchRef,
  children,
}: AdminToolbarProps) => {

  const Item = ({ icon, onClick, title, disabled, className: itemClassName }: AdminToolbarAction) => (
    <button
      type="button"
      onClick={onClick}
      disabled={disabled}
      title={title}
      className={cn(
        "p-2 text-primary hover:bg-gray-100 dark:hover:bg-slate-700 rounded transition-colors disabled:opacity-30 disabled:cursor-not-allowed",
        itemClassName
      )}
    >
      <div className="w-5 h-5 flex items-center justify-center text-secondary">
        {/* Allow custom icon color via css if needed, but default is generic text color */}
        <span className="text-secondary hover:text-primary transition-colors">{icon}</span>
      </div>
    </button>
  );

  return (
    <div className={cn("flex items-center bg-white dark:bg-slate-800 border-b p-1 gap-1 relative z-20", className)}>

      {/* Group 1: CRUD */}
      <div className="flex items-center gap-1">
        <BlueItem icon={<Plus size={20} className="stroke-[2.5]" />} onClick={onAdd} title="Add New" />
        <BlueItem icon={<MoreHorizontal size={20} className="stroke-[2.5]" />} onClick={onMore} title="More Actions" />
        <BlueItem icon={<Edit size={18} className="stroke-[2.5]" />} onClick={onEdit} disabled={selectedCount !== 1} title="Edit" />
        <BlueItem icon={<Minus size={20} className="stroke-[2.5]" />} onClick={onDelete} disabled={selectedCount === 0} title="Delete" />
        <BlueItem icon={<Check size={20} className="stroke-[2.5]" />} onClick={onCheck} title="Complete/Approve" />
      </div>

      <div className="h-6 w-px bg-gray-200 dark:bg-slate-600 mx-2" />

      {/* Group 2: Search & Refresh */}
      <div className="flex items-center gap-1">
        <BlueItem
          ref={searchRef}
          icon={<Search size={20} className="stroke-[2.5]" />}
          onClick={onSearchClick}
          title="Search"
          className={isSearchActive ? "bg-blue-100 dark:bg-blue-900/30" : ""}
        />
        <BlueItem icon={<RotateCcw size={18} className="stroke-[2.5]" />} onClick={onReset} title="Refresh/Reset" />
      </div>

      <div className="h-6 w-px bg-gray-200 dark:bg-slate-600 mx-2" />

      {/* Group 3: File Ops */}
      <div className="flex items-center gap-1">
        <BlueItem icon={<Download size={20} className="stroke-[2.5]" />} onClick={onImport} title="Import" />
        <BlueItem icon={<Upload size={20} className="stroke-[2.5]" />} onClick={onExport} title="Export" />
        <BlueItem icon={<Printer size={20} className="stroke-[2.5]" />} onClick={onPrint} title="Print" />
      </div>

      <div className="h-6 w-px bg-gray-200 dark:bg-slate-600 mx-2" />

      {/* Group 4: Help */}
      <div className="flex items-center gap-1">
        <BlueItem icon={<HelpCircle size={20} className="stroke-[2.5]" />} onClick={onHelp} title="Help" />
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
