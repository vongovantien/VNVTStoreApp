import { Search, Download, Upload, Trash2 } from 'lucide-react';
import { Button } from '@/components/ui';

interface TableToolbarProps {
  // Search
  searchQuery: string;
  onSearchChange: (value: string) => void;
  searchField: string;
  onSearchFieldChange: (value: string) => void;
  searchOptions: { label: string; value: string }[];
  
  // Selection
  selectedCount: number;
  onBulkDelete?: () => void;
  
  // Actions
  onExport: () => void;
  onImport?: (file: File) => void; // Made optional - backend doesn't support import yet
}

export const TableToolbar = ({
  searchQuery,
  onSearchChange,
  searchField,
  onSearchFieldChange,
  searchOptions,
  selectedCount,
  onBulkDelete,
  onExport,
  onImport,
}: TableToolbarProps) => {
  
  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>): void => {
    const file = e.target.files?.[0];
    if (file && onImport) {
      onImport(file);
    }
  };

  return (
    <div className="bg-primary p-4 rounded-xl space-y-4 mb-6 border shadow-sm">
      <div className="flex flex-col md:flex-row justify-between gap-4">
        {/* Search Section */}
        <div className="flex flex-1 gap-2">
          <select
            value={searchField}
            onChange={(e) => onSearchFieldChange(e.target.value)}
            className="px-3 py-2 border rounded-lg bg-secondary focus:outline-none focus:border-accent w-auto min-w-[120px]"
          >
            <option value="all">Tất cả</option>
            {searchOptions.map((opt) => (
              <option key={opt.value} value={opt.value}>{opt.label}</option>
            ))}
          </select>
          
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-tertiary" size={18} />
            <input
              type="text"
              placeholder="Tìm kiếm..."
              value={searchQuery}
              onChange={(e) => onSearchChange(e.target.value)}
              className="w-full pl-10 pr-4 py-2 border rounded-lg bg-transparent focus:outline-none focus:border-accent"
            />
          </div>
        </div>

        {/* Actions Section */}
        <div className="flex gap-2">
           {onImport && (
             <label className="cursor-pointer inline-flex">
               <input type="file" accept=".csv" className="hidden" onChange={handleFileChange} />
               <div className="inline-flex items-center justify-center rounded-lg text-sm font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring disabled:pointer-events-none disabled:opacity-50 ring-offset-background border border-input hover:bg-accent hover:text-accent-foreground h-9 px-3 py-2 gap-2">
                  <Upload size={16} /> Import
               </div>
             </label>
           )}
           
           <Button variant="outline" size="sm" leftIcon={<Download size={16} />} onClick={onExport}>
             Export
           </Button>
        </div>
      </div>

      {/* Bulk Actions Indicator */}
      {selectedCount > 0 && (
        <div className="flex items-center justify-between bg-accent/10 text-accent p-2 rounded-lg border border-accent/20 animate-in fade-in slide-in-from-top-2">
          <div className="flex items-center gap-2">
            <span className="font-semibold">{selectedCount} đã chọn</span>
            <span className="text-sm opacity-80">|</span>
            <button className="text-sm hover:underline" onClick={onBulkDelete}>
                Xóa tất cả
            </button>
          </div>
          
          {onBulkDelete && (
             <Button 
               variant="primary" 
               size="sm" 
               className="bg-error hover:bg-error/90 border-error text-white h-8"
               leftIcon={<Trash2 size={14} />}
               onClick={onBulkDelete}
             >
               Xóa {selectedCount} mục
             </Button>
          )}
        </div>
      )}
    </div>
  );
};
