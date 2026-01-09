import { useState } from 'react';
import { Columns } from 'lucide-react';
import { useClickOutside } from '@/hooks';
import { useTranslation } from 'react-i18next';

interface Column {
  id: string;
  label: string;
}

interface ColumnVisibilityProps {
  columns: Column[];
  visibleColumns: string[];
  onChange: (newVisible: string[]) => void;
}

export const ColumnVisibility = ({ columns, visibleColumns, onChange }: ColumnVisibilityProps) => {
  const { t } = useTranslation();
  const [isOpen, setIsOpen] = useState(false);
  const [filter, setFilter] = useState('');
  
  const dropdownRef = useClickOutside<HTMLDivElement>(() => setIsOpen(false));

  const handleToggle = (id: string) => {
    if (visibleColumns.includes(id)) {
      onChange(visibleColumns.filter((c) => c !== id));
    } else {
      onChange([...visibleColumns, id]);
    }
  };

  const filteredColumns = columns.filter((col) =>
    col.label.toLowerCase().includes(filter.toLowerCase())
  );

  return (
    <div className="relative inline-block text-left" ref={dropdownRef}>
      <button
        type="button"
        className="inline-flex items-center justify-center gap-2 rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 dark:bg-slate-800 dark:border-slate-600 dark:text-gray-200 dark:hover:bg-slate-700"
        onClick={() => setIsOpen(!isOpen)}
      >
        <Columns size={16} />
        <span>{visibleColumns.length} {t('common.columnsVisible') || 'Cột hiển thị'}</span>
      </button>

      {isOpen && (
        <div className="absolute right-0 z-50 mt-2 w-56 origin-top-right rounded-md bg-white shadow-lg ring-1 ring-black ring-opacity-5 focus:outline-none dark:bg-slate-800 dark:ring-slate-700 animate-in fade-in zoom-in-95 duration-100">
          <div className="p-2 border-b dark:border-slate-700">
            <input
              type="text"
              className="w-full rounded-md border border-gray-300 px-2 py-1 text-sm focus:border-indigo-500 focus:outline-none dark:bg-slate-900 dark:border-slate-600"
              placeholder={t('common.search') || 'Tìm kiếm...'}
              value={filter}
              onChange={(e) => setFilter(e.target.value)}
              autoFocus
            />
          </div>
          <div className="max-h-60 overflow-y-auto p-1">
            {filteredColumns.map((col) => (
              <label
                key={col.id}
                className="flex cursor-pointer items-center rounded-md px-2 py-2 hover:bg-gray-100 dark:hover:bg-slate-700"
              >
                <input
                  type="checkbox"
                  className="h-4 w-4 rounded border-gray-300 text-indigo-600 focus:ring-indigo-500"
                  checked={visibleColumns.includes(col.id)}
                  onChange={() => handleToggle(col.id)}
                />
                <span className="ml-2 text-sm text-gray-700 dark:text-gray-200">{col.label}</span>
              </label>
            ))}
            {filteredColumns.length === 0 && (
              <p className="px-2 py-2 text-sm text-gray-500">No columns found</p>
            )}
          </div>
        </div>
      )}
    </div>
  );
};
