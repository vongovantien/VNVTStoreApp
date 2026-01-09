import { useState, useCallback, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { ChevronUp, ChevronDown, Loader2, AlertCircle } from 'lucide-react';
import { Pagination } from '@/components/ui';
import { AdminToolbar } from '@/components/admin/AdminToolbar';
import { TableToolbar } from '@/pages/admin/components/TableToolbar';
import { exportToCSV } from '@/utils/export';
import { cn } from '@/utils/cn';

// ============ Types ============
export interface DataTableColumn<T> {
  id: string;
  header: string | React.ReactNode;
  accessor: keyof T | ((row: T) => React.ReactNode);
  sortable?: boolean;
  className?: string;
  headerClassName?: string;
}

export interface DataTableProps<T extends object> {
  // Data
  columns: DataTableColumn<T>[];
  data: T[];
  keyField: keyof T;
  
  // Loading/Error
  isLoading?: boolean;
  isFetching?: boolean;
  error?: Error | null;
  
  // Toolbar
  showToolbar?: boolean;
  title?: string;
  searchPlaceholder?: string;
  searchOptions?: { label: string; value: string }[];
  exportFilename?: string;
  
  // Actions
  onAdd?: () => void;
  onEdit?: (item: T) => void;
  onDelete?: (item: T) => void;
  onRefresh?: () => void;
  
  // Pagination (external control)
  currentPage?: number;
  totalPages?: number;
  totalItems?: number;
  pageSize?: number;
  onPageChange?: (page: number) => void;
  
  // Custom rendering
  emptyMessage?: string;
  rowClassName?: (row: T, index: number) => string;
  
  // Sorting (external control - optional)
  externalSortField?: string;
  externalSortDir?: 'asc' | 'desc';
  onExternalSort?: (field: string, dir: 'asc' | 'desc') => void;
}

// ============ DataTable Component ============
export function DataTable<T extends object>({
  columns,
  data,
  keyField,
  isLoading = false,
  isFetching = false,
  error = null,
  showToolbar = true,
  title,
  searchPlaceholder = 'Tìm kiếm...',
  searchOptions = [],
  exportFilename = 'export',
  onAdd,
  onEdit,
  onDelete,
  onRefresh,
  currentPage: externalPage,
  totalPages: externalTotalPages,
  totalItems: externalTotalItems,
  pageSize: externalPageSize = 10,
  onPageChange: externalOnPageChange,
  emptyMessage = 'Không có dữ liệu',
  rowClassName,
  externalSortField,
  externalSortDir,
  onExternalSort,
}: DataTableProps<T>) {
  const { t } = useTranslation();
  
  // ============ Internal State ============
  const [searchQuery, setSearchQuery] = useState('');
  const [searchField, setSearchField] = useState('all');
  const [showSearch, setShowSearch] = useState(true);
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  
  // Internal sorting (used when no external sort provided)
  const [internalSortField, setInternalSortField] = useState<string | null>(null);
  const [internalSortDir, setInternalSortDir] = useState<'asc' | 'desc'>('asc');
  
  // Internal pagination (used when no external pagination provided)
  const [internalPage, setInternalPage] = useState(1);
  
  // Determine if using external or internal control
  const isExternalPagination = externalPage !== undefined && externalOnPageChange !== undefined;
  const isExternalSort = externalSortField !== undefined && onExternalSort !== undefined;
  
  const currentPage = isExternalPagination ? externalPage : internalPage;
  const setCurrentPage = isExternalPagination ? externalOnPageChange : setInternalPage;
  
  const sortField = isExternalSort ? externalSortField : internalSortField;
  const sortDir = isExternalSort ? externalSortDir : internalSortDir;

  // ============ Filtering & Sorting ============
  const filteredData = useMemo(() => {
    if (!searchQuery.trim()) return data;
    
    const query = searchQuery.toLowerCase();
    return data.filter(item => {
      if (searchField === 'all') {
        return Object.values(item).some(val => 
          String(val).toLowerCase().includes(query)
        );
      }
      const fieldValue = item[searchField as keyof T];
      return String(fieldValue).toLowerCase().includes(query);
    });
  }, [data, searchQuery, searchField]);

  const sortedData = useMemo(() => {
    if (!sortField || isExternalSort) return filteredData;
    
    return [...filteredData].sort((a, b) => {
      const aVal = a[sortField as keyof T];
      const bVal = b[sortField as keyof T];
      
      let comparison = 0;
      if (typeof aVal === 'string' && typeof bVal === 'string') {
        comparison = aVal.localeCompare(bVal);
      } else if (typeof aVal === 'number' && typeof bVal === 'number') {
        comparison = aVal - bVal;
      } else if (typeof aVal === 'boolean' && typeof bVal === 'boolean') {
        comparison = aVal === bVal ? 0 : aVal ? -1 : 1;
      }
      
      return sortDir === 'desc' ? -comparison : comparison;
    });
  }, [filteredData, sortField, sortDir, isExternalSort]);

  // Internal pagination
  const internalPageSize = externalPageSize;
  const internalTotalPages = Math.ceil(sortedData.length / internalPageSize);
  const paginatedData = useMemo(() => {
    if (isExternalPagination) return sortedData;
    const start = (internalPage - 1) * internalPageSize;
    return sortedData.slice(start, start + internalPageSize);
  }, [sortedData, internalPage, internalPageSize, isExternalPagination]);

  const displayData = isExternalPagination ? sortedData : paginatedData;
  const totalPages = isExternalPagination ? (externalTotalPages || 1) : internalTotalPages;
  const totalItems = isExternalPagination ? (externalTotalItems || sortedData.length) : sortedData.length;

  // ============ Handlers ============
  const handleSort = useCallback((columnId: string) => {
    if (isExternalSort) {
      const newDir = externalSortField === columnId && externalSortDir === 'asc' ? 'desc' : 'asc';
      onExternalSort(columnId, newDir);
    } else {
      if (internalSortField === columnId) {
        setInternalSortDir(prev => prev === 'asc' ? 'desc' : 'asc');
      } else {
        setInternalSortField(columnId);
        setInternalSortDir('asc');
      }
    }
  }, [isExternalSort, externalSortField, externalSortDir, onExternalSort, internalSortField]);

  const handleSelectAll = useCallback((checked: boolean) => {
    if (checked) {
      setSelectedIds(new Set(displayData.map(item => String(item[keyField]))));
    } else {
      setSelectedIds(new Set());
    }
  }, [displayData, keyField]);

  const handleSelectRow = useCallback((key: string) => {
    setSelectedIds(prev => {
      const next = new Set(prev);
      if (next.has(key)) next.delete(key);
      else next.add(key);
      return next;
    });
  }, []);

  const handleExport = useCallback(() => {
    exportToCSV(sortedData, exportFilename);
  }, [sortedData, exportFilename]);

  const handleReset = useCallback(() => {
    setSearchQuery('');
    setSearchField('all');
    setSelectedIds(new Set());
    if (!isExternalSort) {
      setInternalSortField(null);
      setInternalSortDir('asc');
    }
    if (!isExternalPagination) {
      setInternalPage(1);
    }
  }, [isExternalSort, isExternalPagination]);

  // Get selected item for edit/delete
  const getSelectedItem = useCallback((): T | null => {
    if (selectedIds.size !== 1) return null;
    const key = Array.from(selectedIds)[0];
    return data.find(item => String(item[keyField]) === key) || null;
  }, [selectedIds, data, keyField]);

  // ============ Render Helpers ============
  const getCellValue = (row: T, column: DataTableColumn<T>): React.ReactNode => {
    if (typeof column.accessor === 'function') {
      return column.accessor(row);
    }
    return row[column.accessor] as React.ReactNode;
  };

  const SortIcon = ({ columnId }: { columnId: string }) => {
    if (sortField !== columnId) return null;
    return sortDir === 'asc' 
      ? <ChevronUp size={14} className="inline ml-1" />
      : <ChevronDown size={14} className="inline ml-1" />;
  };

  const showError = error && !isLoading;
  const allSelected = displayData.length > 0 && selectedIds.size === displayData.length;

  // ============ Render ============
  return (
    <div className="flex flex-col gap-4">
      {/* Toolbar */}
      {showToolbar && (
        <>
          <div className="flex justify-between items-center gap-4 bg-white dark:bg-slate-800 p-2 rounded-lg border shadow-sm">
            <AdminToolbar
              onAdd={onAdd}
              onEdit={onEdit ? () => {
                const item = getSelectedItem();
                if (item) onEdit(item);
              } : undefined}
              onDelete={onDelete ? () => {
                const item = getSelectedItem();
                if (item) onDelete(item);
              } : undefined}
              onSearchClick={() => setShowSearch(!showSearch)}
              onReset={handleReset}
              onExport={handleExport}
              isSearchActive={showSearch}
              selectedCount={selectedIds.size}
            />
          </div>

          {showSearch && (
            <TableToolbar
              searchQuery={searchQuery}
              onSearchChange={(val) => { setSearchQuery(val); if (!isExternalPagination) setInternalPage(1); }}
              searchField={searchField}
              onSearchFieldChange={setSearchField}
              searchOptions={searchOptions}
              selectedCount={selectedIds.size}
              onExport={handleExport}
            />
          )}
        </>
      )}

      {/* Table Container */}
      <div className="bg-primary rounded-xl overflow-hidden shadow-sm border relative min-h-[400px]">
        {/* Loading Overlay */}
        {(isLoading || isFetching) && (
          <div className="absolute inset-0 bg-primary/70 flex items-center justify-center z-10">
            <div className="flex items-center gap-3">
              <Loader2 className="w-6 h-6 animate-spin text-indigo-600" />
              <span className="text-secondary text-sm">{t('common.loading')}</span>
            </div>
          </div>
        )}

        {/* Error Overlay */}
        {showError && (
          <div className="absolute inset-0 bg-primary flex items-center justify-center z-10">
            <div className="text-center">
              <AlertCircle className="w-12 h-12 mx-auto mb-4 text-red-500" />
              <h3 className="font-semibold mb-2">{t('messages.error')}</h3>
              <p className="text-secondary text-sm">{error?.message || t('messages.loadError')}</p>
            </div>
          </div>
        )}

        {/* Table */}
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-secondary border-b">
              <tr>
                {/* Selection Checkbox */}
                <th className="px-4 py-3 w-12">
                  <input
                    type="checkbox"
                    checked={allSelected}
                    onChange={(e) => handleSelectAll(e.target.checked)}
                    className="w-4 h-4 rounded border-gray-300"
                  />
                </th>
                
                {columns.map(column => (
                  <th
                    key={column.id}
                    className={cn(
                      "px-4 py-3 text-left text-sm font-semibold",
                      column.sortable && "cursor-pointer hover:bg-secondary/80 select-none",
                      column.headerClassName
                    )}
                    onClick={column.sortable ? () => handleSort(column.id) : undefined}
                  >
                    {column.header}
                    {column.sortable && <SortIcon columnId={column.id} />}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {displayData.length === 0 && !isLoading ? (
                <tr>
                  <td colSpan={columns.length + 1} className="px-4 py-12 text-center text-secondary">
                    {emptyMessage}
                  </td>
                </tr>
              ) : (
                displayData.map((row, index) => {
                  const rowKey = String(row[keyField]);
                  const isSelected = selectedIds.has(rowKey);
                  
                  return (
                    <tr
                      key={rowKey}
                      className={cn(
                        "border-b last:border-0 hover:bg-secondary/50 transition-colors",
                        isSelected && "bg-blue-50 dark:bg-blue-900/20",
                        rowClassName?.(row, index)
                      )}
                    >
                      <td className="px-4 py-4">
                        <input
                          type="checkbox"
                          checked={isSelected}
                          onChange={() => handleSelectRow(rowKey)}
                          className="w-4 h-4 rounded border-gray-300"
                        />
                      </td>
                      
                      {columns.map(column => (
                        <td key={column.id} className={cn("px-4 py-4", column.className)}>
                          {getCellValue(row, column)}
                        </td>
                      ))}
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>

        {/* Pagination */}
        {totalPages > 1 && (
          <Pagination
            currentPage={currentPage}
            totalPages={totalPages}
            totalItems={totalItems}
            pageSize={externalPageSize}
            onPageChange={setCurrentPage}
          />
        )}
      </div>
    </div>
  );
}

export default DataTable;
