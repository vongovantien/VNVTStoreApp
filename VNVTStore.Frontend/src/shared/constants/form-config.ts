export enum FieldType {
    TEXT = 'text',
    NUMBER = 'number',
    TEXTAREA = 'textarea',
    SELECT = 'select',
    CHECKBOX = 'checkbox',
    DATE = 'date',
}

export interface FieldConfig<T> {
    name: keyof T;
    type: FieldType;
    label: string;
    placeholder?: string;
    options?: { label: string; value: string | number }[]; // For Select
    className?: string;
    disabled?: boolean;
}
