import { useDropzone } from 'react-dropzone';
import { Upload, X } from 'lucide-react';
import { UseFormReturn } from 'react-hook-form';
import { ProductFormData } from './ProductForm';

interface ImageUploadFieldProps {
    form: UseFormReturn<ProductFormData>;
    t: (key: string) => string;
    isUploading: boolean;
    setIsUploading: (val: boolean) => void;
    setPreviewImage: (val: string | null) => void;
}

export const ImageUploadField = ({ 
    form, 
    t, 
    setIsUploading, 
    isUploading, 
    setPreviewImage 
}: ImageUploadFieldProps) => {
    const images = form.watch('images') || [];

    const onDrop = async (acceptedFiles: File[]) => {
        if (acceptedFiles.length > 0) {
            setIsUploading(true);
            try {
                const promises = acceptedFiles.map((file) => new Promise<string>((resolve, reject) => {
                    const reader = new FileReader();
                    reader.onload = () => resolve(reader.result as string);
                    reader.onerror = reject;
                    reader.readAsDataURL(file);
                }));
                const results = await Promise.all(promises);
                const uniqueResults = results.filter(newImg => !images.includes(newImg));
                form.setValue('images', [...images, ...uniqueResults], { shouldValidate: true });
            } catch { 
                // Error handled
            } finally { 
                setIsUploading(false); 
            }
        }
    };

    const { getRootProps, getInputProps, isDragActive } = useDropzone({ onDrop, accept: { 'image/*': [] } });

    return (
        <div className="space-y-4">
            <div {...getRootProps()} className={`border-2 border-dashed rounded-xl p-8 flex flex-col items-center justify-center cursor-pointer transition-all bg-slate-50 dark:bg-slate-900/50 ${isDragActive ? 'border-accent-primary bg-accent-primary/5' : 'border-border-color hover:border-accent-primary/50'}`}>
                <input {...getInputProps()} />
                <div className="w-12 h-12 bg-accent-primary/5 text-accent-primary rounded-full flex items-center justify-center mb-3"><Upload size={20} /></div>
                <p className="text-sm font-medium text-primary">{isUploading ? t('common.loading') : t('common.dragOrClick')}</p>
            </div>
            {images.length > 0 && (
                <div className="grid grid-cols-4 sm:grid-cols-6 gap-4">
                    {images.map((img: string, idx: number) => (
                        <div key={idx} className="relative aspect-square rounded-lg border border-border-color overflow-hidden group bg-white cursor-pointer" onClick={() => setPreviewImage(img)}>
                            <img src={img} className="w-full h-full object-cover" />
                            <button type="button" onClick={(e) => { e.stopPropagation(); form.setValue('images', images.filter((_, i) => i !== idx)); }} className="absolute inset-0 bg-black/40 text-white opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center backdrop-blur-sm"><X size={16} /></button>
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
};
