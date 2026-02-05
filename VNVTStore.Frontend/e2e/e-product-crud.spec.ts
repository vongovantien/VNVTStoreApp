
import { test, expect } from '@playwright/test';
import path from 'path';

test.describe('Admin Product CRUD with Image Upload', () => {

    test.beforeEach(async ({ page }) => {
        try {
            console.error('DEBUG: Starting Login...');
            await page.goto('/login');
            await page.fill('[data-testid="email-input"]', 'admin@vnvtstore.com');
            await page.fill('[data-testid="password-input"]', 'Admin@123');
            await page.click('[data-testid="login-button"]');
            await page.waitForURL('**/admin/**', { timeout: 20000 });
            console.error('DEBUG: Login Successful');
        } catch (e) {
            console.error('DEBUG: Login Failed', e);
            await page.screenshot({ path: 'e2e/login-debug.png', fullPage: true });
            throw e;
        }
    });

    test('Should create product with image, update, and delete', async ({ page }) => {
        // Extended timeout for slow environments
        test.setTimeout(120000);

        const timestamp = Date.now();
        const prodName = `AutoProd_${timestamp}`;
        const updatedName = `AutoProd_${timestamp}_Updated`;

        console.error('DEBUG: Navigating to Products...');
        await page.goto('/admin/products');

        console.error('DEBUG: Waiting for Add Button...');
        await page.waitForSelector('table');
        // Try generic Add button selector
        const addBtn = page.locator('button').filter({ hasText: /New|Add|Thêm|Tạo/i }).first();
        await expect(addBtn).toBeVisible({ timeout: 30000 });
        await addBtn.click();

        console.error('DEBUG: Filling form...');
        await page.waitForSelector('input[name="name"]');
        await page.fill('input[name="name"]', prodName);
        await page.fill('input[name="price"]', '150000');
        await page.fill('input[name="stockQuantity"]', '100');

        // Description might be rich text or textarea
        await page.fill('textarea[name="description"]', 'Automation Test Description');

        console.error('DEBUG: Selecting Category...');
        // Use getByLabel since LazySelect binds label to button ID
        await page.getByLabel(/Category|Danh mục/i).first().click();

        console.error('DEBUG: Waiting for Option...');
        await expect(page.locator('div.absolute button').first()).toBeVisible({ timeout: 10000 });
        await page.locator('div.absolute button').first().click();

        console.error('DEBUG: Selecting Brand...');
        await page.getByLabel(/Brand|Thương hiệu/i).first().click();

        await expect(page.locator('div.absolute button').first()).toBeVisible({ timeout: 10000 });
        await page.locator('div.absolute button').first().click();

        console.error('DEBUG: Uploading Image...');
        const fileInput = page.locator('input[type="file"]');
        const imagePath = path.join(__dirname, '../src/assets/default-image.png');
        await fileInput.setInputFiles(imagePath);

        // Wait for upload preview
        await expect(page.locator('div.relative img').first()).toBeVisible({ timeout: 10000 });

        console.error('DEBUG: Saving...');
        await page.locator('button').filter({ hasText: /Save|Lưu/i }).click();

        console.error('DEBUG: Verifying creation...');
        await page.waitForSelector(`text=${prodName}`, { timeout: 30000 });
        expect(await page.locator(`text=${prodName}`).isVisible()).toBeTruthy();

        console.error('DEBUG: Verifying Image...');
        const row = page.locator('tr', { hasText: prodName });
        await expect(row.locator('img')).toBeVisible();

        // EDIT
        console.error('DEBUG: Editing...');
        const editBtn = row.locator('button').filter({ hasText: /Edit|Sửa/i });
        if (await editBtn.count() > 0) {
            await editBtn.first().click();
        } else {
            await row.locator('button[aria-label="Edit"]').click();
        }

        console.error('DEBUG: Updating Name...');
        await page.waitForSelector('input[name="name"]');
        // Check if image preview is visible in Edit form (verifies fix for missing image)
        await expect(page.locator('form img').first()).toBeVisible({ timeout: 5000 });
        console.log('Verified Image Preview in Edit Modal');

        // Update name
        await page.fill('input[name="name"]', updatedName);
        await page.locator('button').filter({ hasText: /Save|Lưu/i }).click();

        console.error('DEBUG: Verifying Update...');
        await page.waitForSelector(`text=${updatedName}`, { timeout: 30000 });
        expect(await page.locator(`text=${updatedName}`).isVisible()).toBeTruthy();

        // DELETE
        console.error('DEBUG: Deleting...');
        const updatedRow = page.locator('tr', { hasText: updatedName });

        const deleteBtn = updatedRow.locator('button').filter({ hasText: /Delete|Xóa/i });
        if (await deleteBtn.count() > 0) {
            await deleteBtn.click();
        } else {
            await updatedRow.locator('button[aria-label="Delete"]').click();
        }

        console.error('DEBUG: Confirming Delete...');
        await page.locator('button').filter({ hasText: /Delete|Xóa|Đồng ý/i }).click();

        console.error('DEBUG: Verifying Deletion...');
        await expect(page.locator(`text=${updatedName}`)).not.toBeVisible();
        console.error('DEBUG: Test Complete.');
    });
});
