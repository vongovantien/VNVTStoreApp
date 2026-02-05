import { test, expect } from '@playwright/test';

test.describe('i18n Language Switching', () => {

    test('should switch from Vietnamese to English and back', async ({ page }) => {
        await page.goto('/');

        // 1. Initial should be Vietnamese (default)
        // Check for common Vietnamese text like "Giỏ hàng" or "Sản phẩm"
        await expect(page.getByText('Sản phẩm')).toBeVisible();

        // 2. Click Language Toggle (Assuming there's a button or dropdown in Header)
        // We'll look for a button that likely holds the language label or icon
        const langToggle = page.getByTestId('lang-toggle');
        await langToggle.click();

        // Select English
        const enOption = page.getByRole('button', { name: /English/i });
        if (await enOption.isVisible()) {
            await enOption.click();
        } else {
            // Backup: click the toggle again if it's a direct toggle button
        }

        // 3. Verify English text
        await expect(page.getByText('Products')).toBeVisible();
        await expect(page.getByText('Sản phẩm')).not.toBeVisible();

        // 4. Switch back to Vietnamese
        await langToggle.click();
        await page.getByRole('button', { name: /Tiếng Việt/i }).click();

        // 5. Verify Vietnamese text again
        await expect(page.getByText('Sản phẩm')).toBeVisible();
    });
});
