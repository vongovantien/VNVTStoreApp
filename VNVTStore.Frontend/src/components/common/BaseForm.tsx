
import React, { useState, useCallback, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { useForm, Controller, UseFormReturn, Path, DefaultValues } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { ZodSchema } from 'zod';
import { Button, Input, Select, NumberInput, Switch, Modal } from '@/components/ui';
import { Upload, X } from 'lucide-react';
import { useDropzone } from 'react-dropzone';
import { uploadService } from '@/services/uploadService';

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
  | 'password';

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
  className?: string;
  hidden?: boolean;
}

export interface FieldGroup {
  title?: string;
  description?: string;
  fields: FieldDefinition[];
}

// ============ BaseForm Props ============
export interface BaseFormProps<T extends Record<string, unknown>> {
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
  modalSize?: 'sm' | 'md' | 'lg' | 'xl';
  onModalClose?: () => void;
  renderBefore?: (form: UseFormReturn<T>) => React.ReactNode;
  renderAfter?: (form: UseFormReturn<T>) => React.ReactNode;
  className?: string;
}

// ============ Field Renderer Component ============
interface FieldRendererProps<T extends Record<string, unknown>> {
  field: FieldDefinition;
  form: UseFormReturn<T>;
}

function FieldRenderer<T extends Record<string, unknown>>({ 
  field, 
  form 
}: FieldRendererProps<T>) {
  const { t } = useTranslation();
  const { control, register, formState: { errors }, setValue, watch } = form;
  const error = (errors as Record<string, { message?: string }>)[field.name]?.message;
  const [isUploading, setIsUploading] = useState(false);

  const onDrop = useCallback(async (acceptedFiles: File[]) => {
    if (acceptedFiles.length > 0 && field.type === 'image') {
      try {
        setIsUploading(true);
        const url = await uploadService.upload(acceptedFiles[0]);
        setValue(field.name as Path<T>, url as T[keyof T]);
      } catch (err) {
        console.error("Upload failed", err);
      } finally {
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
  const labelWithRequired = field.required ? `${field.label} *` : field.label;

  switch (field.type) {
    case 'text':
    case 'email':
    case 'phone':
    case 'password':
      return (
        <div className={colSpanClass}>
          <Input
            type={field.type === 'phone' ? 'tel' : field.type}
            label={labelWithRequired}
            placeholder={field.placeholder}
            disabled={field.disabled}
            error={error}
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
                label={labelWithRequired}
                placeholder={field.placeholder}
                value={f.value as number}
                onChange={f.onChange}
                min={field.min}
                max={field.max}
                step={field.step}
                disabled={field.disabled}
                error={error}
              />
            )}
          />
        </div>
      );

    case 'textarea':
      return (
        <div className={colSpanClass}>
          <div className="space-y-1">
            <label className="text-sm font-semibold text-gray-700 dark:text-gray-300">
              {labelWithRequired}
            </label>
            <textarea
              className="w-full min-h-[100px] px-3 py-2 border border-gray-300 dark:border-gray-700 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 bg-white dark:bg-slate-900 transition-all resize-y text-sm"
              placeholder={field.placeholder}
              rows={field.rows || 4}
              disabled={field.disabled}
              {...register(field.name as Path<T>)}
            />
            {error && <p className="text-xs text-red-500">{error}</p>}
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
                label={labelWithRequired}
                options={field.options || []}
                value={f.value as string}
                onChange={f.onChange}
                disabled={field.disabled}
                error={error}
              />
            )}
          />
        </div>
      );

    case 'switch':
      return (
        <div className={colSpanClass}>
          <div className="p-4 bg-gray-50 dark:bg-slate-800/50 rounded-lg">
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
      return (
        <div className={colSpanClass}>
          <div className="space-y-2">
            <label className="text-sm font-semibold text-gray-700 dark:text-gray-300">
              {labelWithRequired}
            </label>
            
            {currentValue && (
              <div className="relative w-32 h-32 border rounded-lg overflow-hidden group">
                <img src={currentValue} alt="Preview" className="w-full h-full object-cover" />
                <button
                  type="button"
                  onClick={() => setValue(field.name as Path<T>, '' as T[keyof T])}
                  className="absolute top-1 right-1 p-1 bg-red-500 text-white rounded-full opacity-0 group-hover:opacity-100 transition-opacity"
                >
                  <X size={12} />
                </button>
              </div>
            )}
            
            <div
              {...getRootProps()}
              className={`border-2 border-dashed rounded-lg p-4 text-center cursor-pointer transition-colors ${
                isDragActive 
                  ? 'border-indigo-500 bg-indigo-50 dark:bg-indigo-900/20' 
                  : 'border-gray-300 dark:border-gray-600 hover:border-indigo-400'
              } ${currentValue ? 'h-16' : 'h-24'}`}
            >
              <input {...getInputProps()} />
              {isUploading ? (
                <p className="text-sm text-indigo-600">{t('common.uploading', 'Đang tải lên...')}</p>
              ) : (
                <div className="flex flex-col items-center justify-center h-full">
                  <Upload size={20} className="text-gray-400 mb-2" />
                  <p className="text-xs text-gray-500">
                    {isDragActive 
                      ? t('common.dropHere', 'Thả file vào đây') 
                      : t('common.dragOrClick', 'Kéo thả hoặc click để chọn file')}
                  </p>
                </div>
              )}
            </div>
            {error && <p className="text-xs text-red-500">{error}</p>}
          </div>
        </div>
      );

    default:
      return null;
  }
}

// ============ BaseForm Component ============
export function BaseForm<T extends Record<string, unknown>>({
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
}: BaseFormProps<T>) {
  const { t } = useTranslation();
  
  // @ts-expect-error - Zod schema type inference issue
  const form = useForm<T>({
    resolver: zodResolver(schema),
    defaultValues,
  });

  const handleSubmit = form.handleSubmit(async (data) => {
    await onSubmit(data);
  });

  const allFields = useMemo(() => {
    if (fields) return fields;
    if (fieldGroups) {
      return fieldGroups.flatMap(group => group.fields);
    }
    return [];
  }, [fields, fieldGroups]);

  const renderFields = () => {
    if (fieldGroups) {
      return fieldGroups.map((group, groupIndex) => (
        <div key={groupIndex} className="space-y-4">
          {group.title && (
            <div className="border-b pb-2">
              <h3 className="text-lg font-semibold text-gray-800 dark:text-gray-200">
                {group.title}
              </h3>
              {group.description && (
                <p className="text-sm text-gray-500 dark:text-gray-400">
                  {group.description}
                </p>
              )}
            </div>
          )}
          <div className="grid grid-cols-12 gap-4">
            {group.fields.map((field, fieldIndex) => (
              <FieldRenderer
                key={`${groupIndex}-${fieldIndex}`}
                field={field}
                form={form as UseFormReturn<Record<string, unknown>>}
              />
            ))}
          </div>
        </div>
      ));
    }

    return (
      <div className="grid grid-cols-12 gap-4">
        {allFields.map((field, index) => (
          <FieldRenderer 
            key={index} 
            field={field} 
            form={form as UseFormReturn<Record<string, unknown>>} 
          />
        ))}
      </div>
    );
  };

  const formContent = (
    <form onSubmit={handleSubmit} className={`space-y-6 ${className || ''}`}>
      {renderBefore?.(form)}
      
      {renderFields()}
      
      {renderAfter?.(form)}

      <div className="flex justify-end gap-3 pt-4 border-t">
        {onCancel && (
          <Button type="button" variant="outline" onClick={onCancel}>
            {cancelLabel || t('common.cancel')}
          </Button>
        )}
        <Button type="submit" isLoading={isLoading}>
          {submitLabel || t('common.save')}
        </Button>
      </div>
    </form>
  );

  if (isModal) {
    return (
      <Modal
        isOpen={modalOpen}
        onClose={onModalClose || onCancel || (() => {})}
        title={modalTitle}
        size={modalSize}
      >
        {formContent}
      </Modal>
    );
  }

  return formContent;
}

export default BaseForm;
