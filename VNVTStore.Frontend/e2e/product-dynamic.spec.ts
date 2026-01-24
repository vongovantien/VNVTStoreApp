import { test, expect } from '@playwright/test';

test.describe('Dynamic Product Management', () => {

    test.beforeEach(async ({ page }) => {
        await page.goto('/login');
        await page.getByPlaceholder('email@example.com').fill('admin@vnvtstore.com');
        await page.getByPlaceholder('••••••••').fill('Password123!');
        await page.getByRole('button', { name: /Đăng nhập|Login/i }).click();
        await expect(page).toHaveURL(/.*\/admin/);
    });

    test('should create a product with dynamic specifications', async ({ page }) => {
        await page.goto('/admin/products');

        // Click Add Button
        await page.getByRole('button', { name: /Thêm sản phẩm|Add Product/i }).click();
        await expect(page).toHaveURL(/.*\/admin\/products\/new/);

        // Tab 1: Basic Info
        await page.getByLabel('Tên sản phẩm').fill('Sản phẩm Test Dynamic ' + Date.now());
        await page.locator('input[name="price"]').fill('150000');
        await page.locator('select[name="categoryCode"]').selectOption({ index: 1 });

        // Tab 2: Specs
        await page.getByRole('tab', { name: /Thông số|Specifications/i }).click();

        // Add SPEC (Button usually has a plus icon or specific text)
        await page.getByRole('button', { name: /Thêm thông số|Add specification/i }).click();
        await page.locator('input[placeholder="Tên thông số"]').last().fill('Công suất');
        await page.locator('input[placeholder="Giá trị"]').last().fill('50W');

        // Add LOGISTICS
        await page.getByRole('button', { name: /Thêm vận chuyển|Add logistics/i }).click();
        await page.locator('input[placeholder="Tên thông số"]').last().fill('Trọng lượng');
        await page.locator('input[placeholder="Giá trị"]').last().fill('2kg');

        // Submit
        await page.getByRole('button', { name: /Lưu|Save/i }).first().click();

        // Verify Success Toast or Redirect
        await expect(page.getByText(/thành công|success/i)).toBeVisible({ timeout: 15000 });
        await expect(page).toHaveURL(/.*\/admin\/products/);
    });

    test('should display dynamic specs on product detail page', async ({ page }) => {
        // Go to first product in list (assuming one exists from previous test or seed)
        await page.goto('/products');
        await page.locator('.product-card').first().click();

        // Check for Specs Table
        await expect(page.locator('table')).toBeVisible();
        await expect(page.getByText('Thông số kỹ thuật')).toBeVisible();
    });

});
