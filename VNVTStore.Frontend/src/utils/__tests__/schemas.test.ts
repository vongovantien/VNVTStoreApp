import { describe, it, expect, vi } from 'vitest';
import { createSchemas } from '../schemas';
import { z } from 'zod';

// Mock TFunction
const t = vi.fn((key: string, params?: any) => {
    if (params?.field) return `${params.field} is required`;
    return key;
});

describe('Validation Schemas', () => {
    const schemas = createSchemas(t as any);

    describe('contactSchema', () => {
        it('should validate correctly with valid data', () => {
            const validData = {
                fullName: 'Nguyên Văn A',
                email: 'test@example.com',
                phone: '0901234567',
                subject: 'Hỗ trợ',
                message: 'Nội dung tin nhắn test'
            };
            const result = schemas.contactSchema.safeParse(validData);
            expect(result.success).toBe(true);
        });

        it('should fail if required fields are missing', () => {
            const invalidData = {
                fullName: '',
                email: 'invalid-email',
                subject: '',
                message: ''
            };
            const result = schemas.contactSchema.safeParse(invalidData);
            expect(result.success).toBe(false);
            if (!result.success) {
                const errors = result.error.flatten().fieldErrors;
                expect(errors.fullName).toBeDefined();
                expect(errors.email).toContain('validation.invalidEmail');
                expect(errors.subject).toBeDefined();
                expect(errors.message).toBeDefined();
            }
        });

        it('should fail with invalid phone format', () => {
            const data = {
                fullName: 'A',
                email: 'a@b.com',
                phone: '123', // Invalid
                subject: 'S',
                message: 'M'
            };
            const result = schemas.contactSchema.safeParse(data);
            expect(result.success).toBe(false);
            if (!result.success) {
                expect(result.error.flatten().fieldErrors.phone).toContain('validation.invalidPhone');
            }
        });
    });

    describe('changePasswordSchema', () => {
        it('should fail if passwords do not match', () => {
            const data = {
                currentPassword: 'Password123',
                newPassword: 'NewPassword123!',
                confirmPassword: 'WrongPassword'
            };
            const result = schemas.changePasswordSchema.safeParse(data);
            expect(result.success).toBe(false);
            if (!result.success) {
                expect(result.error.flatten().fieldErrors.confirmPassword).toContain('validation.passwordMismatch');
            }
        });

        it('should fail if new password is same as old', () => {
            const data = {
                currentPassword: 'Password123',
                newPassword: 'Password123',
                confirmPassword: 'Password123'
            };
            const result = schemas.changePasswordSchema.safeParse(data);
            expect(result.success).toBe(false);
            if (!result.success) {
                expect(result.error.flatten().fieldErrors.newPassword).toContain('validation.passwordSameAsOld');
            }
        });
    });
});
