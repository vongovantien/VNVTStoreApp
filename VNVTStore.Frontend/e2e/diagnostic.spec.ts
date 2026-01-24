import { test, expect } from '@playwright/test';

test('diagnostic login test', async ({ page }) => {
    // Listen for console logs
    page.on('console', msg => console.log('BROWSER LOG:', msg.text()));
    page.on('pageerror', err => console.log('BROWSER ERROR:', err.message));

    await page.goto('/login');

    // Fill login
    await page.getByPlaceholder('email@example.com').fill('admin@vnvt.com');
    await page.getByPlaceholder('••••••••').fill('Password123!');

    console.log('Clicking login button...');
    await page.getByRole('button', { name: /Đăng nhập|Login/i }).click();

    console.log('Waiting for URL change...');
    try {
        await expect(page).toHaveURL(/.*\/admin/, { timeout: 10000 });
        console.log('SUCCESS: URL changed to admin');
    } catch (e) {
        console.log('FAILURE: Current URL is', page.url());
        // Check for error message on page
        const errorMsg = await page.locator('.bg-red-50').textContent().catch(() => 'No visible error div');
        console.log('Error message on page:', errorMsg);
    }
});
