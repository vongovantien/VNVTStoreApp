import { useState, useMemo, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Button, Input, Switch, Badge, Modal } from '@/components/ui';
import { AdminPageHeader } from '@/components/admin';
import { DataTable } from '@/components/common';
import { useToast } from '@/store';
import { systemConfigService, SystemConfigDto, UpdateSystemConfigRequest } from '@/services/systemConfigService';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';

const configSchema = z.object({
  configKey: z.string().min(1, 'Key is required').max(50),
  configValue: z.string().min(1, 'Value is required'),
  description: z.string().max(255).optional(),
  isActive: z.boolean()
});

type ConfigFormData = z.infer<typeof configSchema>;

export const SystemConfigsPage = () => {
  const { t } = useTranslation();
  const toast = useToast();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingConfig, setEditingConfig] = useState<SystemConfigDto | null>(null);
  const [configs, setConfigs] = useState<SystemConfigDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);

  const { register, handleSubmit, reset, setValue, watch, formState: { errors } } = useForm<ConfigFormData>({
    resolver: zodResolver(configSchema),
    defaultValues: {
      configKey: '',
      configValue: '',
      description: '',
      isActive: true
    }
  });

  const isActiveValue = watch('isActive');

  const fetchConfigs = useCallback(async () => {
    setIsLoading(true);
    try {
      const res = await systemConfigService.getAll();
      if (res.success && res.data) {
        setConfigs(res.data);
      }
    } catch (err) {
      toast.error(t('messages.loadError'));
    } finally {
      setIsLoading(false);
    }
  }, [t, toast]);

  useEffect(() => {
    fetchConfigs();
  }, [fetchConfigs]);

  const columns = useMemo(() => [
    {
      id: 'configKey',
      header: t('common.fields.code'),
      accessor: (c: SystemConfigDto) => (
        <span className="font-mono text-xs font-bold text-indigo-600 dark:text-indigo-400">
          {c.configKey}
        </span>
      ),
      sortable: true,
      minWidth: '200px'
    },
    {
      id: 'configValue',
      header: t('common.fields.value'),
      accessor: (c: SystemConfigDto) => (
        <div className="max-w-[350px] truncate group relative">
          <span className="text-sm">{c.configValue}</span>
        </div>
      ),
      minWidth: '300px'
    },
    {
      id: 'description',
      header: t('common.fields.description'),
      accessor: (c: SystemConfigDto) => (
        <span className="text-xs text-secondary italic line-clamp-1">{c.description || '-'}</span>
      ),
      minWidth: '200px'
    },
    {
      id: 'status',
      header: t('common.fields.status'),
      accessor: (c: SystemConfigDto) => (
        <Badge 
          color={c.isActive ? 'success' : 'secondary'} 
          variant="soft"
          size="sm"
        >
          {c.isActive ? t('common.status.active') : t('common.status.inactive')}
        </Badge>
      ),
      className: 'text-center',
      minWidth: '120px'
    }
  ], [t]);

  const handleEdit = (config: SystemConfigDto) => {
    setEditingConfig(config);
    reset({
      configKey: config.configKey,
      configValue: config.configValue || '',
      description: config.description || '',
      isActive: config.isActive
    });
    setIsModalOpen(true);
  };

  const handleAddNew = () => {
    setEditingConfig(null);
    reset({
      configKey: '',
      configValue: '',
      description: '',
      isActive: true
    });
    setIsModalOpen(true);
  };

  const handleExport = async () => {
    try {
      await systemConfigService.export();
      toast.success(t('messages.exportSuccess'));
    } catch {
      toast.error(t('messages.exportError'));
    }
  };

  const handleImport = async (file: File) => {
    try {
      const res = await systemConfigService.import(file);
      if (res.success) {
        toast.success(t('messages.importSuccess', { count: res.data ?? 0 }));
        fetchConfigs();
      } else {
        toast.error(res.message || t('messages.importError'));
      }
    } catch {
      toast.error(t('messages.errorOccurred'));
    }
  };

  const onSubmit = async (data: ConfigFormData) => {
    try {
      const payload: UpdateSystemConfigRequest = {
        configKey: data.configKey,
        configValue: data.configValue,
        isActive: data.isActive
      };
      
      const res = await systemConfigService.update(payload);
      if (res.success) {
        toast.success(t('messages.updateSuccess'));
        setIsModalOpen(false);
        fetchConfigs();
      } else {
        toast.error(res.message || t('messages.updateError'));
      }
    } catch (err) {
      toast.error(t('messages.errorOccurred'));
    }
  };

  return (
    <div className="space-y-6">
      <AdminPageHeader
        title={'admin.sidebar.systemConfigs'}
        subtitle={'admin.subtitles.systemConfig'}
      />

      <div className="bg-primary rounded-xl shadow-sm border border-border">
        <DataTable
          columns={columns}
          data={configs}
          keyField="configKey"
          isLoading={isLoading}
          onRefresh={fetchConfigs}
          onAdd={handleAddNew}
          onEdit={handleEdit}
          onImport={handleImport}
          onExport={handleExport}
          importTitle={t('admin.systemConfig.importTitle')}
          searchPlaceholder={t('common.search')}
        />
      </div>

      <Modal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={editingConfig ? t('common.actions.edit') : t('common.actions.create')}
        size="md"
      >
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4 pt-4">
          <Input
            label={t('common.fields.code')}
            {...register('configKey')}
            disabled={!!editingConfig}
            error={errors.configKey?.message || ''}
            placeholder="MAINTENANCE_MODE"
          />
          <Input
            label={t('common.fields.value')}
            {...register('configValue')}
            error={errors.configValue?.message || ''}
            placeholder="true / modern / JSON..."
          />
          <div className="space-y-1">
            <label className="text-sm font-bold text-primary">{t('common.fields.description')}</label>
            <textarea
              {...register('description')}
              className="w-full px-4 py-2 border border-border rounded-lg bg-primary focus:ring-1 focus:ring-indigo-500 outline-none text-sm"
              rows={3}
              placeholder="Mô tả mục đích của cấu hình này..."
            />
          </div>
          <div className="flex items-center justify-between p-3 bg-secondary/50 rounded-lg">
            <span className="text-sm font-medium">{t('common.fields.active')}</span>
            <Switch
              checked={isActiveValue}
              onChange={(val) => setValue('isActive', val)}
            />
          </div>
          <div className="flex justify-end gap-3 pt-4 border-t border-border mt-6">
            <Button variant="ghost" type="button" onClick={() => setIsModalOpen(false)}>
              {t('common.actions.cancel')}
            </Button>
            <Button type="submit">
              {t('common.actions.save')}
            </Button>
          </div>
        </form>
      </Modal>
    </div>
  );
};

export default SystemConfigsPage;
