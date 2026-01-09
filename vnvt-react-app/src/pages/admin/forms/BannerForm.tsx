
import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Button, Input, Select } from '@/components/ui';

export interface BannerFormData {
  title: string;
  content: string;
  linkUrl: string;
  linkText: string;
  priority: number;
  isActive: boolean;
}

interface BannerFormProps {
  initialData?: BannerFormData;
  onSubmit: (data: BannerFormData) => void;
  onCancel: () => void;
  isLoading?: boolean;
}

export const BannerForm = ({
  initialData,
  onSubmit,
  onCancel,
  isLoading,
}: BannerFormProps) => {
  const { t } = useTranslation();
  const [formData, setFormData] = useState<BannerFormData>({
    title: '',
    content: '',
    linkUrl: '',
    linkText: 'Learn More',
    priority: 0,
    isActive: true,
  });

  useEffect(() => {
    if (initialData) {
      setFormData(initialData);
    }
  }, [initialData]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit(formData);
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div>
        <label className="block text-sm font-medium mb-1">Title *</label>
        <Input
          value={formData.title}
          onChange={(e) => setFormData({ ...formData, title: e.target.value })}
          required
          placeholder="Enter banner title"
        />
      </div>

      <div>
        <label className="block text-sm font-medium mb-1">Content</label>
        <Input
          value={formData.content}
          onChange={(e) => setFormData({ ...formData, content: e.target.value })}
          placeholder="Enter banner content description"
        />
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium mb-1">Link URL</label>
          <Input
            value={formData.linkUrl}
            onChange={(e) => setFormData({ ...formData, linkUrl: e.target.value })}
            placeholder="/shop/category"
          />
        </div>
        <div>
          <label className="block text-sm font-medium mb-1">Link Text</label>
          <Input
            value={formData.linkText}
            onChange={(e) => setFormData({ ...formData, linkText: e.target.value })}
            placeholder="Learn More"
          />
        </div>
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div>
           <label className="block text-sm font-medium mb-1">Priority</label>
           <Input
             type="number"
             value={formData.priority}
             onChange={(e) => setFormData({ ...formData, priority: parseInt(e.target.value) || 0 })}
           />
        </div>
        <div className="flex items-center pt-6">
            <input
              type="checkbox"
              id="isActive"
              checked={formData.isActive}
              onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
              className="w-4 h-4 text-primary border-gray-300 rounded focus:ring-primary"
            />
            <label htmlFor="isActive" className="ml-2 text-sm font-medium text-gray-700">
              Active
            </label>
        </div>
      </div>

      <div className="flex justify-end gap-2 mt-6">
        <Button
          type="button"
          variant="outline"
          onClick={onCancel}
          disabled={isLoading}
        >
          {t('common.cancel')}
        </Button>
        <Button type="submit" isLoading={isLoading}>
          {initialData ? t('common.save') : t('common.create')}
        </Button>
      </div>
    </form>
  );
};
