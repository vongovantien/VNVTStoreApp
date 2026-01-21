import { useForm, Controller } from 'react-hook-form';
import { useTranslation } from 'react-i18next';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useDropzone } from 'react-dropzone';
import { Upload, X } from 'lucide-react';
import { Button, Input, Select, NumberInput, Switch } from '@/components/ui';
import { useState } from 'react';
import { useCategories, useSuppliers } from '@/hooks';
import { useToast } from '@/store';

const productSchemaBase = z.object({
  name: z.string(),
  categoryId: z.string(),
  price: z.number(),
  costPrice: z.number().optional(),
  stock: z.number().int(),
  description: z.string().optional(),
  brand: z.string().optional(),

  weight: z.number().optional(),
  supplierCode: z.string().optional(),
  images: z.array(z.string()).optional(),
  color: z.string().optional(),
  power: z.string().optional(),
  voltage: z.string().optional(),
  material: z.string().optional(),
  size: z.string().optional(),
  isActive: z.boolean().optional(),
});

export type ProductFormData = z.infer<typeof productSchemaBase>;

interface ProductFormProps {
  initialData?: ProductFormData & { id?: string; productImages?: { imageUrl: string, isPrimary: boolean }[] };
  onSubmit: (data: ProductFormData) => void;
  onCancel: () => void;
  isLoading?: boolean;
}

