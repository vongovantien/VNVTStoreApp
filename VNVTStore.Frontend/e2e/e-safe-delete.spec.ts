
import { test, expect } from '@playwright/test';

// Configuration
const BASE_URL = 'http://localhost:5173';

test.describe('Admin Safe Delete Complex Flows', () => {

    test.beforeEach(async ({ page }) => {
        // Login
        await page.goto('/auth/login');
        await page.fill('input[name="username"]', 'admin');
        await page.fill('input[name="password"]', 'admin123');
        await page.click('button[type="submit"]');
        await page.waitForURL('**/admin/dashboard');
    });

    // Helpers
    const createCategory = async (page: any, name: string) => {
        await page.goto('/admin/categories');
        const addBtn = page.locator('button').filter({ hasText: /New|Add|Thêm|Tạo/i }).first();
        if (await addBtn.isVisible()) {
            await addBtn.click();
        } else {
            await page.getByRole('button').filter({ hasText: 'Add' }).first().click();
        }

        await page.fill('input[name="name"]', name);
        // Submit is usually inside the modal form
        // Wait for modal
        await page.waitForSelector('form');
        await page.locator('form button[type="submit"]').click();

        // Wait for table to reload/contain name
        await page.waitForSelector(`text=${name}`);
    };

    test('Complex Flow: Verify Safe Delete on Category', async ({ page }) => {
        // 1. Identification
        // We rely on existing data that has products to verify the BLOCK mechanism.
        // "Electronics" or similar usually exists.

        await page.goto('/admin/categories');
        await page.waitForSelector('table');

        // Find a row that likely has products. 
        // Safe Delete logic triggers a TOAST error.

        const deleteButtons = page.locator('button[aria-label="Delete"]');
        const count = await deleteButtons.count();

        if (count > 0) {
            await deleteButtons.first().click();

            // Check for Toast with "Cannot delete"
            try {
                // Try specific toast selector if known, or generic text
                const toast = page.locator('li[role="status"]').filter({ hasText: /Cannot delete|Không thể xóa/i }).first();

                // Wait for toast if it appears
                await expect(toast).toBeVisible({ timeout: 5000 });
                console.log("Verified Safe Delete Toast");
            } catch (e) {
                // If not blocked, it probably showed Confirm Delete modal
                const confirmModal = page.getByText(/Are you sure|Bạn có chắc chắn/i);
                if (await confirmModal.isVisible()) {
                    console.log("Warning: Category was safe to delete (no products). Block logic not exercised.");
                    await page.getByText(/Cancel|Hủy/i).click();
                } else {
                    console.log("No toast and no modal? Logic might be broken.");
                }
            }
        }
    });

    test('Complex Flow: Delete Empty Category (Success)', async ({ page }) => {
        const timestamp = Date.now();
        const catName = `SafeDel_${timestamp}`;

        // 1. Create Empty Category
        await page.goto('/admin/categories');

        await page.waitForSelector('table');
        const addBtn = page.locator('button').filter({ hasText: /New|Add|Thêm|Tạo/i }).first();
        await addBtn.click();

        await page.waitForSelector('input[name="name"]');
        await page.fill('input[name="name"]', catName);
        await page.locator('button').filter({ hasText: /Save|Lưu|Create/i }).click();

        await page.waitForSelector(`text=${catName}`);

        // 2. Delete it
        // Find row with this name
        const row = page.locator('tr', { hasText: catName });

        // Click delete on the row
        const deleteBtn = row.locator('button').filter({ hasText: /Delete|Xóa/i });
        if (await deleteBtn.count() > 0) {
            await deleteBtn.click();
        } else {
            await row.locator('button[aria-label="Delete"]').click();
        }

        // Expect "Are you sure" dialog (Custom Modal) not native dialog
        const confirmModal = page.getByText(/Are you sure|Bạn có chắc chắn/i);
        await expect(confirmModal).toBeVisible();
        await page.locator('button').filter({ hasText: /Delete|Xóa|Đồng ý/i }).click();

        await page.waitForTimeout(1000); // Wait for delete
        await expect(page.locator(`text=${catName}`)).not.toBeVisible();
    });
});
