import { test, expect } from '@playwright/test';

test.describe('Admin Functionality', () => {

    test.beforeEach(async ({ page }) => {
        // 1. Clear state fully
        await page.context().clearCookies();
        await page.evaluate(() => localStorage.clear());
        await page.evaluate(() => sessionStorage.clear());

        // 2. Login as Admin
        await page.goto('/login');
        // Use 'admin' username directly to rule out email match issues
        await page.getByPlaceholder('email@example.com').fill('admin');
        await page.getByPlaceholder('••••••••').fill('Password123!');

        await page.getByText('Ghi nhớ đăng nhập').click();
        await page.getByRole('button', { name: /^Đăng nhập$/i }).click();

        // 3. Wait for Successful Auth Markers
        await expect(page).toHaveURL(/.*admin/, { timeout: 15000 });
        await expect(page.getByText('VNVT Admin')).toBeVisible({ timeout: 15000 });
    });

    test('should display Dashboard statistics', async ({ page }) => {
        const statCards = page.locator('.grid >> .bg-primary');
        await expect(statCards.first()).toBeVisible({ timeout: 15000 });

        await expect(page.getByText(/Doanh thu|Revenue/i)).toBeVisible();
        await expect(page.getByText(/Đơn hàng|Orders/i)).toBeVisible();
        await expect(page.getByText(/Sản phẩm|Products/i)).toBeVisible();
    });

    test('should manage products module', async ({ page }) => {
        await page.goto('/admin/products');
        await expect(page.getByRole('heading', { name: /Sản phẩm|Products/i })).toBeVisible();
        await expect(page.locator('table')).toBeVisible();
    });

    test('should manage categories module', async ({ page }) => {
        await page.goto('/admin/categories');
        await expect(page.getByRole('heading', { name: /Danh mục|Categories/i })).toBeVisible();
        await expect(page.locator('table')).toBeVisible();
    });

    test('should manage suppliers module', async ({ page }) => {
        await page.goto('/admin/suppliers');
        await expect(page.getByRole('heading', { name: /Nhà cung cấp|Suppliers/i })).toBeVisible();
        await expect(page.locator('table')).toBeVisible();
    });

    test('should manage customers module', async ({ page }) => {
        await page.goto('/admin/customers');
        await expect(page.getByRole('heading', { name: /Khách hàng|Customers/i })).toBeVisible();
        await expect(page.locator('table')).toBeVisible();
    });

    test('should manage orders module', async ({ page }) => {
        await page.goto('/admin/orders');
        await expect(page.getByRole('heading', { name: /Đơn hàng|Orders/i })).toBeVisible();
        await expect(page.locator('table')).toBeVisible();
    });

    test('should manage promotions module', async ({ page }) => {
        await page.goto('/admin/promotions');
        await expect(page.getByRole('heading', { name: /Khuyến mãi|Promotions/i })).toBeVisible();
    });

    test('should manage banners module', async ({ page }) => {
        await page.goto('/admin/banners');
        await expect(page.getByRole('heading', { name: /Banners|Quảng cáo/i })).toBeVisible();
    });

    test('should manage quote requests module', async ({ page }) => {
        await page.goto('/admin/quotes');
        await expect(page.getByRole('heading', { name: /Yêu cầu báo giá|Quotes/i })).toBeVisible();
    });

    test('should view settings module', async ({ page }) => {
        await page.goto('/admin/settings');
        await expect(page.getByRole('heading', { name: /Cài đặt|Settings/i })).toBeVisible();
    });

    test('should logout correctly', async ({ page }) => {
        // User profile menu in header
        await page.locator('header').getByRole('button').last().click();
        const logoutBtn = page.getByText(/Đăng xuất|Sign out/i);
        await expect(logoutBtn).toBeVisible();
        await logoutBtn.click();

        // Confirm if asked
        const confirmBtn = page.getByRole('button', { name: /đăng xuất|logout/i }).last();
        if (await confirmBtn.isVisible()) {
            await confirmBtn.click();
        }

        await expect(page).toHaveURL(/.*login/);
    });

});
