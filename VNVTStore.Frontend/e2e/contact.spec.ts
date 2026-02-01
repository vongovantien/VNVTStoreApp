import { test, expect } from '@playwright/test';

test.describe('Contact Page', () => {
    test.beforeEach(async ({ page }) => {
        await page.goto('/contact');
    });

    test('should display validation errors when fields are empty', async ({ page }) => {
        // Click submit without filling anything
        await page.click('button[type="submit"]');

        // Check for validation messages
        // Since we localized it, we expect strings from vi.json for validation.required
        await expect(page.locator('p.text-red-500').first()).toBeVisible();

        // Check for specific labels using localized text
        const fullNameLabel = page.getByText('Họ và tên');
        await expect(fullNameLabel).toBeVisible();
    });

    test('should show error for invalid email', async ({ page }) => {
        await page.fill('input[name="fullName"]', 'Test User');
        await page.fill('input[name="email"]', 'invalid-email');
        await page.fill('input[name="subject"]', 'Support');
        await page.fill('textarea[name="message"]', 'Hello world');

        await page.click('button[type="submit"]');

        // Check for invalid email message (vi.json: validation.invalidEmail)
        await expect(page.getByText('Email không hợp lệ')).toBeVisible();
    });

    test('should submit successfully with valid data', async ({ page }) => {
        await page.fill('input[name="fullName"]', 'Nguyễn Văn A');
        await page.fill('input[name="email"]', 'vna@example.com');
        await page.fill('input[name="phone"]', '0901234567');
        await page.fill('input[name="subject"]', 'Hợp tác');
        await page.fill('textarea[name="message"]', 'Tôi muốn hợp tác kinh doanh.');

        await page.click('button[type="submit"]');

        // Check for success toast (vi.json: contactPage.success)
        await expect(page.getByText('Cảm ơn bạn! Chúng tôi sẽ liên hệ lại trong thời gian sớm nhất.')).toBeVisible();
    });
});
