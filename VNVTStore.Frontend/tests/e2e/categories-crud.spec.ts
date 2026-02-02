import { test, expect } from '@playwright/test';

test.describe('Admin Categories CRUD', () => {
    test.beforeEach(async ({ page }) => {
        // Add network and console logging
        page.on('console', msg => console.log('BROWSER CONSOLE:', msg.text()));
        page.on('requestfailed', request => console.log('REQUEST FAILED:', request.url(), request.failure()?.errorText));
        page.on('response', response => {
            if (response.status() >= 400) {
                console.log('ERROR RESPONSE:', response.status(), response.url());
            }
        });

        console.log('--- STARTING LOGIN FLOW ---');
        await page.goto('/login');

        console.log('Filling Email: admin@vnvtstore.com');
        await page.getByTestId('login-email').fill('admin@vnvtstore.com');

        console.log('Filling Password');
        await page.getByTestId('login-password').fill('Admin@123');

        console.log('Clicking Login button');
        await page.getByTestId('login-submit').click();

        try {
            console.log('Waiting for redirect to /admin...');
            // Wait for navigation and URL change - allow more time for slow backend
            await page.waitForURL(/.*admin/, { timeout: 30000, waitUntil: 'load' });
            console.log('Login successful, navigated to admin.');
        } catch (e) {
            console.error('Login failed or redirect timed out.');
            await page.screenshot({ path: 'tests/e2e/login_failure_diagnostics.png' });
            throw e;
        }

        console.log('Navigating to Categories page...');
        await page.goto('/admin/categories', { waitUntil: 'networkidle' });

        // Wait for the admin layout or categories heading to be definitely visible
        // This ensures the data has at least started loading
        console.log('Waiting for Categories heading...');
        const heading = page.getByRole('heading', { name: /Categories|Danh mục/i });
        await heading.waitFor({ state: 'visible', timeout: 30000 });

        console.log('Navigated to Categories page and heading is visible.');
    });

    test('should complete a full Category CRUD cycle', async ({ page }) => {
        const timestamp = Date.now();
        const categoryName = `Test Category ${timestamp}`;
        const updatedName = `Updated Category ${timestamp}`;

        // --- CREATE ---
        // Click "Add New" button using stable testid from AdminToolbar
        await page.getByTestId('toolbar-add').click();

        // Fill form
        await page.getByTestId('field-name').fill(categoryName);
        await page.getByTestId('field-description').fill('This is a test category created by E2E automation.');

        // Intercept create request
        const createPromise = page.waitForResponse(response =>
            response.url().includes('/api/v1/categories') &&
            !response.url().includes('/search') && // Avoid catching search results
            response.request().method() === 'POST'
        );

        // Click "Create" button in Modal
        await page.getByTestId('form-submit').click();

        // Wait for create to finish
        const createResponse = await createPromise;
        expect([200, 201]).toContain(createResponse.status()); // Backend returns 200 or 201
        const createResult = await createResponse.json();
        console.log('Category create full response:', JSON.stringify(createResult));

        // Robust extraction: backend ApiResponse has 'data' (camelCase) or 'Data' (PascalCase) 
        // depending on serialization, and DTO has 'code' or 'Code'.
        const categoryData = createResult.data || createResult.Data;
        const categoryCode = categoryData?.code || categoryData?.Code;

        console.log(`Extracted Category Code: ${categoryCode}`);
        if (!categoryCode) {
            throw new Error(`Failed to extract categoryCode from response: ${JSON.stringify(createResult)}`);
        }

        // Verify Success Toast or list presence
        await expect(page.getByText(/Created successfully!|Tạo mới thành công/i).first()).toBeVisible();

        // Use Search to find the category specifically
        await page.getByTestId('toolbar-search').click();
        await page.waitForTimeout(500); // Wait for focus/animation
        await page.getByPlaceholder(/Search|Tìm kiếm/i).first().fill(categoryName);
        await page.waitForTimeout(1000); // Wait for debounce/fetch

        // --- UPDATE ---
        // Find by data-row-key which we now know from create response
        const row = page.locator(`tr[data-row-key="${categoryCode}"]`);
        await row.getByTestId('row-action-edit').click();

        // Intercept update request
        const updatePromise = page.waitForResponse(response =>
            response.url().includes(`/api/v1/categories/${categoryCode}`) && response.request().method() === 'PUT'
        );

        // Change name
        await page.getByTestId('field-name').fill(updatedName);
        await page.getByTestId('form-submit').click();

        await updatePromise;
        await expect(page.getByText(/Updated successfully!|Cập nhật thành công/i).first()).toBeVisible();

        // --- DEACTIVATE ---
        // Backend requires item to be inactive before deletion
        console.log('Deactivating category before deletion...');
        // Find row again to be safe after refresh
        const rowToDeactivate = page.locator(`tr[data-row-key="${categoryCode}"]`);
        await rowToDeactivate.getByTestId('row-action-edit').click();

        // Wait for modal and toggle off
        await page.getByTestId('field-isActive').click();
        await page.getByTestId('form-submit').click();
        await expect(page.getByText(/Updated successfully!|Cập nhật thành công/i).first()).toBeVisible();

        // --- DELETE ---
        console.log('Proceeding to deletion...');
        const rowToDelete = page.locator(`tr[data-row-key="${categoryCode}"]`);
        await rowToDelete.getByTestId('row-action-delete').click();

        // Confirm in Dialog
        console.log('Confirming deletion in dialog...');
        await page.getByTestId('confirm-dialog-confirm').click();

        // Verify item removed (search still active)
        await expect(page.getByText(updatedName)).not.toBeVisible();
        await expect(page.getByText(/Deleted successfully!|Xóa thành công/i).first()).toBeVisible();
    });
});
