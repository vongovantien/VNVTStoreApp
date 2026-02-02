import { test, expect } from '@playwright/test';

test.describe('Frontend Page Loading', () => {
    test('should load the home page', async ({ page }) => {
        await page.goto('http://localhost:5173');
        // Check for a common element like "VNVT Store" or a hero section
        const title = await page.title();
        expect(title).toBeTruthy();
    });

    test('should load the products page', async ({ page }) => {
        await page.goto('http://localhost:5173/products');
        // Expect some product cards to be present or at least the heading
        await expect(page.locator('h1, h2')).toContainText(/Product/i);
    });

    test('should load the login page', async ({ page }) => {
        await page.goto('http://localhost:5173/login');
        await expect(page.locator('form')).toBeVisible();
        await expect(page.locator('input[name="username"]')).toBeVisible();
    });
});
