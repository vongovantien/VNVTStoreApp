import React, { useState, useRef, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Camera, Image as ImageIcon, Loader2 } from 'lucide-react';
import Cropper from 'react-easy-crop';
import { cn } from '@/utils/cn';
import { useToast } from '@/store';
import Modal from '@/components/ui/Modal';
import { Button } from '@/components/ui/Button';
import getCroppedImg from '@/utils/cropImage';

import { getImageUrl } from '@/utils/format';

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
    const [previewUrl, setPreviewUrl] = useState<string | null>(getImageUrl(currentAvatarUrl) || null);
    
    // Cropping State
    const [imageSrc, setImageSrc] = useState<string | null>(null);
    const [crop, setCrop] = useState({ x: 0, y: 0 });
    const [zoom, setZoom] = useState(1);
    const [croppedAreaPixels, setCroppedAreaPixels] = useState<any>(null);
    const [isCropping, setIsCropping] = useState(false);

    const fileInputRef = useRef<HTMLInputElement>(null);

    const sizeClasses = {
        sm: 'w-16 h-16',
        md: 'w-24 h-24',
        lg: 'w-32 h-32',
        xl: 'w-40 h-40'
    };

    const onCropComplete = useCallback((croppedArea: any, croppedAreaPixels: any) => {
        setCroppedAreaPixels(croppedAreaPixels);
    }, []);

    const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (!file) return;

        // Validation
        const validTypes = ['image/jpeg', 'image/png', 'image/webp'];
        if (!validTypes.includes(file.type)) {
            toast.error(t('validation.invalidFileType') || 'Invalid file type. Please use JPEG, PNG, or WebP.');
            return;
        }

        const maxSize = 10 * 1024 * 1024; // 10MB
        if (file.size > maxSize) {
            toast.error(t('validation.fileTooLarge') || 'File is too large. Max size is 10MB.');
            return;
        }

        // Read file for cropping
        const reader = new FileReader();
        reader.addEventListener('load', () => {
             setImageSrc(reader.result?.toString() || '');
             setIsCropping(true);
        });
        reader.readAsDataURL(file);
        
        // Reset input value to allow re-selecting same file
        e.target.value = '';
    };

    const handleCropSave = async () => {
        try {
            if (!imageSrc || !croppedAreaPixels) return;
            
            setIsUploading(true);
            const croppedImageBase64 = await getCroppedImg(imageSrc, croppedAreaPixels);
            
            if (croppedImageBase64) {
                setPreviewUrl(croppedImageBase64);
                onUploadSuccess(croppedImageBase64);
                toast.success(t('common.messages.uploadReady') || 'Avatar ready to save');
                handleCloseCropper();
            }
        } catch (e) {
            console.error(e);
            toast.error('Failed to crop image');
        } finally {
            setIsUploading(false);
        }
    };

    const handleCloseCropper = () => {
        setIsCropping(false);
        setImageSrc(null);
        setZoom(1);
        setCrop({ x: 0, y: 0 });
    };

    return (
        <div className={cn("relative group", sizeClasses[size], className)}>
            <div className={cn(
                "rounded-full overflow-hidden border-4 border-white dark:border-slate-800 shadow-xl w-full h-full bg-slate-50 dark:bg-slate-900 flex items-center justify-center relative transition-all duration-300 group-hover:shadow-indigo-500/10",
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
                className="absolute bottom-0 right-0 p-3 bg-[#2d7ad6] text-white rounded-full shadow-lg hover:bg-blue-600 transition-all border-4 border-white dark:border-slate-800 transform group-hover:scale-110 group-hover:rotate-6 duration-300"
                title={t('common.actions.upload') || "Upload Avatar"}
            >
                <Camera size={18} className="stroke-[2.5]" />
            </button>
            
            <input 
                ref={fileInputRef}
                type="file" 
                accept="image/jpeg,image/png,image/webp" 
                className="hidden" 
                onChange={handleFileChange}
            />

            {/* Cropper Modal */}
            <Modal
                isOpen={isCropping}
                onClose={handleCloseCropper}
                title={t('common.actions.cropImage') || "Adjust Image"}
                size="lg"
                footer={
                    <div className="flex justify-end gap-3 w-full">
                        <Button variant="ghost" onClick={handleCloseCropper}>
                            {t('common.actions.cancel') || "Cancel"}
                        </Button>
                        <Button onClick={handleCropSave} isLoading={isUploading} className="bg-[#2d7ad6] hover:bg-blue-600 border-none px-8 shadow-lg shadow-blue-500/20">
                            {t('common.actions.save') || "Save"}
                        </Button>
                    </div>
                }
            >
                <div className="relative h-72 w-full bg-slate-900 rounded-xl overflow-hidden mb-6 shadow-inner">
                    {imageSrc && (
                        <Cropper
                            image={imageSrc}
                            crop={crop}
                            zoom={zoom}
                            aspect={1}
                            onCropChange={setCrop}
                            onCropComplete={onCropComplete}
                            onZoomChange={setZoom}
                            cropShape="round"
                            showGrid={false}
                        />
                    )}
                </div>
                
                <div className="px-5 py-4 bg-slate-50 dark:bg-slate-800/50 rounded-xl border border-secondary/5">
                    <div className="flex items-center gap-5">
                        <div className="p-2 bg-white dark:bg-slate-700 rounded-lg shadow-sm">
                            <ImageIcon size={20} className="text-secondary" />
                        </div>
                        <div className="flex-1">
                            <div className="flex justify-between mb-2">
                                <span className="text-xs font-bold text-tertiary uppercase tracking-wider">Zoom Level</span>
                                <span className="text-xs font-mono font-bold text-accent">{Math.round(zoom * 100)}%</span>
                            </div>
                            <input
                                type="range"
                                value={zoom}
                                min={1}
                                max={3}
                                step={0.1}
                                aria-labelledby="Zoom"
                                onChange={(e) => setZoom(Number(e.target.value))}
                                className="w-full h-1.5 bg-slate-200 dark:bg-slate-700 rounded-lg appearance-none cursor-pointer accent-[#2d7ad6]"
                            />
                        </div>
                    </div>
                </div>
            </Modal>
        </div>
    );
};

const UserPlaceholder = ({ size }: { size: string }) => {
    const { user } = useAuthStore();
    
    // Gradient backgrounds based on initials to add variety
    const gradients = [
        'from-indigo-500 to-purple-500',
        'from-blue-500 to-cyan-500',
        'from-purple-500 to-pink-500',
        'from-emerald-500 to-teal-500',
        'from-orange-500 to-red-500'
    ];
    
    const initials = user?.fullName?.charAt(0).toUpperCase() || user?.email?.charAt(0).toUpperCase() || '?';
    const gradientIndex = initials.charCodeAt(0) % gradients.length;
    const gradient = gradients[gradientIndex];

    const fontSizes = {
        sm: 'text-xl',
        md: 'text-3xl',
        lg: 'text-5xl',
        xl: 'text-6xl'
    };
    
    return (
        <div className={cn(
            "w-full h-full flex items-center justify-center bg-gradient-to-br text-white font-extrabold shadow-inner",
            gradient,
            fontSizes[size as keyof typeof fontSizes]
        )}>
            {initials}
        </div>
    );
}

import { useAuthStore } from '@/store';
