import { test, expect } from '@playwright/test';

test.describe('User Account Page', () => {
    test.beforeEach(async ({ page }) => {
        // Login before each test
        await page.goto('/login');
        await page.locator('input[placeholder="email@example.com"]').fill('user@example.com');
        await page.locator('input[placeholder="••••••••"]').fill('Password123!');
        await page.getByRole('button', { name: 'Đăng nhập' }).click();
        await expect(page).toHaveURL('/');
    });

    test('should load account profile', async ({ page }) => {
        await page.goto('/account');
        await expect(page).toHaveURL('/account');
        await expect(page.locator('text=Thông tin tài khoản')).toBeVisible();
        await expect(page.locator('input[name="fullName"]')).toBeVisible();
    });

    test('should navigate to orders', async ({ page }) => {
        await page.goto('/account/orders');
        await expect(page.locator('h2:has-text("Lịch sử đơn hàng")')).toBeVisible();
    });

    test('should navigate to addresses', async ({ page }) => {
        await page.goto('/account/addresses');
        await expect(page.locator('h2:has-text("Sổ địa chỉ")')).toBeVisible();
    });

    test('should navigate to settings/notifications', async ({ page }) => {
        await page.goto('/account/settings');
        await expect(page.locator('text=Cài đặt tài khoản')).toBeVisible();

        await page.goto('/account/notifications');
        await expect(page.locator('text=Thông báo của tôi')).toBeVisible();
    });
    test('should navigate to wishlist', async ({ page }) => {
        await page.goto('/account/wishlist');
        await expect(page.locator('text=Sản phẩm yêu thích')).toBeVisible();
    });

    test('should redirect /wishlist to /account/wishlist', async ({ page }) => {
        await page.goto('/wishlist');
        await expect(page).toHaveURL(/\/account\/wishlist/);
    });
});
