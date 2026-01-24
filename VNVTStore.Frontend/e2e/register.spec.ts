import { test, expect } from '@playwright/test';

test.describe('Registration Logic', () => {
    test('should validate empty fields', async ({ page }) => {
        await page.goto('/register');

        // Submit empty form
        await page.getByRole('button', { name: /đăng ký/i }).click();

        // Check for specific validation message to be sure
        // "Email không hợp lệ" is from regex check on email/empty
        // We just verify we are still on the register page
        await expect(page).toHaveURL(/.*register/);

        // Optional: Check if validation prevented submission
        // If validation prevented, we stay on page.
    });

    test('should register successfully', async ({ page }) => {
        await page.goto('/register');

        const randomId = Date.now();
        const email = `testuser${randomId}@example.com`;

        await page.getByPlaceholder('Nguyễn Văn A').fill('Auto Tester');
        await page.getByPlaceholder('email@example.com').fill(email);
        await page.getByPlaceholder('0901234567').fill('0901234567');

        // Password
        await page.getByPlaceholder('••••••••').first().fill('Password123!');
        await page.getByPlaceholder('••••••••').last().fill('Password123!'); // Confirm pass

        // Check terms
        // It's a checkbox: <input type="checkbox" ... />
        await page.locator('input[type="checkbox"]').check();

        // Submit
        await page.getByRole('button', { name: /đăng ký/i }).click();

        // Verify success redirect to login
        await expect(page).toHaveURL(/.*login/);

        // Verify toast?
        // await expect(page.getByText('Đăng ký thành công')).toBeVisible();
    });
});
