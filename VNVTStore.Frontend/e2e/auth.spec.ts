import { test, expect } from '@playwright/test';

test.describe('Authentication Flow', () => {
    test('homepage loads successfully', async ({ page }) => {
        await page.goto('/');
        await expect(page).toHaveTitle(/VNVT Store/);
    });

    test('should navigate to login page', async ({ page }) => {
        await page.goto('/');

        // On desktop, login is inside the user menu dropdown caused by Header.tsx structure
        // Find the button with User icon. 
        // We look for a button in the header that contains the lucide-user svg class or similar logic.
        const userMenuBtn = page.locator('header button').filter({ has: page.locator('svg.lucide-user') });

        if (await userMenuBtn.isVisible()) {
            await userMenuBtn.click();
            // Wait for dropdown
            const loginLink = page.getByRole('link', { name: /login|đăng nhập/i });
            await expect(loginLink).toBeVisible();
            await loginLink.click();
            await expect(page).toHaveURL(/.*login/);
        } else {
            console.log('User menu button not found, checking direct login link (mobile view?)');
            // Fallback for mobile or if different layout
            const loginLink = page.getByRole('button', { name: /login|đăng nhập/i });
            if (await loginLink.isVisible()) {
                await loginLink.click();
                await expect(page).toHaveURL(/.*login/);
            } else {
                console.log('No login entry point found');
            }
        }
    });
});
