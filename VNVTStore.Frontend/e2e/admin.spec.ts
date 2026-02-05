import { test, expect } from '@playwright/test';

test.describe('Admin Functionality', () => {



    test.beforeEach(async ({ page }) => {
        // 1. Navigate first to ensure valid context for Storage access
        await page.goto('/login');

        // 2. Clear state fully
        await page.context().clearCookies();
        await page.evaluate(() => localStorage.clear());
        await page.evaluate(() => sessionStorage.clear());
        // Wait for inputs to be ready
        await page.fill('[data-testid="email-input"]', 'admin@vnvtstore.com');
        await page.fill('[data-testid="password-input"]', 'Admin@123');

        // 3. Submit
        await page.click('[data-testid="login-button"]');

        // 4. Wait for redirection to Admin Dashboard
        // Admin usually redirects to /admin
        // Increase timeout for potentially slow backend/seed
        await expect(page).toHaveURL(/.*\/admin/, { timeout: 40000 });

        // 5. Verify Admin Sidebar presence which confirms successful load
        await expect(page.locator('aside')).toBeVisible({ timeout: 30000 });
    });

    test('should display Dashboard statistics', async ({ page }) => {
        // Wait for stats to load
        await expect(page.getByText(/Doanh thu|Revenue/i)).toBeVisible({ timeout: 20000 });
        await expect(page.getByText(/Đơn hàng|Orders/i)).toBeVisible();
        await expect(page.getByText(/Sản phẩm|Products/i)).toBeVisible();
    });

    test('should manage products module', async ({ page }) => {
        const link = page.locator('aside a[href="/admin/products"]');
        await expect(link).toBeVisible();
        await link.click();

        await expect(page).toHaveURL(/.*\/admin\/products/);
        await expect(page.locator('table')).toBeVisible({ timeout: 15000 });
    });

    test('should manage categories module', async ({ page }) => {
        const link = page.locator('aside a[href="/admin/categories"]');
        await expect(link).toBeVisible();
        await link.click();

        await expect(page).toHaveURL(/.*\/admin\/categories/);
        await expect(page.locator('table')).toBeVisible({ timeout: 10000 });
    });

    test('should manage suppliers module', async ({ page }) => {
        const link = page.locator('aside a[href="/admin/suppliers"]');
        await expect(link).toBeVisible();
        await link.click();

        await expect(page).toHaveURL(/.*\/admin\/suppliers/);
        await expect(page.locator('table')).toBeVisible({ timeout: 10000 });
    });

    test('should manage customers module', async ({ page }) => {
        const link = page.locator('aside a[href="/admin/customers"]');
        await expect(link).toBeVisible();
        await link.click();

        await expect(page).toHaveURL(/.*\/admin\/customers/);
        await expect(page.locator('table')).toBeVisible({ timeout: 10000 });
    });

    test('should manage orders module', async ({ page }) => {
        const link = page.locator('aside a[href="/admin/orders"]');
        await expect(link).toBeVisible();
        await link.click();

        await expect(page).toHaveURL(/.*\/admin\/orders/);
        await expect(page.locator('table')).toBeVisible({ timeout: 10000 }); // some fail if empty
    });

    test('should manage promotions module', async ({ page }) => {
        const link = page.locator('aside a[href="/admin/promotions"]');
        await expect(link).toBeVisible();
        await link.click();

        await expect(page).toHaveURL(/.*\/admin\/promotions/);
        await expect(page.locator('table')).toBeVisible({ timeout: 10000 });
    });

    test('should manage banners module', async ({ page }) => {
        const link = page.locator('aside a[href="/admin/banners"]');
        await expect(link).toBeVisible();
        await link.click();

        await expect(page).toHaveURL(/.*\/admin\/banners/);
        await expect(page.locator('table')).toBeVisible({ timeout: 10000 });
    });

    test('should manage quote requests module', async ({ page }) => {
        const link = page.locator('aside a[href="/admin/quotes"]');
        await expect(link).toBeVisible();
        await link.click();
        await expect(page).toHaveURL(/.*\/admin\/quotes/);
    });

    test('should view settings module', async ({ page }) => {
        const link = page.locator('aside a[href="/admin/settings"]');
        await expect(link).toBeVisible();
        await link.click();
        await expect(page).toHaveURL(/.*\/admin\/settings/);
    });

    test('should logout correctly', async ({ page }) => {
        // User profile menu in header (it's a button with user email)
        await page.locator('header').getByRole('button').last().click();

        // The Sign out button is in the portal dropdown
        const logoutBtn = page.getByText(/Sign out|Đăng xuất/i).last();
        await expect(logoutBtn).toBeVisible();
        await logoutBtn.click();

        // Confirm Dialog
        const confirmBtn = page.getByRole('button', { name: /Đăng xuất|Logout/i }).filter({ hasText: /Đăng xuất|Logout/i });
        await expect(confirmBtn).toBeVisible();
        await confirmBtn.click();

        await expect(page).toHaveURL(/.*login/);
    });

});
