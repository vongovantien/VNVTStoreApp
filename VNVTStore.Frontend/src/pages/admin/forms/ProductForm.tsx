import { useForm, Controller } from 'react-hook-form';
import { useTranslation } from 'react-i18next';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useDropzone } from 'react-dropzone';
import { Upload, X } from 'lucide-react';
import { Button, Input, Select, NumberInput, Switch, Badge } from '@/components/ui';
import { useState } from 'react';
import { useCategories, useSuppliers } from '@/hooks';
import { useToast } from '@/store';

const productSchemaBase = z.object({
  name: z.string(),
  categoryCode: z.string(), // categoryId -> categoryCode
  price: z.number(),
  wholesalePrice: z.number().optional(), // New
  costPrice: z.number().optional(),
  stockQuantity: z.number().int(), // stock -> stockQuantity
  description: z.string().optional(),
  brandCode: z.string().optional(), // brand -> brandCode
  baseUnit: z.string().optional(), // New
  minStockLevel: z.number().optional(), // New
  binLocation: z.string().optional(), // New
  vatRate: z.number().optional(), // New
  countryOfOrigin: z.string().optional(), // New
  supplierCode: z.string().optional(),
  images: z.array(z.string()).optional(),
  isActive: z.boolean().optional(),
  // Dynamic fields mapped to top level for convenience/legacy
  weight: z.number().optional(),
  color: z.string().optional(),
  material: z.string().optional(),
  size: z.string().optional(),
  power: z.string().optional(),
  voltage: z.string().optional(),
  details: z.array(z.object({
    detailType: z.enum(['SPEC', 'LOGISTICS', 'RELATION', 'IMAGE']),
    specName: z.string(),
    specValue: z.string()
  })).optional(),
});

export type ProductFormData = z.infer<typeof productSchemaBase>;

interface ProductFormProps {
  initialData?: any; // Simpler for now as we transition
  onSubmit: (data: ProductFormData) => void;
  onCancel: () => void;
  isLoading?: boolean;
}

