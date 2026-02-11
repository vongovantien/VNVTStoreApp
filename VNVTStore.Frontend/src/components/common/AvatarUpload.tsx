import React, { useState, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import { Camera, Image as ImageIcon, Loader2 } from 'lucide-react';
import { cn } from '@/utils/cn';
import { useToast } from '@/store';
import { apiClient } from '@/services/api';

interface AvatarUploadProps {
    currentAvatarUrl?: string;
    onUploadSuccess: (url: string) => void;
    className?: string;
    size?: 'sm' | 'md' | 'lg' | 'xl';
}

export const AvatarUpload: React.FC<AvatarUploadProps> = ({
    currentAvatarUrl,
    onUploadSuccess,
    className,
    size = 'lg'
}) => {
    const { t } = useTranslation();
    const toast = useToast();
    const [isUploading, setIsUploading] = useState(false);
    const [previewUrl, setPreviewUrl] = useState<string | null>(currentAvatarUrl || null);
    const fileInputRef = useRef<HTMLInputElement>(null);

    const sizeClasses = {
        sm: 'w-16 h-16',
        md: 'w-24 h-24',
        lg: 'w-32 h-32',
        xl: 'w-40 h-40'
    };

    const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (!file) return;

        // Validation
        const validTypes = ['image/jpeg', 'image/png', 'image/webp'];
        if (!validTypes.includes(file.type)) {
            toast.error(t('validation.invalidFileType') || 'Invalid file type. Please use JPEG, PNG, or WebP.');
            return;
        }

        const maxSize = 2 * 1024 * 1024; // 2MB
        if (file.size > maxSize) {
            toast.error(t('validation.fileTooLarge') || 'File is too large. Max size is 2MB.');
            return;
        }

        // Preview
        const objectUrl = URL.createObjectURL(file);
        setPreviewUrl(objectUrl);

        // Upload
        setIsUploading(true);
        const formData = new FormData();
        formData.append('file', file);

        try {
            // Using direct API call here or via a dedicated service method
            // Assuming UploadController is at /api/v1/upload
            const response = await apiClient.post<{ url: string }>('/api/v1/upload', formData, {
                headers: {
                    'Content-Type': 'multipart/form-data',
                },
            });

            if (response.success && response.data) {
                onUploadSuccess(response.data.url);
                toast.success(t('common.messages.uploadSuccess') || 'Avatar uploaded successfully');
            } else {
                setPreviewUrl(currentAvatarUrl || null); // Revert on failure
                toast.error(response.message || t('common.messages.uploadError') || 'Upload failed');
            }
        } catch (error) {
            console.error('Upload error:', error);
            setPreviewUrl(currentAvatarUrl || null); // Revert on failure
            toast.error(t('common.messages.uploadError') || 'Upload failed');
        } finally {
            setIsUploading(false);
        }
    };

    const handleRemove = () => {
        setPreviewUrl(null);
        onUploadSuccess(''); // Clear avatar
        if (fileInputRef.current) fileInputRef.current.value = '';
    };

    return (
        <div className={cn("relative group", sizeClasses[size], className)}>
            <div className={cn(
                "rounded-full overflow-hidden border-4 border-white dark:border-slate-800 shadow-lg w-full h-full bg-slate-100 dark:bg-slate-800 flex items-center justify-center relative",
                isUploading && "opacity-70"
            )}>
                {previewUrl ? (
                    <img 
                        src={previewUrl} 
                        alt="Avatar" 
                        className="w-full h-full object-cover" 
                    />
                ) : (
                    <UserPlaceholder size={size} />
                )}
                
                {isUploading && (
                    <div className="absolute inset-0 flex items-center justify-center bg-black/20 backdrop-blur-sm">
                        <Loader2 className="animate-spin text-white" size={24} />
                    </div>
                )}
            </div>

            <button
                type="button"
                onClick={() => fileInputRef.current?.click()}
                disabled={isUploading}
                className="absolute bottom-0 right-0 p-2 bg-primary text-white rounded-full shadow-md hover:bg-primary/90 transition-all border-2 border-white dark:border-slate-900"
                title={t('common.actions.upload') || "Upload Avatar"}
            >
                <Camera size={16} />
            </button>
            
            <input 
                ref={fileInputRef}
                type="file" 
                accept="image/jpeg,image/png,image/webp" 
                className="hidden" 
                onChange={handleFileChange}
            />
        </div>
    );
};

const UserPlaceholder = ({ size }: { size: string }) => {
    const iconSizes = {
        sm: 24,
        md: 32,
        lg: 48,
        xl: 64
    };
    const iconSize = iconSizes[size as keyof typeof iconSizes] || 32;
    
    return <div className="text-slate-300 dark:text-slate-600"><ImageIcon size={iconSize} /></div>;
}
