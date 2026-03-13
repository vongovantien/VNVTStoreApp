import React from 'react';
import { useForm, SubmitHandler, Controller, DefaultValues, FieldValues } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { ZodSchema } from 'zod';
import { Button, Input, Textarea } from '@/components/ui';
import { FieldConfig, FieldType } from '@/shared/constants/form-config';

// Generic Props Interface
interface DynamicFormProps<T extends FieldValues> {
  schema: ZodSchema<T>;
  defaultValues?: DefaultValues<T>;
  fields: FieldConfig<T>[];
  onSubmit: SubmitHandler<T>;
  isLoading?: boolean;
  submitLabel?: string;
  cancelLabel?: string;
  onCancel?: () => void;
  renderHeader?: () => React.ReactNode;
}

export const DynamicForm = <T extends FieldValues>({
  schema,
  defaultValues,
  fields,
  onSubmit,
  isLoading,
  submitLabel = 'Submit',
  cancelLabel = 'Cancel',
  onCancel,
  renderHeader,
}: DynamicFormProps<T>) => {
  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<T>({
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    resolver: zodResolver(schema as any),
    defaultValues,
  });

  // Dynamic Field Renderer
  const renderField = (field: FieldConfig<T>) => {
    return (
      <Controller
        key={String(field.name)}
        control={control}
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        name={field.name as any} // Use 'any' to bypass deep path typing complexity for generic T
        render={({ field: controllerField }) => {
          const commonProps = {
            ...controllerField,
            // Wrap onChange to handle specific types
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            onChange: (e: any) => {
                let value = e;
                if (e?.target) {
                    value = field.type === FieldType.NUMBER ? parseFloat(e.target.value) : e.target.value;
                }
                controllerField.onChange(value);
            },
            label: field.label,
            placeholder: field.placeholder,
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            error: (errors as any)[field.name]?.message as string,
            disabled: field.disabled || isLoading,
            className: field.className,
          };

          switch (field.type) {
            case FieldType.TEXT:
            case FieldType.NUMBER:
            case FieldType.DATE:
              return (
                <Input
                  {...commonProps}
                  type={field.type}
                />
              );
            case FieldType.TEXTAREA:
              return <Textarea {...commonProps} />;
            default:
              return <Input {...commonProps} />;
          }
        }}
      />
    );
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      {renderHeader && renderHeader()}

      <div className="space-y-4">
        {fields.map(renderField)}
      </div>

      <div className="flex justify-end gap-3 pt-4 border-t border-slate-100 dark:border-slate-800">
        {onCancel && (
          <Button variant="ghost" onClick={onCancel} type="button" disabled={isLoading}>
            {cancelLabel}
          </Button>
        )}
        <Button 
            type="submit" 
            isLoading={isLoading} 
            disabled={isLoading}
            className="bg-rose-600 hover:bg-rose-700 text-white"
        >
          {submitLabel}
        </Button>
      </div>
    </form>
  );
};
// End of file

