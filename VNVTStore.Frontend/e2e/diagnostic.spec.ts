import { test, expect } from '@playwright/test';

test('diagnostic login test', async ({ page }) => {
    // Listen for console logs
    page.on('console', msg => console.log('BROWSER LOG:', msg.text()));
    page.on('pageerror', err => console.log('BROWSER ERROR:', err.message));

    await page.goto('/login');

    // Fill login
    await page.fill('[data-testid="email-input"]', 'admin@vnvtstore.com');
    await page.fill('[data-testid="password-input"]', 'Admin@123');

    console.log('Clicking login button...');
    await page.click('[data-testid="login-button"]');

    console.log('Waiting for URL change...');
    try {
        await expect(page).toHaveURL(/.*\/admin/, { timeout: 10000 });
        console.log('SUCCESS: URL changed to admin');
    } catch {
        console.log('FAILURE: Current URL is', page.url());
        // Check for error message on page
        const errorMsg = await page.locator('.bg-red-50').textContent().catch(() => 'No visible error div');
        console.log('Error message on page:', errorMsg);
    }
});
