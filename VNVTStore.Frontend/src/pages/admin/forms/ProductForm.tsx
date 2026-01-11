import { useForm, Controller } from 'react-hook-form';
import { useTranslation } from 'react-i18next';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useDropzone } from 'react-dropzone';
import { Upload, X } from 'lucide-react';
import { Button, Input, Select } from '@/components/ui';
import { useState } from 'react';
import { useCategories } from '@/hooks';

// Schema Definition
const productSchema = z.object({
  name: z.string().min(3, 'Tên sản phẩm phải có ít nhất 3 ký tự'),
  categoryId: z.string().min(1, 'Vui lòng chọn danh mục'),
  price: z.number().min(0, 'Giá không được âm'),
  originalPrice: z.number().min(0, 'Giá gốc không được âm').optional(),
  stock: z.number().int().min(0, 'Tồn kho không được âm'),
  description: z.string().optional(),
  brand: z.string().optional(),
  image: z.string().optional(),
  color: z.string().optional(),
  power: z.string().optional(),
  voltage: z.string().optional(),
  material: z.string().optional(),
  size: z.string().optional(),
});

export type ProductFormData = z.infer<typeof productSchema>;

interface ProductFormProps {
  initialData?: ProductFormData & { id?: string };
  onSubmit: (data: ProductFormData) => void;
  onCancel: () => void;
  isLoading?: boolean;
}

