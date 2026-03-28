import { useState, useMemo, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Eye, EyeOff, ShieldCheck, Key } from 'lucide-react';
import { Button, Input, Modal, Badge } from '@/components/ui';
import { AdminPageHeader } from '@/components/admin';
import { DataTable } from '@/components/common';
import { useToast } from '@/store';
import { systemSecretService, SystemSecretDto, UpdateSystemSecretRequest } from '@/services/systemSecretService';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';

const secretSchema = z.object({
  key: z.string().min(1, 'Key is required').max(100),
  value: z.string().min(1, 'Value is required'),
  description: z.string().max(255).optional()
});

type SecretFormData = z.infer<typeof secretSchema>;

export const SystemSecretsPage = () => {
  const { t } = useTranslation();
  const toast = useToast();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingSecret, setEditingSecret] = useState<SystemSecretDto | null>(null);
  const [secrets, setSecrets] = useState<SystemSecretDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [visibleKeys, setVisibleKeys] = useState<Set<string>>(new Set());

  const toggleVisibility = (code: string) => {
    const newSet = new Set(visibleKeys);
    if (newSet.has(code)) newSet.delete(code);
    else newSet.add(code);
    setVisibleKeys(newSet);
  };

  const { register, handleSubmit, reset, formState: { errors } } = useForm<SecretFormData>({
    resolver: zodResolver(secretSchema),
    defaultValues: {
      key: '',
      value: '',
      description: ''
    }
  });

  const fetchSecrets = useCallback(async () => {
    setIsLoading(true);
    try {
      const res = await systemSecretService.getAll();
      if (res.success && res.data) {
        setSecrets(res.data);
      }
    } catch (err) {
      toast.error(t('messages.loadError'));
    } finally {
      setIsLoading(false);
    }
  }, [t, toast]);

  useEffect(() => {
    fetchSecrets();
  }, [fetchSecrets]);

  const columns = useMemo(() => [
    {
      id: 'key',
      header: t('common.fields.code'),
      accessor: (s: SystemSecretDto) => (
        <div className="flex items-center gap-2">
            <Key size={14} className="text-amber-500" />
            <span className="font-mono text-xs font-bold text-slate-700 dark:text-slate-300">
                {s.code}
            </span>
        </div>
      ),
      sortable: true,
      minWidth: '200px'
    },
    {
      id: 'value',
      header: t('common.fields.value'),
      accessor: (s: SystemSecretDto) => {
        const isVisible = visibleKeys.has(s.code);
        return (
          <div className="flex items-center gap-2 min-w-[200px]">
            <span className="text-sm font-mono flex-1 truncate">
              {isVisible ? s.secretValue : '••••••••••••••••'}
            </span>
            <button
              onClick={() => toggleVisibility(s.code)}
              className="p-1 hover:bg-slate-100 dark:hover:bg-slate-800 rounded transition-colors text-slate-400"
              title={isVisible ? t('common.hide') : t('common.show')}
            >
              {isVisible ? <EyeOff size={14} /> : <Eye size={14} />}
            </button>
          </div>
        );
      },
      minWidth: '300px'
    },
    {
        id: 'security',
        header: 'Security',
        accessor: (s: SystemSecretDto) => (
            <Badge 
              color={s.isEncrypted ? 'info' : 'secondary'} 
              variant="soft" 
              size="sm"
            >
                {s.isEncrypted ? 'Encrypted' : 'Plain'}
            </Badge>
        ),
        minWidth: '100px'
    }
  ], [t, visibleKeys]);

  const handleEdit = (secret: SystemSecretDto) => {
    setEditingSecret(secret);
    reset({
      key: secret.code,
      value: secret.secretValue || '',
      description: secret.description || ''
    });
    setIsModalOpen(true);
  };

  const handleAddNew = () => {
    setEditingSecret(null);
    reset({
      key: '',
      value: '',
      description: ''
    });
    setIsModalOpen(true);
  };

  const handleDelete = async (secret: SystemSecretDto) => {
    if (!confirm(t('messages.confirmDelete', { count: 1 }))) return;
    try {
      const res = await systemSecretService.delete(secret.code);
      if (res.success) {
        toast.success(t('messages.deleteSuccess'));
        fetchSecrets();
      }
    } catch {
      toast.error(t('messages.errorOccurred'));
    }
  };

  const handleExport = async () => {
    try {
      await systemSecretService.export();
      toast.success(t('messages.exportSuccess'));
    } catch {
      toast.error(t('messages.exportError'));
    }
  };

  const handleImport = async (file: File) => {
    try {
      const res = await systemSecretService.import(file);
      if (res.success) {
        toast.success(t('messages.importSuccess', { count: res.data ?? 0 }));
        fetchSecrets();
      } else {
        toast.error(res.message || t('messages.importError'));
      }
    } catch {
      toast.error(t('messages.errorOccurred'));
    }
  };

  const onSubmit = async (data: SecretFormData) => {
    try {
      const payload: UpdateSystemSecretRequest = {
        key: data.key,
        value: data.value,
        description: data.description || null
      };
      
      const res = await systemSecretService.update(payload);
      if (res.success) {
        toast.success(t('messages.saveSuccess'));
        setIsModalOpen(false);
        fetchSecrets();
      } else {
        toast.error(res.message || t('messages.saveError'));
      }
    } catch (err) {
      toast.error(t('messages.errorOccurred'));
    }
  };

  return (
    <div className="space-y-6">
      <AdminPageHeader
        title={'admin.sidebar.systemSecrets'}
        subtitle={'admin.subtitles.systemSecrets'}
      />

      <div className="bg-primary rounded-xl shadow-sm border border-border overflow-hidden">
        <div className="p-4 bg-amber-50 dark:bg-amber-900/10 border-b border-amber-100 dark:border-amber-900/30 flex items-center gap-3">
            <ShieldCheck className="text-amber-600 dark:text-amber-500" size={20} />
            <p className="text-sm text-amber-800 dark:text-amber-300 font-medium">
                {t('admin.systemSecrets.warning', 'Chú ý: Các tham số này ảnh hưởng trực tiếp đến bảo mật hệ thống. Vui lòng cẩn thận khi thay đổi.')}
            </p>
        </div>
        <DataTable
          columns={columns}
          data={secrets}
          keyField="code"
          isLoading={isLoading}
          onRefresh={fetchSecrets}
          onAdd={handleAddNew}
          onEdit={handleEdit}
          onDelete={handleDelete}
          onImport={handleImport}
          onExport={handleExport}
          importTitle={t('admin.systemSecrets.importTitle')}
          searchPlaceholder={t('common.search')}
        />
      </div>

      <Modal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={editingSecret ? t('common.actions.edit') : t('common.actions.create')}
        size="md"
      >
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4 pt-4">
          <Input
            label={t('common.fields.code')}
            {...register('key')}
            disabled={!!editingSecret}
            error={errors.key?.message || ''}
            placeholder="FIREBASE_API_KEY"
          />
          <div className="space-y-1">
            <label className="text-sm font-bold text-primary">{t('common.fields.value')}</label>
            <textarea
              {...register('value')}
              className="w-full px-4 py-2 border border-border rounded-lg bg-primary focus:ring-1 focus:ring-indigo-500 outline-none text-sm font-mono"
              rows={4}
              placeholder="Nhập giá trị bí mật..."
            />
            {errors.value && <p className="text-xs text-red-500">{errors.value.message}</p>}
          </div>
          <Input
            label={t('common.fields.description')}
            {...register('description')}
            error={errors.description?.message || ''}
            placeholder="Mô tả tham số này..."
          />
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

export default SystemSecretsPage;
