
import React, { useState, useCallback, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { useForm, Controller, UseFormReturn, Path, DefaultValues, FieldValues, Resolver } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { ZodSchema } from 'zod';
import { Button, Input, Select, NumberInput, Switch, Modal } from '@/components/ui';
import { Upload, X, ImageOff } from 'lucide-react';
import { useDropzone } from 'react-dropzone';

// ============ Field Definition Types ============
export type FieldType = 
  | 'text' 
  | 'email' 
  | 'phone' 
  | 'number' 
  | 'textarea' 
  | 'select' 
  | 'switch' 
  | 'image' 
  | 'multi-image'
  | 'password'
  | 'custom';

export interface SelectOption {
  value: string;
  label: string;
}

export interface FieldDefinition {
  name: string;
  type: FieldType;
  label: string;
  placeholder?: string;
  description?: string;
  required?: boolean;
  disabled?: boolean;
  options?: SelectOption[];
  min?: number;
  max?: number;
  step?: number;
  rows?: number;
  colSpan?: 1 | 2 | 3 | 4 | 6 | 12;
  size?: 'sm' | 'md' | 'lg';
  className?: string;
  hidden?: boolean;
  /** For 'custom' type */
  render?: (form: UseFormReturn<any>) => React.ReactNode; 
  /** For 'image' or other supporting types */
  multiple?: boolean;
}

export interface FieldGroup {
  title?: string;
  description?: string;
  fields: FieldDefinition[];
}

// ============ BaseForm Props ============
export interface BaseFormProps<T extends FieldValues> {
  schema: ZodSchema<T>;
  defaultValues: DefaultValues<T>;
  fields?: FieldDefinition[];
  fieldGroups?: FieldGroup[];
  onSubmit: (data: T) => void | Promise<void>;
  onCancel?: () => void;
  isLoading?: boolean;
  submitLabel?: string;
  cancelLabel?: string;
  isModal?: boolean;
  modalOpen?: boolean;
  modalTitle?: string;
  modalSize?: 'sm' | 'md' | 'lg' | 'xl' | '2xl' | '3xl' | '4xl' | '5xl' | '6xl' | '7xl' | 'full';
  onModalClose?: () => void;
  renderBefore?: (form: UseFormReturn<T>) => React.ReactNode;
  renderAfter?: (form: UseFormReturn<T>) => React.ReactNode;
  className?: string;
  groupLayoutClassName?: string; // New prop for controlling groups container layout
  imageBaseUrl?: string; // New prop for image preview base URL
  layout?: 'vertical' | 'sidebar' | 'tabs'; // sidebar will use a Grid layout for groups
  sidebarColSpan?: number; // Span for sidebar if layout is sidebar
}

// ============ Field Renderer Component ============
interface FieldRendererProps<T extends FieldValues> {
  field: FieldDefinition;
  form: UseFormReturn<T>;
  onPreviewImage: (url: string) => void;
  imageBaseUrl?: string;
}

function FieldRenderer<T extends FieldValues>({ 
  field, 
  form,
  onPreviewImage,
  imageBaseUrl
}: FieldRendererProps<T>) {
  const { t } = useTranslation();
  const { control, register, formState: { errors }, setValue, watch } = form;
  const error = (errors as Record<string, { message?: string }>)[field.name]?.message;
  const [isUploading, setIsUploading] = useState(false);
  const [imageError, setImageError] = useState(false);

  // Reset error when value changes
  React.useEffect(() => {
    setImageError(false);
  }, [watch(field.name as Path<T>)]);

  // ... (useDropzone logic) 
  const onDrop = useCallback(async (acceptedFiles: File[]) => {
    if (acceptedFiles.length > 0 && field.type === 'image') {
      try {
        setIsUploading(true);
        const file = acceptedFiles[0];
        const reader = new FileReader();
        reader.onload = () => {
          const base64 = reader.result as string;
          setValue(field.name as Path<T>, base64 as any);
          setIsUploading(false);
        };
        reader.onerror = () => {
          console.error("Failed to read file");
          setIsUploading(false);
        };
        reader.readAsDataURL(file);
      } catch (err) {
        console.error("Upload failed", err);
        setIsUploading(false);
      }
    }
  }, [field.name, field.type, setValue]);

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: { 'image/*': [] },
    disabled: field.disabled,
  });

  if (field.hidden) return null;

  const colSpanClass = `col-span-${field.colSpan || 12}`;
  
  // Helper to format image URL
  const getPreviewUrl = (url: string) => {
    if (!url) return '';
    if (url.startsWith('data:') || url.startsWith('http')) return url;
    if (imageBaseUrl) {
       // Simple join: remove trailing slash from base and leading from url
       const base = imageBaseUrl.replace(/\/$/, '');
       const path = url.startsWith('/') ? url : `/${url}`;
       return `${base}${path}`;
    }
    return url;
  };

  switch (field.type) {
    // ... (cases) ...
    case 'text':
    case 'email':
    case 'phone':
    case 'password':
      return (
        <div className={colSpanClass}>
          <Input
            type={field.type === 'phone' ? 'tel' : field.type}
            label={field.label}
            placeholder={field.placeholder}
            disabled={field.disabled}
            size={field.size || 'md'}
            error={error}
            isRequired={field.required}
            {...register(field.name as Path<T>)}
          />
        </div>
      );

    case 'number':
      return (
        <div className={colSpanClass}>
          <Controller
            name={field.name as Path<T>}
            control={control}
            render={({ field: f }) => (
              <NumberInput
                label={field.label}
                placeholder={field.placeholder}
                value={f.value as number}
                onChange={f.onChange}
                min={field.min}
                max={field.max}
                step={field.step}
                disabled={field.disabled}
                error={error}
                isRequired={field.required}
              />
            )}
          />
        </div>
      );

    case 'textarea':
      return (
        <div className={colSpanClass}>
          <div className="space-y-1">
            <label className="text-sm font-bold text-primary mb-1 block">
              {field.label}
              {field.required && <span className="text-red-500 ml-1">*</span>}
            </label>
            <textarea
              className="w-full min-h-[100px] px-4 py-3 border rounded-xl focus:outline-none focus:ring-4 focus:ring-accent-primary/5 bg-primary transition-all resize-y text-base text-primary placeholder:text-tertiary"
              placeholder={field.placeholder}
              rows={field.rows || 4}
              disabled={field.disabled}
              {...register(field.name as Path<T>)}
            />
            {error && <p className="text-xs text-error font-medium">{error}</p>}
          </div>
        </div>
      );

    case 'select':
      return (
        <div className={colSpanClass}>
          <Controller
            name={field.name as Path<T>}
            control={control}
            render={({ field: f }) => (
              <Select
                label={field.label}
                options={field.options || []}
                value={f.value as string}
                onChange={f.onChange}
                disabled={field.disabled}
                error={error}
                isRequired={field.required}
              />
            )}
          />
        </div>
      );

    case 'custom':
      return (
        <div className={colSpanClass}>
          {field.render?.(form)}
        </div>
      );
    
    case 'multi-image':
      // Basic multi-image handled in ProductForm via custom render for now 
      // but we could implement generic one here too.
      // For now, let's keep ProductForm specific parts in custom render until generic enough.
      return null;

    case 'switch':
      return (
        <div className={colSpanClass}>
          <div className="p-4 bg-tertiary/30 dark:bg-slate-900/10 rounded-xl border border-border-color">
            <Controller
              name={field.name as Path<T>}
              control={control}
              render={({ field: f }) => (
                <Switch
                  label={field.label}
                  description={field.description}
                  checked={(f.value as boolean) ?? false}
                  onChange={f.onChange}
                  disabled={field.disabled}
                />
              )}
            />
          </div>
        </div>
      );

    case 'image':
      const currentValue = watch(field.name as Path<T>) as string;
      const previewUrl = getPreviewUrl(currentValue);
      return (
        <div className={colSpanClass}>
          <div className="space-y-2">
            <label className="text-sm font-semibold text-primary">
              {field.label}
              {field.required && <span className="text-red-500 ml-1">*</span>}
            </label>
            
            {currentValue && (
              <div className="relative w-32 h-32 border rounded-lg overflow-hidden group">
                  <div className="w-full h-full flex items-center justify-center bg-secondary">
                    {imageError ? (
                      <div className="flex flex-col items-center text-tertiary">
                        <ImageOff size={24} />
                        <span className="text-[10px] mt-1 uppercase font-bold">Error</span>
                      </div>
                    ) : (
                      <img 
                        src={previewUrl} 
                        alt="Preview" 
                        className="w-full h-full object-cover cursor-pointer hover:scale-105 transition-transform" 
                        onClick={() => onPreviewImage(previewUrl)}
                        onError={() => {
                          console.error("Image load failed:", previewUrl);
                          setImageError(true);
                        }}
                      />
                    )}
                  </div>
                <button
                  type="button"
                  onClick={() => setValue(field.name as Path<T>, '' as any)}
                  className="absolute top-1 right-1 p-1 bg-error text-white rounded-full opacity-0 group-hover:opacity-100 transition-opacity"
                >
                  <X size={12} />
                </button>
              </div>
            )}
            
            <div
              {...getRootProps()}
              className={`border-2 border-dashed rounded-xl p-4 text-center cursor-pointer transition-colors ${
                isDragActive 
                  ? 'border-accent-primary bg-accent-primary/5' 
                  : 'border-border-color hover:border-accent-primary/50'
              } ${currentValue ? 'h-16' : 'h-24'}`}
            >
              <input {...getInputProps()} />
              {isUploading ? (
                <p className="text-sm text-accent-primary font-medium">{t('common.uploading', 'Đang tải lên...')}</p>
              ) : (
                <div className="flex flex-col items-center justify-center h-full">
                  <Upload size={20} className="text-tertiary mb-2" />
                  <p className="text-xs text-secondary font-medium">
                    {isDragActive 
                      ? t('common.dropHere', 'Thả file vào đây') 
                      : t('common.dragOrClick', 'Kéo thả hoặc click để chọn file')}
                  </p>
                </div>
              )}
            </div>
            {error && <p className="text-xs text-error font-medium">{error}</p>}
          </div>
        </div>
      );
    default:
      return null;
  }
}