export const ProductForm = ({ initialData, onSubmit, onCancel, isLoading }: ProductFormProps) => {
  const { t } = useTranslation();
  const productSchema = z.object({
    name: z.string().min(3, t('admin.validation.productNameMin') || 'Tên sản phẩm phải có ít nhất 3 ký tự'),
    categoryId: z.string().min(1, t('admin.validation.categoryRequired') || 'Vui lòng chọn danh mục'),
    price: z.number().min(0, t('admin.validation.priceMin') || 'Giá không được âm'),
    costPrice: z.number().min(0, t('admin.validation.priceMin') || 'Giá vốn không được âm').optional(),
    stock: z.number().int().min(0, t('admin.validation.stockMin') || 'Tồn kho không được âm'),
    description: z.string().optional(),
    brand: z.string().optional(),

    weight: z.number().optional(),
    supplierCode: z.string().optional(),
    images: z.array(z.string()).optional(),
    color: z.string().optional(),
    power: z.string().optional(),
    voltage: z.string().optional(),
    material: z.string().optional(),
    size: z.string().optional(),
    isActive: z.boolean().optional(),
  });

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

      stock: 0,
      description: '',
      brand: '',

      weight: 0,
      supplierCode: '',
      color: '',
      power: '',
      voltage: '',
      material: '',
      size: '',
      isActive: true,
      ...initialData,
      // @ts-ignore
      costPrice: initialData?.costPrice ?? initialData?.originalPrice ?? 0,
      images: initialData?.images || initialData?.productImages?.map(img => img.imageUrl) || [],
    },
  });

  const [previewImages, setPreviewImages] = useState<string[]>(initialData?.images || initialData?.productImages?.map(img => img.imageUrl) || []);
  const [isUploading, setIsUploading] = useState(false);
  const { data: categories = [] } = useCategories();
  const { data: suppliers = [] } = useSuppliers();

  const categoryOptions = categories.map((c) => ({
    value: c.code,
    label: c.name,
  }));
  
  const supplierOptions = suppliers.map((s) => ({
    value: s.code,
    label: s.name,
  }));

  // Toast for errors
  const { toast } = useToast();

  const onDrop = async (acceptedFiles: File[]) => {
    if (acceptedFiles.length > 0) {
      setIsUploading(true);
      const currentImages = watch('images') || [];
      const newBase64Images: string[] = [];

      try {
        const promises = acceptedFiles.map((file) => {
          return new Promise<string>((resolve, reject) => {
            const reader = new FileReader();
            reader.onload = () => resolve(reader.result as string);
            reader.onerror = reject;
            reader.readAsDataURL(file);
          });
        });

        const results = await Promise.all(promises);
        newBase64Images.push(...results);

        const finalImages = [...currentImages, ...newBase64Images];
        setValue('images', finalImages, { shouldValidate: true });
        setPreviewImages(finalImages);
      } catch (error) {
        console.error("Error reading files", error);
        toast({
             title: t('messages.error'),
             description: "Failed to process images",
             variant: 'destructive',
        });
      } finally {
        setIsUploading(false);
      }
    }
  };

  const removeImage = (indexToRemove: number) => {
    const currentImages = watch('images') || [];
    const updatedImages = currentImages.filter((_, index) => index !== indexToRemove);
    setValue('images', updatedImages, { shouldValidate: true });
    setPreviewImages(updatedImages);
  };

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: { 'image/*': [] },
  });

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <div className="grid grid-cols-12 gap-8">
        <div className="col-span-12 md:col-span-4 space-y-4">
          <div className="space-y-2">
            <div className="flex justify-between items-center">
               <label className="text-sm font-semibold text-gray-700 dark:text-gray-300">{t('common.fields.image')}</label>
               <span className="text-xs text-gray-400">({previewImages.length} images)</span>
            </div>
            
            {previewImages.length > 0 && (
              <div className="grid grid-cols-3 gap-2 mb-2">
                {previewImages.map((imgUrl, index) => (
                  <div key={index} className="relative aspect-square group border rounded-lg overflow-hidden">
                    <img src={imgUrl} alt={`Product ${index}`} className="w-full h-full object-cover" />
                    <button
                      type="button"
                      onClick={() => removeImage(index)}
                      className="absolute top-1 right-1 p-1 bg-red-500 text-white rounded-full opacity-0 group-hover:opacity-100 transition-opacity"
                    >
                      <X size={12} />
                    </button>
                    {index === 0 && <span className="absolute bottom-0 left-0 right-0 bg-black/50 text-white text-[10px] text-center py-0.5">Primary</span>}
                  </div>
                ))}
              </div>
            )}

            <div
              {...getRootProps()}
              className={`border-2 border-dashed rounded-xl p-4 flex flex-col items-center justify-center cursor-pointer transition-colors aspect-square relative bg-gray-50 dark:bg-slate-900 ${isDragActive ? 'border-primary bg-primary/5' : 'border-gray-300 dark:border-gray-700 hover:border-primary'}`}
            >
              <input {...getInputProps()} />
              <div className="text-center p-4">
                 <div className="w-12 h-12 rounded-full bg-indigo-50 dark:bg-indigo-900/30 flex items-center justify-center mx-auto mb-3">
                   <Upload size={24} className="text-indigo-600 dark:text-indigo-400" />
                 </div>
                 <p className="text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                   {isUploading ? "Uploading..." : t('messages.dragDropImage')}
                 </p>
                 <p className="text-xs text-gray-500">
                   PNG, JPG, WEBP (Max 5MB)
                 </p>
               </div>
            </div>
            {errors.images && <p className="text-xs text-red-500">{errors.images.message}</p>}
          </div>
        </div>

        <div className="col-span-12 md:col-span-8 space-y-6">
          <div className="space-y-4">
            <h3 className="text-sm font-semibold text-gray-900 dark:text-gray-100 border-b pb-2">
              {t('common.info')}
            </h3>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="col-span-2">
                <Input
                  label={t('common.fields.name')}
                  {...register('name')}
                  error={errors.name?.message}
                  placeholder={t('common.placeholders.productName')}
                  isRequired
                />
              </div>

              <Controller
                name="categoryId"
                control={control}
                render={({ field }) => (
                  <Select
                    label={t('common.fields.category')}
                    options={categoryOptions}
                    value={field.value}
                    onChange={field.onChange}
                    error={errors.categoryId?.message}
                    placeholder={t('common.placeholders.selectCategory')}
                    isRequired
                  />
                )}
              />

              <Input
                label={t('common.fields.brand')}
                {...register('brand')}
                error={errors.brand?.message}
                placeholder={t('common.placeholders.brand')}
              />


              
              <Controller
                name="supplierCode"
                control={control}
                render={({ field }) => (
                  <Select
                    label={t('common.fields.supplier')}
                    options={supplierOptions}
                    value={field.value}
                    onChange={field.onChange}
                    error={errors.supplierCode?.message}
                    placeholder={t('common.placeholders.select')}
                  />
                )}
              />
            </div>
          </div>

          <div className="space-y-4">
            <h3 className="text-sm font-semibold text-gray-900 dark:text-gray-100 border-b pb-2">
              {t('common.fields.priceAndStock')}
            </h3>
            <div className="grid grid-cols-2 gap-4">
              <Controller
                name="price"
                control={control}
                render={({ field }) => (
                  <NumberInput
                    label={t('common.fields.price')}
                    value={field.value}
                    onChange={field.onChange}
                    error={errors.price?.message}
                    placeholder={t('common.placeholders.enterPrice')}
                    isRequired
                    min={0}
                  />
                )}
              />
              <Controller
                name="costPrice"
                control={control}
                render={({ field }) => (
                  <NumberInput
                    label={t('common.fields.costPrice')}
                    value={field.value}
                    onChange={field.onChange}
                    error={errors.costPrice?.message}
                    placeholder={t('common.placeholders.enterPrice')}
                    min={0}
                  />
                )}
              />
              <Controller
                name="stock"
                control={control}
                render={({ field }) => (
                  <NumberInput
                    label={t('common.fields.stock')}
                    value={field.value}
                    onChange={field.onChange}
                    error={errors.stock?.message}
                    placeholder={t('common.placeholders.enterQuantity')}
                    isRequired
                    min={0}
                  />
                )}
              />
               <Controller
                name="weight"
                control={control}
                render={({ field }) => (
                  <NumberInput
                    label={t('common.fields.weight')}
                    value={field.value}
                     onChange={field.onChange}
                    error={errors.weight?.message}
                    placeholder={t('common.placeholders.enterWeight')}
                    min={0}
                  />
                )}
              />
            </div>
          </div>

          <div className="space-y-4">
            <h3 className="text-sm font-semibold text-gray-900 dark:text-gray-100 border-b pb-2">
              {t('common.fields.specifications')}
            </h3>
            <div className="grid grid-cols-2 gap-4">
              <Input label={t('common.fields.color')} {...register('color')} placeholder={t('common.placeholders.exampleColor')} />
              <Input label={t('common.fields.material')} {...register('material')} placeholder={t('common.placeholders.exampleMaterial')} />
            </div>
            <div className="grid grid-cols-3 gap-4">
              <Input label={t('common.fields.power')} {...register('power')} placeholder={t('common.placeholders.examplePower')} />
              <Input label={t('common.fields.voltage')} {...register('voltage')} placeholder={t('common.placeholders.exampleVoltage')} />
              <Input label={t('common.fields.size')} {...register('size')} placeholder={t('common.placeholders.exampleSize')} />
            </div>
          </div>
        </div>

        {/* Status Toggle */}
        <div className="col-span-12">
          <div className="p-4 bg-gray-50 dark:bg-slate-800/50 rounded-lg">
            <Controller
              name="isActive"
              control={control}
              render={({ field }) => (
                <Switch
                  label={t('common.fields.status')}
                  description={t('admin.statusHint')}
                  checked={field.value ?? true}
                  onChange={field.onChange}
                />
              )}
            />
          </div>
        </div>

        <div className="col-span-12 space-y-2">
          <label className="text-sm font-semibold text-gray-700 dark:text-gray-300">
            {t('common.fields.description')}
          </label>
          <textarea
            className="w-full min-h-[100px] px-3 py-2 border border-gray-300 dark:border-gray-700 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 bg-white dark:bg-slate-900 transition-all resize-y text-sm"
            placeholder={t('common.placeholders.productDescription')}
            {...register('description')}
          />
        </div>
      </div>

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