export const ProductForm = ({ initialData, onSubmit, onCancel, isLoading }: ProductFormProps) => {
  const { t } = useTranslation();
  const {
    register,
    control,
    handleSubmit,
    setValue,
    formState: { errors },
  } = useForm<ProductFormData>({
    resolver: zodResolver(productSchema),
    defaultValues: {
      name: '',
      categoryId: '',
      price: 0,
      originalPrice: 0,
      stock: 0,
      description: '',
      brand: '',
      image: '',
      color: '',
      power: '',
      voltage: '',
      material: '',
      size: '',
      ...initialData,
    },
  });

  const [previewImage, setPreviewImage] = useState<string | null>(initialData?.image || null);
  const { data: categories = [] } = useCategories();

  const categoryOptions = categories.map((c) => ({
    value: c.code,
    label: c.name,
  }));

  const onDrop = (acceptedFiles: File[]) => {
    const file = acceptedFiles[0];
    if (file) {
      // Create a fake URL for preview (in real app, upload to Cloudinary here)
      const url = URL.createObjectURL(file);
      setPreviewImage(url);
      setValue('image', url, { shouldValidate: true });
    }
  };

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: { 'image/*': [] },
    maxFiles: 1,
  });

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <div className="grid grid-cols-12 gap-8">
        {/* Left Column: Image Upload & Preview */}
        <div className="col-span-12 md:col-span-4 space-y-4">
          <div className="space-y-2">
            <label className="text-sm font-semibold text-gray-700 dark:text-gray-300">{t('admin.columns.image')}</label>
            <div
              {...getRootProps()}
              className={`border-2 border-dashed rounded-xl p-4 flex flex-col items-center justify-center cursor-pointer transition-colors aspect-square relative bg-gray-50 dark:bg-slate-900 ${isDragActive ? 'border-primary bg-primary/5' : 'border-gray-300 dark:border-gray-700 hover:border-primary'
                }`}
            >
              <input {...getInputProps()} />
              {previewImage ? (
                <div className="relative w-full h-full group">
                  <img
                    src={previewImage || undefined}
                    alt="Preview"
                    className="w-full h-full object-contain rounded-lg"
                  />
                  <button
                    type="button"
                    onClick={(e) => {
                      e.stopPropagation();
                      setPreviewImage(null);
                      setValue('image', '');
                    }}
                    className="absolute top-2 right-2 p-1.5 bg-red-500 text-white rounded-full opacity-0 group-hover:opacity-100 transition-opacity shadow-sm hover:bg-red-600"
                  >
                    <X size={16} />
                  </button>
                </div>
              ) : (
                <div className="text-center p-4">
                  <div className="w-12 h-12 rounded-full bg-indigo-50 dark:bg-indigo-900/30 flex items-center justify-center mx-auto mb-3">
                    <Upload size={24} className="text-indigo-600 dark:text-indigo-400" />
                  </div>
                  <p className="text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                    {t('messages.dragDropImage') || 'Tải ảnh lên'}
                  </p>
                  <p className="text-xs text-gray-500">
                    PNG, JPG, WEBP (Max 5MB)
                  </p>
                </div>
              )}
            </div>
            {errors.image && <p className="text-xs text-red-500">{errors.image.message}</p>}
          </div>

          {/* Description moved to bottom of left col for better balance if needed, or keep at bottom of main form. 
              Let's put Description here if it's short, or keep it wide. 
              Actually, let's keep Description at the bottom full width, but put Brand here perhaps? 
              No, let's stick to the plan: Image on left, fields on right.
          */}
        </div>

        {/* Right Column: Form Fields */}
        <div className="col-span-12 md:col-span-8 space-y-6">
          {/* Section 1: General Info */}
          <div className="space-y-4">
            <h3 className="text-sm font-semibold text-gray-900 dark:text-gray-100 border-b pb-2">
              {t('common.info')}
            </h3>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="col-span-2">
                <Input
                  label={t('admin.columns.name')}
                  {...register('name')}
                  error={errors.name?.message}
                  placeholder="Nhập tên sản phẩm..."
                  isRequired
                />
              </div>

              <Controller
                name="categoryId"
                control={control}
                render={({ field }) => (
                  <Select
                    label={t('admin.columns.category')}
                    options={categoryOptions}
                    value={field.value}
                    onChange={field.onChange}
                    error={errors.categoryId?.message}
                    placeholder="Chọn danh mục"
                    isRequired
                  />
                )}
              />

              <Input
                label={t('admin.columns.brand')}
                {...register('brand')}
                error={errors.brand?.message}
                placeholder="Thương hiệu"
              />
            </div>
          </div>

          {/* Section 2: Pricing & Inventory */}
          <div className="space-y-4">
            <h3 className="text-sm font-semibold text-gray-900 dark:text-gray-100 border-b pb-2">
              {t('common.priceAndStock')}
            </h3>
            <div className="grid grid-cols-3 gap-4">
              <Input
                label={t('admin.columns.price')}
                type="number"
                {...register('price', { valueAsNumber: true })}
                error={errors.price?.message}
                placeholder="0"
                isRequired
              />
              <Input
                label={t('admin.columns.originalPrice')}
                type="number"
                {...register('originalPrice', { valueAsNumber: true })}
                error={errors.originalPrice?.message}
                placeholder="0"
              />
              <Input
                label={t('admin.columns.stock')}
                type="number"
                {...register('stock', { valueAsNumber: true })}
                error={errors.stock?.message}
                placeholder="0"
                isRequired
              />
            </div>
          </div>

          {/* Section 3: Specifications (New Attributes) */}
          <div className="space-y-4">
            <h3 className="text-sm font-semibold text-gray-900 dark:text-gray-100 border-b pb-2">
              {t('common.specifications')}
            </h3>
            <div className="grid grid-cols-2 gap-4">
              <Input label={t('admin.columns.color')} {...register('color')} placeholder="VD: Trắng..." />
              <Input label={t('admin.columns.material')} {...register('material')} placeholder="VD: Nhựa PVC..." />
            </div>
            <div className="grid grid-cols-3 gap-4">
              <Input label={t('admin.columns.power')} {...register('power')} placeholder="VD: 50W..." />
              <Input label={t('admin.columns.voltage')} {...register('voltage')} placeholder="VD: 220V..." />
              <Input label={t('admin.columns.size')} {...register('size')} placeholder="VD: Ø20mm..." />
            </div>
          </div>
        </div>

        {/* Footer: Description (Full Width) */}
        <div className="col-span-12 space-y-2">
          <label className="text-sm font-semibold text-gray-700 dark:text-gray-300">
            {t('admin.columns.description')}
          </label>
          <textarea
            className="w-full min-h-[100px] px-3 py-2 border border-gray-300 dark:border-gray-700 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 bg-white dark:bg-slate-900 transition-all resize-y text-sm"
            placeholder="Mô tả chi tiết sản phẩm..."
            {...register('description')}
          />
        </div>
      </div>

      {/* Actions */}
      <div className="flex justify-end gap-3 pt-6 border-t mt-4">
        <Button type="button" variant="outline" onClick={onCancel}>
          {t('common.cancel')}
        </Button>
        <Button type="submit" isLoading={isLoading}>
          {initialData ? t('common.update') : t('common.create')}
        </Button>
      </div>
    </form>
  );
};
