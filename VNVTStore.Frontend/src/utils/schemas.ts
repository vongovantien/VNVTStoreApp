import { z } from 'zod';
import type { TFunction } from 'i18next';

/**
 * Common Regex Patterns
 */
export const REGEX = {
    PHONE: /^(0[3|5|7|8|9])([0-9]{8})$/,
    PASSWORD: /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$/, // At least 6 chars, 1 upper, 1 lower, 1 digit
};

/**
 * Custom Zod-like helpers for common fields
 * Usage: zField.phone(t), zField.email(t)
 */
export const zField = {
    required: (t: TFunction, messageKey = 'validation.required') =>
        z.string().min(1, { message: t(messageKey) }),

    email: (t: TFunction) =>
        z.string().min(1, { message: t('validation.required') }).email({ message: t('validation.invalidEmail') }),

    phone: (t: TFunction) =>
        z.string().regex(REGEX.PHONE, { message: t('validation.invalidPhone') }),

    password: (t: TFunction) =>
        z.string().regex(REGEX.PASSWORD, { message: t('validation.weakPassword') }),
};

/**
 * Schema Factory
 * Provides translated schemas based on the current language
 */
export const createSchemas = (t: TFunction) => {
    // 1. Base Fragments (reusing zField)
    const baseUserFields = {
        fullName: zField.required(t),
        email: zField.email(t),
        phone: zField.phone(t),
    };

    // 2. Base Objects
    const userBaseSchema = z.object(baseUserFields);

    const authBaseSchema = z.object({
        email: zField.email(t),
        password: zField.password(t),
    });

    // 3. Derived Schemas (Inheritance via .extend and .merge)
    const loginSchema = authBaseSchema.extend({
        remember: z.boolean(),
    });

    const registerSchema = z.object({
        username: z.string().optional(),
        ...baseUserFields,
        password: zField.password(t),
        confirmPassword: zField.required(t),
        agreeTerms: z.boolean().refine(v => v === true, t('register.agreeTerms')),
    }).refine((data) => data.password === data.confirmPassword, {
        message: t('register.passwordMismatch'),
        path: ['confirmPassword'],
    });

    const changePasswordSchema = z.object({
        currentPassword: zField.required(t),
        newPassword: zField.password(t),
        confirmPassword: zField.required(t),
    }).refine((data) => data.newPassword === data.confirmPassword, {
        message: t('validation.passwordMismatch'),
        path: ['confirmPassword'],
    }).refine((data) => data.newPassword !== data.currentPassword, {
        message: t('validation.passwordSameAsOld'),
        path: ['newPassword'],
    });

    const addressSchema = z.object({
        fullName: zField.required(t),
        phone: zField.phone(t),
        category: zField.required(t),
        street: zField.required(t),
        ward: z.string().optional(),
        district: zField.required(t),
        city: zField.required(t),
        isDefault: z.boolean(),
    });

    const contactSchema = userBaseSchema.extend({
        subject: zField.required(t),
        message: zField.required(t),
    });

    const bannerSchema = z.object({
        title: zField.required(t),
        content: z.string().optional(),
        linkUrl: z.string().optional(),
        linkText: z.string().optional(),
        imageUrl: z.string().optional(),
        priority: z.number().int().default(0),
        isActive: z.boolean().default(true),
    });

    const generalSettingsSchema = z.object({
        storeName: zField.required(t, 'admin.settingsPage.general.storeName'),
        email: zField.email(t),
        phone: zField.phone(t),
        website: z.string().url(t('validation.invalidUrl')).optional().or(z.literal('')),
        address: zField.required(t, 'admin.settingsPage.general.address'),
        description: z.string().optional(),
    });

    const paymentSettingsSchema = z.object({
        cod: z.boolean().default(false),
        zaloPay: z.boolean().default(false),
        momo: z.boolean().default(false),
        vnpay: z.boolean().default(false),
        bankTransfer: z.boolean().default(false),
    });

    const shippingSettingsSchema = z.object({
        defaultFee: z.number().min(0, t('admin.settingsPage.shipping.priceMin')),
        freeShippingThreshold: z.number().min(0, t('admin.settingsPage.shipping.priceMin')),
        estimatedDelivery: zField.required(t),
    });

    const notificationsSettingsSchema = z.object({
        emailNewOrder: z.boolean().default(false),
        emailQuoteRequest: z.boolean().default(false),
        emailOrderStatus: z.boolean().default(false),
        lowStockAlert: z.boolean().default(false),
    });

    return {
        userBaseSchema,
        authBaseSchema,
        loginSchema,
        registerSchema,
        changePasswordSchema,
        addressSchema,
        contactSchema,
        bannerSchema,
        generalSettingsSchema,
        paymentSettingsSchema,
        shippingSettingsSchema,
        notificationsSettingsSchema,
    };
};

/**
 * Type helpers (optional, but useful for TS)
 */
export type SchemaType = ReturnType<typeof createSchemas>;
