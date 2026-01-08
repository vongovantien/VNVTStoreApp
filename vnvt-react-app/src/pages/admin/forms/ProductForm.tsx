import { useForm, Controller } from 'react-hook-form';
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
  brand: z.string().optional(), // Optional since backend doesn't have this
  image: z.string().optional(), // Optional since backend handles images separately
});

export type ProductFormData = z.infer<typeof productSchema>;

interface ProductFormProps {
  initialData?: ProductFormData & { id?: string };
  onSubmit: (data: ProductFormData) => void;
  onCancel: () => void;
  isLoading?: boolean;
}

export const ProductForm = ({ initialData, onSubmit, onCancel, isLoading }: ProductFormProps) => {
  const {
    register,
    control,
    handleSubmit,
    setValue,
    watch,
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
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {/* Left Column: Image Upload */}
        <div className="space-y-4">
          <label className="block text-sm font-medium text-secondary">Ảnh sản phẩm</label>
          <div
            {...getRootProps()}
            className={`border-2 border-dashed rounded-xl p-6 flex flex-col items-center justify-center cursor-pointer transition-colors h-64 ${isDragActive ? 'border-primary bg-primary/5' : 'border-tertiary hover:border-primary'
              }`}
          >
            <input {...getInputProps()} />
            {previewImage ? (
              <div className="relative w-full h-full group">
                <img
                  src={previewImage}
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
                  className="absolute top-2 right-2 p-1 bg-error text-white rounded-full opacity-0 group-hover:opacity-100 transition-opacity"
                >
                  <X size={16} />
                </button>
              </div>
            ) : (
              <>
                <Upload size={48} className="text-tertiary mb-4" />
                <p className="text-center text-sm text-secondary">
                  Kéo thả ảnh vào đây, hoặc click để chọn
                </p>
                <p className="text-xs text-tertiary mt-2">PNG, JPG, WEBP (Tối đa 5MB)</p>
              </>
            )}
          </div>
          {errors.image && <p className="text-xs text-error">{errors.image.message}</p>}
        </div>

        {/* Right Column: Basic Info */}
        <div className="space-y-4">
          <Input
            label="Tên sản phẩm"
            {...register('name')}
            error={errors.name?.message}
            placeholder="Nhập tên sản phẩm"
          />

          <Controller
            name="categoryId"
            control={control}
            render={({ field }) => (
              <Select
                label="Danh mục"
                options={categoryOptions}
                value={field.value}
                onChange={field.onChange}
                error={errors.categoryId?.message}
                placeholder="Chọn danh mục"
              />
            )}
          />

          <div className="grid grid-cols-2 gap-4">
            <Input
              label="Thương hiệu"
              {...register('brand')}
              error={errors.brand?.message}
              placeholder="Samsung, LG..."
            />
            <Input
              label="Tồn kho"
              type="number"
              {...register('stock', { valueAsNumber: true })}
              error={errors.stock?.message}
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <Input
              label="Giá bán"
              type="number"
              {...register('price', { valueAsNumber: true })}
              error={errors.price?.message}
            />
            <Input
              label="Giá gốc"
              type="number"
              {...register('originalPrice', { valueAsNumber: true })}
              error={errors.originalPrice?.message}
            />
          </div>
        </div>
      </div>

      <div className="space-y-2">
        <label className="block text-sm font-medium text-secondary">Mô tả chi tiết</label>
        <textarea
          className="w-full min-h-[120px] px-3 py-2 border rounded-lg focus:outline-none focus:border-primary bg-transparent"
          placeholder="Mô tả đặc điểm nổi bật của sản phẩm..."
          {...register('description')}
        />
      </div>

      <div className="flex justify-end gap-3 pt-4 border-t">
        <Button type="button" variant="outline" onClick={onCancel}>
          Hủy bỏ
        </Button>
        <Button type="submit" isLoading={isLoading}>
          {initialData ? 'Cập nhật' : 'Thêm mới'}
        </Button>
      </div>
    </form>
  );
};
