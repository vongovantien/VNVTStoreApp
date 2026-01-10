import React, { useState, useCallback, useMemo, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { ChevronUp, ChevronDown, Loader2, AlertCircle, Filter, X } from 'lucide-react';
import { Pagination, Button } from '@/components/ui';
import { AdminToolbar } from '@/components/admin/AdminToolbar';
import { ColumnVisibility } from '@/components/admin/ColumnVisibility';
import { TableToolbar } from '@/pages/admin/components/TableToolbar';
import { AdvancedFilter, type FilterDef } from '@/components/common/AdvancedFilter';
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
  enableColumnVisibility?: boolean;

  // Advanced Filter
  advancedFilterDefs?: FilterDef[];
  onAdvancedSearch?: (filters: Record<string, string>) => void;

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
  onPageSizeChange?: (size: number) => void;
  pageSizeOptions?: number[];

  // Custom rendering
  emptyMessage?: string;
  rowClassName?: (row: T, index: number) => string;

  // Sorting (external control - optional)
  externalSortField?: string;
  externalSortDir?: 'asc' | 'desc';
  onExternalSort?: (field: string, dir: 'asc' | 'desc') => void;

  // Column Visibility (external control - optional)
  visibleColumns?: string[];
  onColumnVisibilityChange?: (cols: string[]) => void;
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
  enableColumnVisibility = true,
  advancedFilterDefs,
  onAdvancedSearch,
  onAdd,
  onEdit,
  onDelete,
  onRefresh,
  currentPage: externalPage,
  totalPages: externalTotalPages,
  totalItems: externalTotalItems,
  pageSize: externalPageSize = 10,
  onPageChange: externalOnPageChange,
  onPageSizeChange: externalOnPageSizeChange,
  pageSizeOptions,
  emptyMessage = 'Không có dữ liệu',
  rowClassName,
  externalSortField,
  externalSortDir,
  onExternalSort,
  visibleColumns: externalVisibleColumns,
  onColumnVisibilityChange,
}: DataTableProps<T>) {
  const { t } = useTranslation();

  // ============ Internal State ============
  const [searchQuery, setSearchQuery] = useState('');
  const [searchField, setSearchField] = useState('all');
  const [showSearch, setShowSearch] = useState(true);
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());

  // Advanced Filter State
  const [showFilters, setShowFilters] = useState(false);
  const [advancedFilters, setAdvancedFilters] = useState<Record<string, string>>({});

  // Column Visibility State (internal)
  const [internalVisibleColumns, setInternalVisibleColumns] = useState<string[]>(
    columns.map(c => c.id)
  );

  // Determine which visible columns to use (external or internal)
  const visibleColIds = externalVisibleColumns || internalVisibleColumns;
  const setVisibleColIds = onColumnVisibilityChange || setInternalVisibleColumns;

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

  // Internal page size state if external is not provided or handled
  const [internalPageSizeState, setInternalPageSizeState] = useState(externalPageSize || 10);
  const pageSize = externalPageSize || internalPageSizeState;

  const handlePageSizeChange = useCallback((newSize: number) => {
    if (externalOnPageSizeChange) {
      externalOnPageSizeChange(newSize);
    } else {
      setInternalPageSizeState(newSize);
      setInternalPage(1); // Reset to first page
    }
  }, [externalOnPageSizeChange]);

  const sortField = isExternalSort ? externalSortField : internalSortField;
  const sortDir = isExternalSort ? externalSortDir : internalSortDir;

  // ============ Filtering & Sorting ============
  const filteredData = useMemo(() => {
    let result = data;

    // 1. Advanced Filters (Client-side only)
    if (!onAdvancedSearch && Object.keys(advancedFilters).length > 0) {
      result = result.filter(item => {
        return Object.entries(advancedFilters).every(([key, value]) => {
          if (!value) return true;
          const itemValue = String(item[key as keyof T] || '').toLowerCase();
          return itemValue.includes(value.toLowerCase());
        });
      });
    }

    // 2. Text Search
    if (searchQuery.trim()) {
      const query = searchQuery.toLowerCase();
      result = result.filter(item => {
        if (searchField === 'all') {
          return Object.values(item).some(val =>
            String(val).toLowerCase().includes(query)
          );
        }
        const fieldValue = item[searchField as keyof T];
        return String(fieldValue).toLowerCase().includes(query);
      });
    }

    return result;
  }, [data, searchQuery, searchField, advancedFilters, onAdvancedSearch]);

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

  // Filter columns based on visibility
  const visibleColumnDefs = useMemo(() => {
    return columns.filter(col => visibleColIds.includes(col.id));
  }, [columns, visibleColIds]);

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
    setAdvancedFilters({}); // Fix: Clear advanced filters
    if (onAdvancedSearch) onAdvancedSearch({}); // Fix: Notify parent
    if (!isExternalSort) {
      setInternalSortField(null);
      setInternalSortDir('asc');
    }
    if (!isExternalPagination) {
      setInternalPage(1);
    }
  }, [isExternalSort, isExternalPagination, onAdvancedSearch]);

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

  // Ref for positioning the popup
  const searchButtonRef = React.useRef<HTMLButtonElement>(null);
  const containerRef = React.useRef<HTMLDivElement>(null);
  const [popupLeft, setPopupLeft] = useState(0);

  useEffect(() => {
    if (showFilters && searchButtonRef.current && containerRef.current) {
      const btnRect = searchButtonRef.current.getBoundingClientRect();
      const containerRect = containerRef.current.getBoundingClientRect();
      // Position left to match the button's left, relative to container
      setPopupLeft(btnRect.left - containerRect.left);
    }
  }, [showFilters]);

  // ============ Render ============
  return (
    <div className="flex flex-col gap-4 relative" ref={containerRef}>
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
              // If advanced filters are defined, the search button opens the filter panel
              // Otherwise, it toggles the standard search bar
              onSearchClick={() => advancedFilterDefs ? setShowFilters(!showFilters) : setShowSearch(!showSearch)}
              onReset={handleReset}
              onExport={handleExport}
              isSearchActive={advancedFilterDefs ? showFilters : showSearch}
              selectedCount={selectedIds.size}
              searchRef={searchButtonRef}
              className="w-full"
            >
              {enableColumnVisibility && (
                <ColumnVisibility
                  columns={columns.map(c => ({ id: c.id, label: typeof c.header === 'string' ? c.header : c.id }))}
                  visibleColumns={visibleColIds}
                  onChange={setVisibleColIds}
                />
              )}
            </AdminToolbar>
          </div>

          {/* TableToolbar removed as per user request to use Advanced Search Popup only */}
        </>
      )}

      {/* Floating Filter Panel - Restored & Enhanced */}
      {advancedFilterDefs && showFilters && (
        <div
          className="absolute top-16 w-full md:w-[600px] bg-white dark:bg-slate-800 shadow-2xl rounded-xl border border-gray-100 dark:border-gray-700 z-50 p-5 animate-in fade-in zoom-in-95 duration-200 origin-top-left"
          style={{ left: popupLeft > 0 ? popupLeft : '230px' }}
        >

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
            {advancedFilterDefs.map((def) => (
              <div key={def.id} className="space-y-1.5">
                <label className="text-sm font-semibold text-gray-700 dark:text-gray-300">
                  {def.label}
                </label>
                {def.type === 'select' ? (
                  <div className="relative">
                    <select
                      className="w-full pl-3 pr-10 py-2 bg-gray-50 dark:bg-slate-900 border border-gray-200 dark:border-gray-700 rounded-lg focus:ring-2 focus:ring-primary/20 focus:border-primary transition-all text-sm appearance-none outline-none cursor-pointer"
                      value={advancedFilters[def.id] || ''}
                      onChange={(e) => setAdvancedFilters(prev => ({ ...prev, [def.id]: e.target.value }))}
                    >
                      <option value="">{t('common.all') || 'Tất cả'}</option>
                      {def.options?.map(opt => (
                        <option key={opt.value} value={opt.value}>{opt.label}</option>
                      ))}
                    </select>
                    <ChevronDown className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
                  </div>
                ) : (
                  <input
                    type={def.type}
                    className="w-full px-3 py-2 bg-gray-50 dark:bg-slate-900 border border-gray-200 dark:border-gray-700 rounded-lg focus:ring-2 focus:ring-primary/20 focus:border-primary transition-all text-sm outline-none placeholder:text-gray-400"
                    placeholder={def.placeholder}
                    value={advancedFilters[def.id] || ''}
                    onChange={(e) => setAdvancedFilters(prev => ({ ...prev, [def.id]: e.target.value }))}
                  />
                )}
              </div>
            ))}
          </div>

          {/* Action Buttons */}
          <div className="flex justify-center items-center gap-3 pt-4 border-t border-gray-100 dark:border-gray-700">
            <button
              className="px-5 py-2 rounded-lg bg-emerald-600 hover:bg-emerald-700 text-white font-medium shadow-md transition-all flex items-center gap-2"
              onClick={() => {
                if (onAdvancedSearch) onAdvancedSearch(advancedFilters);
                setShowFilters(false);
              }}
            >
              <Filter size={16} />
              {t('common.apply') || 'Tìm kiếm'}
            </button>
            <button
              className="px-5 py-2 rounded-lg bg-gray-500 hover:bg-gray-600 text-white font-medium shadow-md transition-all flex items-center gap-2"
              onClick={() => setShowFilters(false)}
            >
              <X size={16} />
              {t('common.close') || 'Đóng'}
            </button>
            <button
              className="px-4 py-2 rounded-lg text-gray-500 hover:text-gray-700 hover:bg-gray-100 font-medium transition-all"
              onClick={() => {
                setAdvancedFilters({});
                if (onAdvancedSearch) onAdvancedSearch({});
              }}
            >
              {t('common.reset') || 'Làm mới'}
            </button>
          </div>
        </div>
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

        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-secondary border-b">
              {/* Main Header Row */}
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

                {visibleColumnDefs.map(column => (
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
                  <td colSpan={visibleColumnDefs.length + 1} className="px-4 py-12 text-center text-secondary">
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

                      {visibleColumnDefs.map(column => (
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
            pageSize={pageSize}
            onPageChange={setCurrentPage}
            onPageSizeChange={handlePageSizeChange}
            pageSizeOptions={pageSizeOptions}
          />
        )}
      </div>
    </div>
  );
}

export default DataTable;
