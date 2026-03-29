import { useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { DataTable, type DataTableColumn } from '@/components/common/DataTable';
import { useEntityManager } from '@/hooks/useEntityManager';
import { tagService } from '@/services/tagService';
import { Tag } from '@/types';
import { AdminPageHeader } from '@/components/admin';
import { useQuery } from '@tanstack/react-query';
import { Button } from '@/components/ui';
import { BaseForm } from '@/components/common/BaseForm';
import { z } from 'zod';

const AdminTagsPage = () => {
  const { t } = useTranslation();

  // Fetch Tags
  const { data: response, isLoading, isFetching } = useQuery({
    queryKey: ['tags'],
    queryFn: () => tagService.getAll(100),
  });

  const tags = response?.data?.items || []; // Use items from PagedResult

  // Entity Manager
  const {
    isFormOpen,
    editingItem,
    openCreate,
    openEdit,
    closeForm,
    create,
    update,
    delete: deleteTag,
    isCreating,
    isUpdating
  } = useEntityManager<Tag>({
    service: tagService,
    queryKey: ['tags'],
    translations: {
      createSuccess: 'Tạo thẻ thành công',
      updateSuccess: 'Cập nhật thẻ thành công',
      deleteSuccess: 'Xóa thẻ thành công'
    }
  });

  // Columns
  const columns = useMemo<DataTableColumn<Tag>[]>(() => [
    {
      id: 'id',
      header: 'ID',
      accessor: 'id',
      sortable: true,
      width: '80px'
    },
    {
      id: 'name',
      header: t('common.fields.name'),
      accessor: 'name',
      sortable: true,
    },
    {
      id: 'isActive',
      header: t('common.fields.status'),
      accessor: (row: Tag) => (
        <span className={`px-2 py-1 rounded-full text-xs font-medium ${row.isActive ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'}`}>
          {row.isActive ? t('common.status.active') : t('common.status.inactive')}
        </span>
      ),
      sortable: true,
      width: '120px'
    }
  ], [t]);

  // Form Schema (Zod)
  const schema = z.object({
    name: z.string().min(1, t('validation.required')),
    isActive: z.boolean()
  });

  type TagFormData = z.infer<typeof schema>;

  const handleSubmit = (data: TagFormData) => {
    if (editingItem) {
      update(editingItem.id, data);
    } else {
      create(data);
    }
  };

  return (
    <div className="space-y-6">
      <AdminPageHeader
        title={t('admin.tags.title')}
        subtitle={t('admin.tags.subtitle')}
        rightSection={
          <Button onClick={openCreate} className="gap-2">
            {t('common.create')}
          </Button>
        }
      />

      <div className="bg-primary rounded-xl shadow-sm border border-border overflow-hidden">
        <DataTable
          columns={columns}
          data={tags}
          keyField="id"
          isLoading={isLoading}
          isFetching={isFetching}
          onEdit={openEdit}
          onDelete={(item) => {
             // Use window.confirm or useConfirm hook if needed
             if (window.confirm(t('common.messages.confirmDeleteCount', { count: 1 }))) {
                deleteTag(item.id);
             }
          }}
          searchPlaceholder={t('common.placeholders.search')}
        />
      </div>

      {/* BaseForm handles Modal internally */}
      <BaseForm
         isModal={true}
         modalOpen={isFormOpen}
         onModalClose={closeForm}
          modalTitle={editingItem ? t('admin.tags.edit') : t('admin.tags.add')}
          schema={schema}
          defaultValues={{
            name: editingItem?.name || '',
            isActive: editingItem?.isActive ?? true
          }}
          onSubmit={handleSubmit}
          onCancel={closeForm}
          isLoading={isCreating || isUpdating}
          fields={[
            {
              name: 'name',
              label: t('admin.tags.name'),
              type: 'text',
              required: true,
              placeholder: t('common.placeholders.enterName')
            },
            {
              name: 'isActive',
              label: t('admin.coupons.isActive'), // Close enough or add to tags
              type: 'switch'
            }
          ]}
      />
    </div>
  );
};

export default AdminTagsPage;
