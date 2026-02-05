/**
 * Validation Utilities
 */

export const validationRules = {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    required: (value: any) => {
        if (value === null || value === undefined || value === '') return false;
        if (Array.isArray(value) && value.length === 0) return false;
        return true;
    },
    email: (value: string) => {
        const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return re.test(value);
    },
    phone: (value: string) => {
        const re = /^(\+?\d{1,3}[- ]?)?\d{10}$/;
        return re.test(value);
    },
    minLength: (value: string, min: number) => {
        return value.length >= min;
    },
    password: (value: string) => {
        // At least 6 chars, 1 uppercase, 1 lowercase, 1 number
        const re = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$/;
        return re.test(value);
    }
};

export interface ValidationErrors {
    [key: string]: string | null;
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export const validateForm = (data: Record<string, any>, schema: { [key: string]: { rules: ((v: any) => boolean)[], messages: string[] } }): ValidationErrors => {
    const errors: ValidationErrors = {};

    Object.keys(schema).forEach(field => {
        const { rules, messages } = schema[field];
        for (let i = 0; i < rules.length; i++) {
            if (!rules[i](data[field])) {
                errors[field] = messages[i];
                break;
            }
            errors[field] = null;
        }
    });

    return errors;
};