// ============ BaseForm Component ============
export function BaseForm<T extends FieldValues>({
  schema,
  defaultValues,
  fields,
  fieldGroups,
  onSubmit,
  onCancel,
  isLoading = false,
  submitLabel,
  cancelLabel,
  isModal = false,
  modalOpen = false,
  modalTitle,
  modalSize = 'lg',
  onModalClose,
  renderBefore,
  renderAfter,
  className,
  groupLayoutClassName,
  imageBaseUrl,
  layout = 'vertical',
  sidebarColSpan = 4
}: BaseFormProps<T>) {
  const { t } = useTranslation();
  const formId = useMemo(() => `form-${Math.random().toString(36).substr(2, 9)}`, []);
  const [previewImage, setPreviewImage] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<number>(0);

  const form = useForm<T>({
    resolver: zodResolver(schema as any) as any,
    values: defaultValues as any,
    resetOptions: {
      keepDirtyValues: true, // Don't overwrite what the user has typed
    }
  });

  const { handleSubmit: handleFormSubmit } = form;

  const handleSubmit = (e?: React.BaseSyntheticEvent) => {
    if (e) {
      e.preventDefault();
      e.stopPropagation();
    }
    return handleFormSubmit(onSubmit as any)(e);
  };

  const handleManualSubmit = (e?: React.MouseEvent | React.KeyboardEvent) => {
    if (e) {
        e.preventDefault();
        e.stopPropagation();
    }
    handleFormSubmit(onSubmit as any)();
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (isModal && e.key === 'Enter' && e.target instanceof HTMLInputElement && e.target.type !== 'textarea') {
      handleManualSubmit(e);
    }
    // Prevent bubbling to parent forms
    if (e.key === 'Enter' || e.key === ' ') {
      e.stopPropagation();
    }
  };
  
  const allFields = fields || (fieldGroups ? fieldGroups.flatMap(g => g.fields) : []);

  // Pass imageBaseUrl to FieldRenderer
  const renderFields = () => {
     if (fieldGroups) {
      if (layout === 'sidebar') {
           const mainGroups = fieldGroups.slice(0, fieldGroups.length - 1);
           const sidebarGroup = fieldGroups[fieldGroups.length - 1];

           return (
            <div className="grid grid-cols-12 gap-6">
                <div className={`col-span-12 lg:col-span-${12 - sidebarColSpan} space-y-8`}>
                    {mainGroups.map((group, groupIndex) => renderGroup(group, groupIndex))}
                </div>
                <div className={`col-span-12 lg:col-span-${sidebarColSpan} space-y-8`}>
                    {renderGroup(sidebarGroup, mainGroups.length)}
                </div>
            </div>
           );
      }

      if (layout === 'tabs') {
        return (
          <div className="space-y-4">
            <div className="flex flex-wrap border-b border-border-color">
              {fieldGroups.map((group, index) => (
                <button
                  key={index}
                  type="button"
                  onClick={() => setActiveTab(index)}
                  className={`px-4 py-3 text-sm font-medium transition-colors border-b-2 -mb-px ${
                    activeTab === index
                      ? 'border-accent-primary text-accent-primary'
                      : 'border-transparent text-secondary hover:text-primary'
                  }`}
                >
                  {group.title || `Tab ${index + 1}`}
                </button>
              ))}
            </div>
            
            <div className="pt-2">
              {fieldGroups.map((group, index) => (
                <div key={index} className={activeTab === index ? 'block animate-in fade-in duration-200' : 'hidden'}>
                  {renderGroup(group, index, layout === 'tabs')}
                </div>
              ))}
            </div>
          </div>
        );
      }

      return (
        <div className={groupLayoutClassName || 'space-y-8'}>
          {fieldGroups.map((group, groupIndex) => renderGroup(group, groupIndex))}
        </div>
      );
    }

    return (
      <div className="grid grid-cols-12 gap-4">
        {allFields.map((field, index) => (
          <FieldRenderer 
            key={index} 
            field={field} 
            form={form}
            onPreviewImage={setPreviewImage}
            imageBaseUrl={imageBaseUrl}
          />
        ))}
      </div>
    );
  };

  const renderGroup = (group: FieldGroup, groupIndex: number, hideTitle: boolean = false) => (
    <div key={groupIndex} className="bg-white dark:bg-slate-800 rounded-2xl shadow-sm border border-border-color p-6 space-y-4">
        {(!hideTitle && (group.title || group.description)) && (
            <div className="space-y-1 mb-6">
                {group.title && (
                    <div className="flex items-center gap-3">
                        <div className="w-10 h-10 rounded-xl bg-accent-primary/10 flex items-center justify-center text-accent-primary font-bold text-base">
                            {groupIndex + 1}
                        </div>
                        <h3 className="text-base font-extrabold text-primary uppercase tracking-widest">{group.title}</h3>
                    </div>
                )}
                {group.description && <p className="text-sm text-secondary ml-13">{group.description}</p>}
            </div>
        )}
        <div className="grid grid-cols-12 gap-4">
            {group.fields.map((field, fieldIndex) => (
                <FieldRenderer
                    key={`${groupIndex}-${fieldIndex}`}
                    field={field}
                    form={form}
                    onPreviewImage={setPreviewImage}
                    imageBaseUrl={imageBaseUrl}
                />
            ))}
        </div>
    </div>
  );
   // ... rest of component


  const actionButtons = (
    <>
      {onCancel && (
        <Button 
          type="button" 
          variant="outline" 
          onClick={onCancel}
        >
          {cancelLabel || t('common.cancel')}
        </Button>
      )}
      <Button 
        type={isModal ? "button" : "submit"} 
        isLoading={isLoading} 
        form={isModal ? undefined : formId}
        onClick={isModal ? handleManualSubmit : (e) => e.stopPropagation()}
      >
        {submitLabel || t('common.save')}
      </Button>
    </>
  );

  const FormTag = isModal ? 'div' : 'form';

  const formContent = (
    <FormTag 
      id={formId} 
      onSubmit={isModal ? undefined : handleSubmit} 
      onKeyDown={handleKeyDown}
      className={`space-y-6 ${className || ''}`}
    >
      {renderBefore?.(form)}
      
      {renderFields()}
      
      {renderAfter?.(form)}

      {/* If NOT modal, render buttons inside form normal flow */}
      {!isModal && (
        <div className="flex justify-end gap-3 pt-4 border-t">
          {actionButtons}
        </div>
      )}
    </FormTag>
  );

  const previewModal = previewImage && (
    <div className="fixed inset-0 z-[9999] flex items-center justify-center bg-black/90 backdrop-blur-sm p-4 animate-in fade-in duration-200" onClick={() => setPreviewImage(null)}>
      <div className="relative max-w-full max-h-full flex flex-col items-center justify-center">
        <img 
          src={previewImage} 
          alt="Full size preview" 
          className="max-w-full max-h-[90vh] object-contain rounded-lg shadow-2xl" 
          onClick={(e) => e.stopPropagation()} 
        />
        <button
          type="button"
          onClick={() => setPreviewImage(null)}
          className="absolute top-4 right-4 p-2 bg-white/10 text-white rounded-full hover:bg-white/20 transition-colors backdrop-blur-md"
        >
          <X size={24} />
        </button>
      </div>
    </div>
  );

  if (isModal) {
    return (
      <>
        <Modal
          isOpen={modalOpen}
          onClose={onModalClose || onCancel || (() => {})}
          title={modalTitle}
          size={modalSize}
          footer={actionButtons} // Pass buttons to fixed footer
        >
          {formContent}
        </Modal>
        {previewModal}
      </>
    );
  }

  return (
    <>
      {formContent}
      {previewModal}
    </>
  );
}

export default BaseForm;
