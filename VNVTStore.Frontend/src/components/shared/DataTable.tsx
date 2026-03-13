import React from 'react';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
  Button,
} from '@/components/ui'; // Assuming these exist in components/ui from Shadcn
import { cn } from '@/utils/cn'; // Standard shadcn utils
import { Loader2 } from 'lucide-react';

export interface ColumnDef<T> {
  header: string;
  accessorKey?: keyof T;
  cell?: (item: T) => React.ReactNode;
  className?: string;
}

interface DataTableProps<T> {
  data: T[];
  columns: ColumnDef<T>[];
  isLoading?: boolean;
  onRowClick?: (item: T) => void;
  className?: string;
  page?: number;
  totalPages?: number;
  onPageChange?: (page: number) => void;
}

export const DataTable = <T,>({
  data,
  columns,
  isLoading,
  onRowClick,
  className,
  page = 1,
  totalPages = 1,
  onPageChange,
}: DataTableProps<T>) => {
  if (isLoading) {
    return (
      <div className="flex h-48 w-full items-center justify-center border rounded-md bg-slate-50/50">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }

  if (!data?.length) {
    return (
      <div className="flex h-32 w-full items-center justify-center border rounded-md bg-slate-50/50 text-slate-500">
        No data available
      </div>
    );
  }

  return (
    <div className={cn("space-y-4", className)}>
      <div className="rounded-md border overflow-hidden">
        <Table>
          <TableHeader className="bg-slate-100 dark:bg-slate-800">
            <TableRow>
              {columns.map((col, index) => (
                <TableHead key={index} className={col.className}>
                  {col.header}
                </TableHead>
              ))}
            </TableRow>
          </TableHeader>
          <TableBody>
            {data.map((row, rowIndex) => (
              <TableRow
                key={rowIndex}
                onClick={() => onRowClick?.(row)}
                className={cn(
                  "hover:bg-slate-50/50 dark:hover:bg-slate-800/50 transition-colors",
                  onRowClick && "cursor-pointer"
                )}
              >
                {columns.map((col, colIndex) => (
                  <TableCell key={colIndex} className={col.className}>
                    {col.cell
                      ? col.cell(row)
                      : col.accessorKey
                      ? (row[col.accessorKey] as React.ReactNode)
                      : null}
                  </TableCell>
                ))}
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      {totalPages > 1 && (
        <div className="flex items-center justify-end space-x-2">
          <Button
            variant="outline"
            size="sm"
            onClick={() => onPageChange?.(Math.max(1, (page || 1) - 1))}
            disabled={!page || page <= 1}
          >
            Previous
          </Button>
          <div className="text-sm text-slate-600 dark:text-slate-400">
            Page {page} of {totalPages}
          </div>
          <Button
            variant="outline"
            size="sm"
            onClick={() => onPageChange?.(Math.min(totalPages || 1, (page || 1) + 1))}
            disabled={!page || page >= (totalPages || 1)}
          >
            Next
          </Button>
        </div>
      )}
    </div>
  );
};
