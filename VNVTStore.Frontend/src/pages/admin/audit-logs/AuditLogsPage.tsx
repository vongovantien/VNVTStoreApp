import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { ColumnDef } from '@tanstack/react-table';
import { format } from 'date-fns';

import { DataTable } from '@/components/ui/data-table';
import { Badge } from '@/components/ui';
import { useDataTable } from '@/hooks/useDataTable';
import auditLogService, { AuditLogDto } from '@/services/auditLogService';

export const AuditLogsPage = () => {
    const { t } = useTranslation();
    
    // Columns definition
    const columns: ColumnDef<AuditLogDto>[] = [
        {
            accessorKey: 'createdAt',
            header: t('auditLog.createdAt', 'Time'),
            cell: ({ row }: { row: { original: AuditLogDto } }) => format(new Date(row.original.createdAt), 'dd/MM/yyyy HH:mm:ss'),
        },
        {
            accessorKey: 'userName',
            header: t('auditLog.user', 'User'),
            cell: ({ row }: { row: { original: AuditLogDto } }) => (
                <div className="flex flex-col">
                    <span className="font-medium">{row.original.userName}</span>
                    <span className="text-xs text-muted-foreground">{row.original.userCode}</span>
                </div>
            ),
        },
        {
            accessorKey: 'action',
            header: t('auditLog.action', 'Action'),
            cell: ({ row }: { row: { original: AuditLogDto } }) => (
                <Badge variant="outline" className="font-mono">
                    {row.original.action}
                </Badge>
            ),
        },
        {
            accessorKey: 'target',
            header: t('auditLog.target', 'Target'),
            cell: ({ row }: { row: { original: AuditLogDto } }) => <span className="font-mono text-xs">{row.original.target}</span>,
        },
        {
            accessorKey: 'detail',
            header: t('auditLog.detail', 'Detail'),
            cell: ({ row }: { row: { original: AuditLogDto } }) => (
                <div className="max-w-[300px] truncate" title={row.original.detail}>
                    {row.original.detail}
                </div>
            ),
        },
        {
            accessorKey: 'ipAddress',
            header: t('auditLog.ipAddress', 'IP Address'),
            cell: ({ row }: { row: { original: AuditLogDto } }) => <span className="font-mono text-xs">{row.original.ipAddress}</span>,
        },
    ];

    const {
        data,
        isLoading,
        pageCount,
        pagination,
        setPagination,
        sorting,
        setSorting,
        columnFilters,
        setColumnFilters,
    } = useDataTable({
        // @ts-expect-error - Base service structure mismatch with hook expectation but works at runtime
        service: auditLogService,
        defaultSort: { id: 'createdAt', desc: true },
        // useDataTable automatically maps state to RequestDTO structure (pageIndex, pageSize, searching, sortDTO)
    });

    return (
        <div className="space-y-4 p-8">
            <div className="flex items-center justify-between">
                <h2 className="text-3xl font-bold tracking-tight">{t('auditLog.title', 'System Audit Logs')}</h2>
            </div>
            
            <DataTable
                columns={columns}
                data={data}
                pageCount={pageCount}
                pagination={pagination}
                onPaginationChange={setPagination}
                sorting={sorting}
                onSortingChange={setSorting}
                columnFilters={columnFilters}
                onColumnFiltersChange={setColumnFilters}
                isLoading={isLoading}
            />
        </div>
    );
};

export default AuditLogsPage;
