import React, { useState, useCallback, useMemo, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { ChevronUp, ChevronDown, AlertCircle, Filter } from 'lucide-react';
import { Pagination, Badge, Loading } from '@/components/ui';
import { AdminToolbar } from '@/components/admin/AdminToolbar';
import { ColumnVisibility } from '@/components/admin/ColumnVisibility';
import { TableActions } from '@/components/ui';
import { ImportModal } from '@/components/common/ImportModal';

// ... (existing imports)
import { type FilterDef } from '@/components/common/AdvancedFilter';
import { TableSkeleton } from '@/components/common/TableSkeleton';
import { useExport } from '@/hooks/useExport';
import { type ExportColumn } from '@/utils/export';
import { cn } from '@/utils/cn';
import { PageSize } from '@/constants';

// ============ Types ============
export interface DataTableColumn<T> {
  id: string;
  header: string | React.ReactNode;
  accessor: keyof T | ((row: T) => React.ReactNode);
  sortable?: boolean;
  className?: string;
  headerClassName?: string;
  width?: string | number;
  minWidth?: string | number;
  maxWidth?: string | number;
  noWrap?: boolean;
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
  exportColumns?: ExportColumn<T>[];
  onExportAllData?: () => Promise<T[]>; // Fetch all data for export
  onExport?: (data: T[]) => void; // Manual export handler
  enableColumnVisibility?: boolean;

  // Advanced Filter
  advancedFilterDefs?: FilterDef[];
  onAdvancedSearch?: (filters: Record<string, string>) => void;

  // Actions
  onAdd?: () => void;
  onView?: (item: T) => void;
  onEdit?: (item: T) => void;
  onDelete?: (item: T) => void;
  onBulkDelete?: (items: T[]) => void;
  onRefresh?: () => void;
  onImport?: (file: File) => Promise<void>; // Changed to receive file for internal modal
  importTemplateUrl?: string;
  importTitle?: string;

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
  renderRowActions?: (item: T) => React.ReactNode; // New prop for custom actions

  // Sorting (external control - optional)
  externalSortField?: string;
  externalSortDir?: 'asc' | 'desc';
  onExternalSort?: (field: string, dir: 'asc' | 'desc') => void;

  // Column Visibility (external control - optional)
  visibleColumns?: string[];
  onColumnVisibilityChange?: (cols: string[]) => void;

  // Selection (external control - optional)
  selectedIds?: Set<string>;
  onSelectionChange?: (ids: Set<string>) => void;
  enableSelection?: boolean;

  // Reset
  onReset?: () => void;
  initialFilters?: Record<string, string>;
  onSearch?: (query: string) => void;
  className?: string;
}

// ============ Helper Functions ============
export const createStatusColumn = <T extends object & { isActive?: boolean }>(t: (key: string) => string): DataTableColumn<T> => ({
  id: 'isActive',
  header: t('common.fields.status'),
  accessor: (row: T) => (
    <Badge
      color={row.isActive ? 'success' : 'error'}
      size="sm"
      variant="soft"
    >
      {row.isActive ? t('common.status.active') : t('common.status.inactive')}
    </Badge>
  ),
  className: 'text-center',
  headerClassName: 'text-center',
  width: 120,
});

export const CommonColumns = {
  createStatusColumn,
};

const SortIcon = ({ columnId, sortField, sortDir }: { columnId: string; sortField: string | null; sortDir: 'asc' | 'desc' }) => {
  if (sortField !== columnId) return null;
  return sortDir === 'asc'
    ? <ChevronUp size={14} className="inline ml-1" />
    : <ChevronDown size={14} className="inline ml-1" />;
};

// ============ DataTable Component ============
function DataTableInner<T extends object>({ // Changed from Record<string, unknown> to object
  columns,
  data,
  keyField,
  isLoading = false,
  isFetching = false,
  error = null,
  showToolbar = true,
  exportFilename = 'export',
  exportColumns,
  onExportAllData,
  enableColumnVisibility = true,
  advancedFilterDefs,
  onAdvancedSearch,
  onAdd,
  onView,
  onEdit,
  onDelete,
  onRefresh,
  onImport,
  importTemplateUrl,
  importTitle,
  currentPage: externalPage,
  totalPages: externalTotalPages,
  totalItems: externalTotalItems,
  pageSize: externalPageSize,
  onPageChange: externalOnPageChange,
  onPageSizeChange: externalOnPageSizeChange,
  pageSizeOptions,
  emptyMessage,
  rowClassName,
  externalSortField,
  externalSortDir,
  onExternalSort,
  visibleColumns: externalVisibleColumns,
  onColumnVisibilityChange,
  onReset,
  selectedIds: externalSelectedIds,
  onSelectionChange,
  onBulkDelete,
  enableSelection = true,
  renderRowActions,
  initialFilters,
  onExport,
}: DataTableProps<T>) {
  const { t } = useTranslation();

  // Default empty message with translation fallback
  const finalEmptyMessage = emptyMessage || t('common.noData');


  // ============ Internal State ============
  const [searchQuery, setSearchQuery] = useState('');
  const [searchField, setSearchField] = useState('all');
  const [isImportOpen, setIsImportOpen] = useState(false);

  // Removed debounced search logic as simple search is deprecated


  // Selection State (controlled or uncontrolled)
  const [internalSelectedIds, setInternalSelectedIds] = useState<Set<string>>(new Set());
  const selectedIds = externalSelectedIds || internalSelectedIds;

  const setSelectedIds = useCallback((update: Set<string> | ((prev: Set<string>) => Set<string>)) => {
    let newIds: Set<string>;
    if (typeof update === 'function') {
      newIds = update(selectedIds);
    } else {
      newIds = update;
    }

    if (onSelectionChange) {
      onSelectionChange(newIds);
    } else {
      setInternalSelectedIds(newIds);
    }
  }, [selectedIds, onSelectionChange]);

  // Advanced Filter State
  const [showFilters, setShowFilters] = useState(false);
  const [advancedFilters, setAdvancedFilters] = useState<Record<string, string>>(initialFilters || {});

  // Export Hook
  const { isExporting, exportData } = useExport<T>();

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
  const [internalPageSizeState, setInternalPageSizeState] = useState(externalPageSize || PageSize.DEFAULT);
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
  // Use the computed pageSize (which includes internal state)
  const effectivePageSize = pageSize;
  const internalTotalPages = Math.ceil(sortedData.length / effectivePageSize);
  const paginatedData = useMemo(() => {
    if (isExternalPagination) return sortedData;
    const start = (internalPage - 1) * effectivePageSize;
    return sortedData.slice(start, start + effectivePageSize);
  }, [sortedData, internalPage, effectivePageSize, isExternalPagination]);

  const displayData = isExternalPagination ? sortedData : paginatedData;
  const totalPages = isExternalPagination ? (externalTotalPages || 1) : internalTotalPages;
  const totalItems = isExternalPagination ? (externalTotalItems || sortedData.length) : sortedData.length;

  // Filter columns based on visibility AND append actions column if needed
  const visibleColumnDefs = useMemo(() => {
    const cols = columns.filter(col => visibleColIds.includes(col.id));

    // Check if we need to add an actions column
    const hasActions = onView || onEdit || onDelete || renderRowActions;
    // Check if actions column already exists to avoid duplication if passed manually
    const hasExplicitActionsCol = cols.some(c => c.id === 'actions');

    if (hasActions && !hasExplicitActionsCol) {
      // Create a new array to avoid mutating the original prop if it was reused
      const newCols = [...cols];
      newCols.push({
        id: 'actions',
        header: t('common.fields.action') || 'Action', // Translation fallback
        accessor: (row: T) => (
          <TableActions
            onView={onView ? () => onView(row) : undefined}
            onEdit={onEdit ? () => onEdit(row) : undefined}
            onDelete={onDelete ? () => onDelete(row) : undefined}
            customActions={renderRowActions ? renderRowActions(row) : undefined}
          />
        ),
        width: '100px',
        className: 'text-center',
        headerClassName: 'text-center'
      });
      return newCols;
    }

    return cols;
  }, [columns, visibleColIds, onView, onEdit, onDelete, renderRowActions, t]);

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
  }, [displayData, keyField, setSelectedIds]);

  const handleSelectRow = useCallback((key: string) => {
    setSelectedIds(prev => {
      const next = new Set(prev);
      if (next.has(key)) next.delete(key);
      else next.add(key);
      return next;
    });
  }, [setSelectedIds]);

  const handleExport = useCallback(() => {
    // If onExportAllData is provided, fetch all data first
    const dataToExport = onExportAllData ? onExportAllData() : sortedData;

    // Automatically generate export columns from visible columns if not provided
    const cols = exportColumns || visibleColumnDefs
      .filter(col => col.id !== 'actions') // Don't export actions column
      .map(col => ({
        key: (typeof col.accessor === 'string' ? col.accessor : col.id) as keyof T,
        label: typeof col.header === 'string' ? col.header : col.id,
        width: 15
      }));

    exportData(dataToExport, exportFilename, cols);
  }, [sortedData, exportFilename, exportColumns, onExportAllData, exportData, visibleColumnDefs]);

  const handleReset = useCallback(() => {
    setSearchQuery('');
    setSearchField('all');
    setSelectedIds(new Set());
    setAdvancedFilters({});
    if (onReset) onReset();
    if (onAdvancedSearch) onAdvancedSearch({});
    if (!isExternalSort) {
      setInternalSortField(null);
      setInternalSortDir('asc');
    }
    if (!isExternalPagination) {
      setInternalPage(1);
    }
    // Also trigger refresh if available, as requested
    // Also trigger refresh if available, as requested
    // if (onRefresh) onRefresh(); // Removed to avoid double API call as onAdvancedSearch/onReset likely triggers fetch
  }, [isExternalSort, isExternalPagination, onAdvancedSearch, onReset, setSelectedIds]);

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
    return row[column.accessor as keyof T] as React.ReactNode;
  };


  const showError = error && !isLoading;
  const allSelected = displayData.length > 0 && displayData.every(item => selectedIds.has(String(item[keyField])));

  // Ref for positioning the popup
  const searchButtonRef = React.useRef<HTMLButtonElement>(null);
  const containerRef = React.useRef<HTMLDivElement>(null);
  const [popupLeft, setPopupLeft] = useState(0);

  useEffect(() => {
    if (showFilters && searchButtonRef.current && containerRef.current) {
      const btnRect = searchButtonRef.current.getBoundingClientRect();
      const containerRect = containerRef.current.getBoundingClientRect();
      setPopupLeft(btnRect.left - containerRect.left);
    }
  }, [showFilters]);

  // ============ Render ============
  return (
    <>
      <div className="bg-bg-primary border shadow-sm rounded-xl relative flex flex-col" ref={containerRef}>
        {/* Toolbar */}
        {showToolbar && (
          <div className="p-4 border-b border-border">
            <AdminToolbar
              onAdd={onAdd}
              onRefresh={onRefresh}
              onViewDetails={onView ? () => {
                const item = getSelectedItem();
                if (item) onView(item);
              } : undefined}
              onEdit={onEdit ? () => {
                const item = getSelectedItem();
                if (item) onEdit(item);
              } : undefined}
              onDelete={(onDelete || onBulkDelete) ? () => {
                if (selectedIds.size === 1 && onDelete) {
                  const item = getSelectedItem();
                  if (item) onDelete(item);
                } else if (selectedIds.size > 0 && onBulkDelete) {
                  const items = data.filter(item => selectedIds.has(String(item[keyField])));
                  onBulkDelete(items);
                }
              } : undefined}
              onSearchClick={() => setShowFilters(!showFilters)}
              onReset={handleReset}
              onExport={onExport ? () => onExport(data) : handleExport}
              onImport={onImport ? () => setIsImportOpen(true) : undefined}
              isSearchActive={showFilters}
              isExporting={isExporting}
              selectedCount={selectedIds.size}
              searchRef={searchButtonRef}
              className="w-full"
            >
              {enableColumnVisibility && (
                <ColumnVisibility
                  columns={columns.filter(c => c.id !== 'actions').map(c => ({ id: c.id, label: typeof c.header === 'string' ? c.header : c.id }))}
                  visibleColumns={visibleColIds}
                  onChange={setVisibleColIds}
                />
              )}
            </AdminToolbar>
          </div>
        )}

        {/* Floating Filter Panel */}
        {advancedFilterDefs && showFilters && (
          <div
            className="fixed inset-x-4 top-24 z-[100] md:absolute md:top-16 md:w-[600px] md:inset-auto bg-bg-primary shadow-2xl rounded-xl border border-border p-5 animate-in fade-in zoom-in-95 duration-200 origin-top-left max-h-[80vh] overflow-y-auto"
            style={{
              ...(window.innerWidth >= 768 ? { left: popupLeft > 0 ? popupLeft : '230px' } : {})
            }}
          >

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
              {advancedFilterDefs.map((def) => (
                <div key={def.id} className="space-y-1.5">
                  <label className="text-sm font-semibold text-primary">
                    {def.label}
                  </label>
                  {def.type === 'select' ? (
                    <div className="relative">
                      <select
                        className="w-full pl-3 pr-10 py-2 bg-secondary border border-border rounded-lg focus:ring-2 focus:ring-primary/20 focus:border-primary transition-all text-sm appearance-none outline-none cursor-pointer"
                        value={advancedFilters[def.id] || ''}
                        onChange={(e) => setAdvancedFilters(prev => ({ ...prev, [def.id]: e.target.value }))}
                      >
                        <option value="">{t('common.all')}</option>
                        {def.options?.map(opt => (
                          <option key={opt.value} value={opt.value}>{opt.label}</option>
                        ))}
                      </select>
                      <ChevronDown className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-secondary pointer-events-none" />
                    </div>
                  ) : (
                    <input
                      type={def.type}
                      className="w-full px-3 py-2 bg-secondary border border-border rounded-lg focus:ring-2 focus:ring-primary/20 focus:border-primary transition-all text-sm outline-none placeholder:text-gray-400"
                      placeholder={def.placeholder}
                      value={advancedFilters[def.id] || ''}
                      onChange={(e) => setAdvancedFilters(prev => ({ ...prev, [def.id]: e.target.value }))}
                    />
                  )}
                </div>
              ))}
            </div>

            {/* Action Buttons */}
            <div className="flex justify-center items-center gap-3 pt-4 border-t border-border">
              <button
                className="px-5 py-2 rounded-lg bg-emerald-600 hover:bg-emerald-700 text-white font-medium shadow-md transition-all flex items-center gap-2"
                onClick={() => {
                  if (onAdvancedSearch) onAdvancedSearch(advancedFilters);
                  setShowFilters(false);
                }}
              >
                <Filter size={16} />
                {t('common.actions.apply')}
              </button>
              <button
                className="px-5 py-2 rounded-lg bg-gray-500 hover:bg-gray-600 text-white font-medium shadow-md transition-all flex items-center gap-2"
                onClick={() => setShowFilters(false)}
              >
                {t('common.actions.close')}
              </button>
            </div>
          </div>
        )}

        {/* Table Container */}
        <div className="relative min-h-[400px] flex-1">

          {/* Error Overlay */}
          {showError && (
            <div className="absolute inset-0 bg-bg-primary/80 flex items-center justify-center z-10 backdrop-blur-sm">
              <div className="text-center">
                <AlertCircle className="w-12 h-12 mx-auto mb-4 text-red-500" />
                <h3 className="font-semibold mb-2">{t('messages.error')}</h3>
                <p className="text-secondary text-sm">{error?.message || t('messages.loadError')}</p>
              </div>
            </div>
          )}
          {/* Table Container with Loading Overlay */}
          <div className="relative">
            {/* Table Skeleton / Loading Overlay */}
            {(isLoading || isFetching) && displayData.length === 0 ? (
              <TableSkeleton
                columns={visibleColumnDefs.length}
                rows={pageSize}
                hasSelection={enableSelection}
                hasActions={!!(onView || onEdit || onDelete || renderRowActions)}
              />
            ) : (
              <>
                {(isLoading || isFetching) && (
                  <div className="absolute inset-0 bg-bg-primary/40 z-10 flex items-center justify-center backdrop-blur-[1px]">
                    <Loading />
                  </div>
                )}
                <div className={cn("overflow-x-auto transition-opacity duration-200", (isLoading || isFetching) && "opacity-60")}>
                  <table className="w-full">
                    <thead className="bg-secondary border-b border-border">
                      {/* Main Header Row */}
                      <tr>
                        {/* Selection Checkbox */}
                        {enableSelection && (
                          <th className="px-4 py-3 w-12">
                            <input
                              type="checkbox"
                              checked={allSelected}
                              onChange={(e) => handleSelectAll(e.target.checked)}
                              className="w-4 h-4 rounded border-gray-300 focus:ring-primary"
                            />
                          </th>
                        )}

                        {visibleColumnDefs.map(column => (
                          <th
                            key={column.id}
                            onClick={() => column.sortable && handleSort(column.id)}
                            className={cn(
                              "px-4 py-3 text-left text-sm font-semibold text-secondary",
                              column.sortable && "cursor-pointer hover:bg-tertiary select-none transition-colors",
                              column.headerClassName
                            )}
                            style={{
                              width: column.width,
                              minWidth: column.minWidth,
                              maxWidth: column.maxWidth
                            }}
                          >
                            <div className={cn("flex items-center gap-1", column.noWrap && "whitespace-nowrap")}>
                              {column.header}
                              {column.sortable && <SortIcon columnId={column.id} sortField={sortField} sortDir={sortDir || 'asc'} />}
                            </div>
                          </th>
                        ))}
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-border">
                      {displayData.length === 0 && !isLoading ? (
                        <tr>
                          <td colSpan={visibleColumnDefs.length + (enableSelection ? 1 : 0)} className="px-4 py-12 text-center text-secondary">
                            <div className="flex flex-col items-center justify-center gap-2">
                              <span className="text-slate-400 text-lg">¯\_(ツ)_/¯</span>
                              <p>{finalEmptyMessage}</p>
                            </div>
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
                                "hover:bg-tertiary transition-colors",
                                isSelected && "bg-blue-50/50 dark:bg-blue-900/20",
                                rowClassName?.(row, index)
                              )}
                            >
                              {enableSelection && (
                                <td className="px-4 py-4">
                                  <input
                                    type="checkbox"
                                    checked={isSelected}
                                    onChange={() => handleSelectRow(rowKey)}
                                    className="w-4 h-4 rounded border-gray-300 focus:ring-primary"
                                  />
                                </td>
                              )}

                              {visibleColumnDefs.map(column => (
                                <td
                                  key={column.id}
                                  className={cn(
                                    "px-4 py-4 text-sm",
                                    column.className,
                                    column.noWrap && "whitespace-nowrap"
                                  )}
                                  style={{
                                    maxWidth: column.maxWidth // Apply max-width to td as well to force truncation if needed
                                  }}
                                >
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
              </>
            )}
          </div>

          {/* Pagination */}
          <Pagination
            currentPage={currentPage}
            totalPages={totalPages}
            totalItems={totalItems}
            pageSize={pageSize}
            onPageChange={setCurrentPage}
            onPageSizeChange={handlePageSizeChange}
            pageSizeOptions={pageSizeOptions}
            isLoading={isLoading || isFetching}
          />

        </div>
      </div>

      {/* Import Modal */}
      {onImport && (
        <ImportModal
          isOpen={isImportOpen}
          onClose={() => setIsImportOpen(false)}
          onImport={onImport}
          title={importTitle || t('common.importData')}
          templateUrl={importTemplateUrl}
        />
      )}
    </>
  );
}

export const DataTable = React.memo(DataTableInner) as typeof DataTableInner;
export default DataTable;
