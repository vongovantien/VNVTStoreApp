import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Upload, X } from 'lucide-react';
import { useDropzone } from 'react-dropzone';
import { Button, Input } from '@/components/ui';

export interface BannerFormData {
  title: string;
  content: string;
  linkUrl: string;
  linkText: string;
  imageUrl?: string;
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
    imageUrl: '',
    priority: 0,
    isActive: true,
  });

  const [previewImage, setPreviewImage] = useState<string | null>(null);
  const [isUploading, setIsUploading] = useState(false);

  useEffect(() => {
    if (initialData) {
      setFormData({
        ...initialData,
        imageUrl: initialData.imageUrl || ''
      });
      if (initialData.imageUrl) {
         setPreviewImage(initialData.imageUrl.startsWith('data:') ? initialData.imageUrl : 
            `${import.meta.env.VITE_API_URL || 'http://localhost:5176/api/v1'}/${initialData.imageUrl}`.replace('/api/v1//', '/api/v1/').replace('v1/uploads', 'uploads')); 
            // Simple heuristic for preview, relying on standard backend static file serving
      }
    }
  }, [initialData]);

  const onDrop = async (acceptedFiles: File[]) => {
    if (acceptedFiles.length > 0) {
      const file = acceptedFiles[0];
      const reader = new FileReader();
      reader.onload = () => {
        const base64 = reader.result as string;
        setFormData(prev => ({ ...prev, imageUrl: base64 }));
        setPreviewImage(base64);
      };
      reader.readAsDataURL(file);
    }
  };

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: { 'image/*': [] },
    multiple: false
  });

  const removeImage = () => {
    setFormData(prev => ({ ...prev, imageUrl: '' }));
    setPreviewImage(null);
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit(formData);
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      {/* Image Upload Section */}
      <div>
        <label className="block text-sm font-medium mb-1">{t('common.fields.image')}</label>
         <div className="flex gap-4 items-start">
            {previewImage && (
              <div className="relative w-24 h-24 rounded-lg overflow-hidden border border-gray-200 group">
                <img src={previewImage} alt="Preview" className="w-full h-full object-cover" />
                <button
                  type="button"
                  onClick={removeImage}
                  className="absolute top-1 right-1 p-1 bg-red-500 text-white rounded-full opacity-0 group-hover:opacity-100 transition-opacity"
                >
                  <X size={12} />
                </button>
              </div>
            )}
            
            <div
              {...getRootProps()}
              className={`flex-1 border-2 border-dashed rounded-lg p-4 flex flex-col items-center justify-center cursor-pointer transition-colors h-24 ${isDragActive ? 'border-primary bg-primary/5' : 'border-gray-300 hover:border-primary'}`}
            >
              <input {...getInputProps()} />
              <div className="text-center">
                 <Upload size={20} className="text-gray-400 mx-auto mb-1" />
                 <p className="text-xs text-gray-500">
                   {isDragActive ? "Drop here" : t('messages.dragDropImage')}
                 </p>
               </div>
            </div>
         </div>
      </div>

      <div>
        <label className="block text-sm font-medium mb-1">{t('common.fields.title')} *</label>
        <Input
          value={formData.title}
          onChange={(e) => setFormData({ ...formData, title: e.target.value })}
          required
          placeholder={t('common.placeholders.enterTitle')}
        />
      </div>
{/* ... rest of form ... */}

      <div>
        <label className="block text-sm font-medium mb-1">{t('common.fields.content')}</label>
        <Input
          value={formData.content}
          onChange={(e) => setFormData({ ...formData, content: e.target.value })}
          placeholder={t('common.placeholders.enterContent')}
        />
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium mb-1">{t('common.fields.linkUrl')}</label>
          <Input
            value={formData.linkUrl}
            onChange={(e) => setFormData({ ...formData, linkUrl: e.target.value })}
            placeholder={t('common.placeholders.enterLinkUrl')}
          />
        </div>
        <div>
          <label className="block text-sm font-medium mb-1">{t('common.fields.linkText')}</label>
          <Input
            value={formData.linkText}
            onChange={(e) => setFormData({ ...formData, linkText: e.target.value })}
            placeholder={t('common.placeholders.enterLinkText')}
          />
        </div>
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div>
           <label className="block text-sm font-medium mb-1">{t('common.fields.priority')}</label>
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
              {t('common.status.active')}
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
