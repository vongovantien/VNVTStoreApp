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
        className="flex items-center gap-2 px-3 py-1.5 rounded-lg hover:bg-secondary transition-all text-sm font-medium text-secondary hover:text-primary group"
        onClick={() => setIsOpen(!isOpen)}
        title={t('common.columnsVisible') || 'Cột hiển thị'}
      >
        <Columns size={18} className="text-tertiary group-hover:text-primary transition-colors" />
        <span>{visibleColumns.length} {t('common.columns') || 'Cột'}</span>
      </button>

      {isOpen && (
        <div className="absolute right-0 top-full mt-2 z-[9999] w-64 origin-top-right rounded-lg bg-primary shadow-xl ring-1 ring-black ring-opacity-5 focus:outline-none animate-in fade-in zoom-in-95 duration-200 border border-border">
          <div className="p-2 border-b border-border bg-secondary rounded-t-lg">
            <input
              type="text"
              className="w-full rounded-md border border-border px-3 py-1.5 text-sm focus:border-blue-500 focus:ring-1 focus:ring-blue-500 focus:outline-none bg-primary"
              placeholder={t('common.search') || 'Tìm kiếm...'}
              value={filter}
              onChange={(e) => setFilter(e.target.value)}
              autoFocus
            />
          </div>
          <div className="max-h-[300px] overflow-y-auto p-1 custom-scrollbar">
            {filteredColumns.map((col) => (
              <label
                key={col.id}
                className="flex cursor-pointer items-center rounded-md px-3 py-2 hover:bg-secondary transition-colors"
              >
                <input
                  type="checkbox"
                  className="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500 cursor-pointer"
                  checked={visibleColumns.includes(col.id)}
                  onChange={() => handleToggle(col.id)}
                />
                <span className="ml-3 text-sm text-secondary select-none">{col.label}</span>
              </label>
            ))}
            {filteredColumns.length === 0 && (
              <p className="px-4 py-3 text-sm text-tertiary text-center">Không tìm thấy</p>
            )}
          </div>
        </div>
      )}
    </div>
  );
};