export const ProductForm = ({ initialData, onSubmit, onCancel, isLoading }: ProductFormProps) => {
  const { t } = useTranslation();
  const productSchema = z.object({
    name: z.string().min(3, t('admin.validation.productNameMin') || 'Tên sản phẩm phải có ít nhất 3 ký tự'),
    categoryCode: z.string().min(1, t('admin.validation.categoryRequired') || 'Vui lòng chọn danh mục'),
    price: z.number().min(0, t('admin.validation.priceMin') || 'Giá không được âm'),
    wholesalePrice: z.number().min(0).optional(),
    costPrice: z.number().min(0).optional(),
    stockQuantity: z.number().int().min(0),
    description: z.string().optional(),
    brandCode: z.string().optional(),
    baseUnit: z.string().optional(),
    minStockLevel: z.number().optional(),
    binLocation: z.string().optional(),
    vatRate: z.number().optional(),
    countryOfOrigin: z.string().optional(),
    supplierCode: z.string().optional(),
    images: z.array(z.string()).optional(),
    isActive: z.boolean().optional(),
    details: z.array(z.object({
        detailType: z.enum(['SPEC', 'LOGISTICS', 'RELATION', 'IMAGE']),
        specName: z.string().min(1, "Tên không được để trống"),
        specValue: z.string().min(1, "Giá trị không được để trống")
    })).optional(),
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
      price: 0,
      wholesalePrice: 0,
      description: '',
      brandCode: '',
      baseUnit: 'Cái',
      isActive: true,
      details: [],
      ...initialData,
      categoryCode: initialData?.categoryCode || initialData?.categoryId || '',
      stockQuantity: initialData?.stockQuantity || initialData?.stock || 0,
      costPrice: initialData?.costPrice ?? initialData?.originalPrice ?? 0,
      images: initialData?.images || initialData?.productImages?.map((img: any) => img.imageUrl) || [],
    },
  });

  const [activeTab, setActiveTab] = useState<'basic' | 'specs' | 'tags'>('basic');
  const details = watch('details') || [];
  
  const addDetail = (type: 'SPEC' | 'LOGISTICS' | 'RELATION') => {
    setValue('details', [...details, { detailType: type, specName: '', specValue: '' }]);
  };

  const removeDetail = (index: number) => {
    setValue('details', details.filter((_, i) => i !== index));
  };

  const updateDetail = (index: number, field: 'specName' | 'specValue', value: string) => {
    const newDetails = [...details];
    newDetails[index][field] = value;
    setValue('details', newDetails);
  };

  // Suggestions for Spec Names
  const specSuggestions = ["Công suất", "Điện áp", "Lưu lượng", "Phi (Ø)", "Đường kính", "Độ dày", "Chất liệu", "Màu sắc"];

  const [previewImages, setPreviewImages] = useState<string[]>(initialData?.images || initialData?.productImages?.map((img: any) => img.imageUrl) || []);
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

  const { success: toastSuccess, error: toastError } = useToast();

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
        toastError("Không thể xử lý hình ảnh");
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
    <div className="bg-white dark:bg-slate-900 rounded-2xl shadow-sm border border-slate-100 dark:border-slate-800 overflow-hidden">
      {/* Tab Header */}
      <div className="flex border-b border-slate-100 dark:border-slate-800 bg-slate-50/50 dark:bg-slate-900">
        {[
          { id: 'basic', label: 'Thông tin cơ bản' },
          { id: 'specs', label: 'Thông số kỹ thuật' },
          { id: 'tags', label: 'Tags & Quan hệ' }
        ].map(tab => (
          <button
            key={tab.id}
            type="button"
            onClick={() => setActiveTab(tab.id as any)}
            className={`px-6 py-4 text-sm font-semibold transition-all border-b-2 ${
              activeTab === tab.id 
                ? 'border-indigo-500 text-indigo-600 bg-white dark:bg-slate-800' 
                : 'border-transparent text-slate-500 hover:text-slate-700 hover:bg-slate-100'
            }`}
          >
            {tab.label}
          </button>
        ))}
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="p-6 space-y-6">
        {activeTab === 'basic' && (
          <div className="grid grid-cols-12 gap-8 animate-in fade-in duration-300">
            {/* Image Upload Area */}
            <div className="col-span-12 md:col-span-4 space-y-4">
              <div className="bg-slate-50 dark:bg-slate-800/50 p-4 rounded-2xl border border-slate-100 dark:border-slate-800">
                  <label className="block text-sm font-bold text-slate-700 dark:text-slate-300 mb-3">Hình ảnh sản phẩm</label>
                  <div
                    {...getRootProps()}
                    className={`border-2 border-dashed rounded-xl p-6 flex flex-col items-center justify-center cursor-pointer transition-all aspect-square relative bg-white dark:bg-slate-900 ${
                      isDragActive ? 'border-blue-500 bg-blue-50/10' : 'border-slate-200 dark:border-slate-700 hover:border-blue-500 hover:bg-blue-50/5'
                    }`}
                  >
                    <input {...getInputProps()} />
                    <div className="text-center p-4">
                      <div className="w-12 h-12 bg-blue-50 text-blue-600 rounded-full flex items-center justify-center mx-auto mb-3">
                         <Upload size={20} />
                      </div>
                      <p className="text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">
                        Kéo thả hoặc click để tải ảnh
                      </p>
                      <p className="text-xs text-slate-400">
                        (Khuyên dùng ảnh tỉ lệ 1:1, tối đa 5MB)
                      </p>
                    </div>
                  </div>
                  
                  {previewImages.length > 0 && (
                      <div className="mt-4 grid grid-cols-4 gap-2">
                        {previewImages.map((img, idx) => (
                          <div key={idx} className="relative aspect-square rounded-lg border border-slate-200 dark:border-slate-700 overflow-hidden group bg-white">
                            <img src={img} className="w-full h-full object-cover" />
                            <button 
                                type="button" 
                                onClick={() => removeImage(idx)}
                                className="absolute inset-0 bg-black/40 text-white opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center backdrop-blur-sm"
                            >
                                <X size={16} />
                            </button>
                          </div>
                        ))}
                      </div>
                  )}
              </div>
            </div>

            {/* Basic Info Fields */}
            <div className="col-span-12 md:col-span-8 space-y-6">
               
               {/* Section 1: General Information */}
               <div className="bg-white dark:bg-slate-900 p-0 space-y-4">
                   <h3 className="text-base font-semibold text-slate-800 dark:text-white flex items-center gap-2">
                       <span className="w-1 h-5 bg-blue-600 rounded-full"></span>
                       Thông tin chung
                   </h3>
                   <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                       <div className="col-span-2">
                          <Input label="Tên sản phẩm" {...register('name')} error={errors.name?.message} isRequired placeholder="Nhập tên sản phẩm đầy đủ..." />
                       </div>
                       <Controller
                        name="categoryCode"
                        control={control}
                        render={({ field }) => (
                          <Select label="Danh mục" options={categoryOptions} {...field} error={errors.categoryCode?.message} isRequired />
                        )}
                      />
                      <Input label="Thương hiệu" {...register('brandCode')} placeholder="VD: Samsung, LG..." />
                   </div>
               </div>

               <hr className="border-dashed border-slate-200 dark:border-slate-700" />

               {/* Section 2: Pricing */}
               <div className="space-y-4">
                   <h3 className="text-base font-semibold text-slate-800 dark:text-white flex items-center gap-2">
                       <span className="w-1 h-5 bg-green-500 rounded-full"></span>
                       Giá sách
                   </h3>
                   <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                     <Controller
                        name="price"
                        control={control}
                        render={({ field }) => <NumberInput label="Giá bán lẻ" {...field} error={errors.price?.message} isRequired className="text-lg font-semibold text-blue-600" />}
                     />
                     <Controller
                        name="wholesalePrice"
                        control={control}
                        render={({ field }) => <NumberInput label="Giá bán sỉ" {...field} />}
                     />
                     <Controller
                        name="costPrice"
                        control={control}
                        render={({ field }) => <NumberInput label="Giá vốn" {...field} />}
                     />
                   </div>
               </div>

               <hr className="border-dashed border-slate-200 dark:border-slate-700" />

               {/* Section 3: Inventory */}
               <div className="space-y-4">
                   <h3 className="text-base font-semibold text-slate-800 dark:text-white flex items-center gap-2">
                       <span className="w-1 h-5 bg-amber-500 rounded-full"></span>
                       Kho hàng & Quy cách
                   </h3>
                   <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
                      <div className="col-span-2">
                          <Controller
                            name="stockQuantity"
                            control={control}
                            render={({ field }) => <NumberInput label="Số lượng tồn kho" {...field} isRequired />}
                          />
                      </div>
                      <div className="col-span-2">
                          <Input label="Đơn vị tính" {...register('baseUnit')} placeholder="VD: Cái, Hộp..." />
                      </div>
                      
                      <Controller
                        name="minStockLevel"
                        control={control}
                        render={({ field }) => <NumberInput label="Tồn tối thiểu" {...field} placeholder="0" />}
                      />
                      <Input label="Vị trí kho" {...register('binLocation')} placeholder="VD: Kệ A1..." />
                      <div className="col-span-2">
                          <Input label="Xuất xứ" {...register('countryOfOrigin')} placeholder="VD: Việt Nam, Trung Quốc..." />
                      </div>
                   </div>
               </div>

            </div>
          </div>
        )}

        {activeTab === 'specs' && (
          <div className="space-y-6 animate-in slide-in-from-right-2 duration-300">
            <div className="flex justify-between items-center bg-indigo-50 dark:bg-indigo-900/20 p-4 rounded-xl">
                <div className="text-sm">
                    <p className="font-bold text-indigo-700 dark:text-indigo-400">Thông số kĩ thuật & Logistics</p>
                    <p className="text-xs text-indigo-600 opacity-70">Thêm các thông tin động để hiển thị trên web.</p>
                </div>
                <div className="flex gap-2">
                    <Button type="button" size="sm" onClick={() => addDetail('SPEC')} className="bg-white border-2 border-indigo-500 text-indigo-600 hover:bg-indigo-50">
                        + Thông số (SPEC)
                    </Button>
                    <Button type="button" size="sm" onClick={() => addDetail('LOGISTICS')} className="bg-white border-2 border-indigo-500 text-indigo-600 hover:bg-indigo-50">
                        + Logistics
                    </Button>
                </div>
            </div>

            <div className="space-y-3">
                {details.map((detail, idx) => (
                    <div key={idx} className="group flex items-center gap-3 p-3 bg-slate-50 dark:bg-slate-800/40 rounded-xl border border-transparent hover:border-indigo-100 transition-all">
                        <Badge color={detail.detailType === 'SPEC' ? 'primary' : 'success'} className="w-24 text-center">
                            {detail.detailType}
                        </Badge>
                        
                        <div className="flex-1 grid grid-cols-2 gap-4">
                            <div className="relative">
                                <input 
                                    className="w-full text-sm bg-transparent border-b border-slate-200 focus:border-indigo-500 p-1 outline-none"
                                    placeholder="Tên (VD: Công suất)"
                                    value={detail.specName}
                                    onChange={(e) => updateDetail(idx, 'specName', e.target.value)}
                                    list={`spec-suggestions-${idx}`}
                                />
                                <datalist id={`spec-suggestions-${idx}`}>
                                    {specSuggestions.map(s => <option key={s} value={s} />)}
                                </datalist>
                            </div>
                            <input 
                                className="w-full text-sm bg-transparent border-b border-slate-200 focus:border-indigo-500 p-1 outline-none"
                                placeholder="Giá trị (VD: 20W)"
                                value={detail.specValue}
                                onChange={(e) => updateDetail(idx, 'specValue', e.target.value)}
                            />
                        </div>

                        <button 
                            type="button" 
                            onClick={() => removeDetail(idx)}
                            className="text-slate-300 hover:text-red-500 opacity-0 group-hover:opacity-100 transition-all p-1"
                        >
                            <X size={18} />
                        </button>
                    </div>
                ))}

                {details.length === 0 && (
                    <div className="text-center py-12 border-2 border-dashed border-slate-100 rounded-2xl">
                        <p className="text-slate-400 text-sm">Chưa có thông số chi tiết nào được thêm.</p>
                    </div>
                )}
            </div>
          </div>
        )}

        {activeTab === 'tags' && (
          <div className="space-y-6 animate-in slide-in-from-right-4 duration-300 min-h-[400px]">
             <div className="space-y-4">
                <label className="text-sm font-bold text-slate-700">Thẻ tìm kiếm & Sản phẩm liên quan</label>
                <div className="p-4 bg-slate-50 dark:bg-slate-800 rounded-2xl border border-slate-100">
                    <p className="text-xs text-slate-500 mb-4">Nhập tên các sản phẩm phụ kiện hoặc cùng bộ để tạo quan hệ (RELATION).</p>
                    <div className="flex flex-wrap gap-2 mb-4">
                        {details.filter(d => d.detailType === 'RELATION').map((rel, idx) => (
                             <Badge key={idx} className="bg-indigo-500 py-1.5 px-3 rounded-lg flex items-center gap-2">
                                {rel.specValue}
                                <X size={14} className="cursor-pointer" onClick={() => removeDetail(details.indexOf(rel))} />
                             </Badge>
                        ))}
                    </div>
                    <Input 
                        placeholder="Nhập tên sản phẩm liên quan và nhấn Enter..." 
                        onKeyDown={(e) => {
                            if (e.key === 'Enter') {
                                e.preventDefault();
                                const val = (e.target as HTMLInputElement).value;
                                if (val) {
                                    setValue('details', [...details, { detailType: 'RELATION', specName: 'RelatedTo', specValue: val }]);
                                    (e.target as HTMLInputElement).value = '';
                                }
                            }
                        }}
                    />
                </div>
             </div>

             <div className="space-y-2">
                <label className="text-sm font-bold text-slate-700">Mô tả chi tiết</label>
                <textarea
                    className="w-full min-h-[200px] px-4 py-3 border border-slate-200 dark:border-slate-800 rounded-2xl focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 bg-white dark:bg-slate-900 transition-all text-sm outline-none"
                    placeholder="Nhập mô tả sản phẩm tại đây..."
                    {...register('description')}
                />
             </div>
          </div>
        )}

        <div className="flex justify-end gap-3 pt-6 border-t border-slate-100 dark:border-slate-800 bg-slate-50/50 -mx-6 -mb-6 p-6">
          <Button type="button" variant="ghost" onClick={onCancel} className="text-slate-500">
            {t('common.cancel')}
          </Button>
          <Button type="submit" isLoading={isLoading} className="bg-indigo-600 hover:bg-indigo-700 px-8">
            {initialData ? 'Lưu thay đổi' : 'Tạo sản phẩm mới'}
          </Button>
        </div>
      </form>
    </div>
  );
};
