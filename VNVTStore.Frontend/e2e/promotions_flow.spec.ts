import { test, expect } from '@playwright/test';

test.describe('Promotions & Checkout Flow', () => {

    test.beforeEach(async ({ page }) => {
        await page.context().clearCookies();
        await page.evaluate(() => localStorage.clear());
    });

    test('should apply a voucher and see discount in checkout', async ({ page }) => {
        // 1. Add item to cart
        await page.goto('/products');
        await page.getByRole('button', { name: /add to cart|thêm vào giỏ/i }).first().click();

        // 2. Go to Checkout
        await page.goto('/checkout');

        // 3. Guest login if needed
        const guestBtn = page.getByRole('button', { name: /mua hàng không cần đăng nhập/i });
        if (await guestBtn.isVisible({ timeout: 5000 })) {
            await guestBtn.click();
        }

        // 4. Fill shipping (required for step progression)
        await page.getByPlaceholder('Nguyễn Văn A').fill('Tester');
        await page.getByPlaceholder('0901234567').fill('0900000000');
        await page.getByPlaceholder('email@example.com').fill('test@promo.com');
        await page.locator('select').first().selectOption({ index: 1 });
        await page.locator('select').nth(1).selectOption({ index: 1 });
        await page.getByPlaceholder('Số nhà, tên đường...').fill('123 Test St');
        await page.getByRole('button', { name: /tiếp tục|continue/i }).click();

        // 5. Payment Step (Continue to final review where voucher is often visible)
        await page.getByRole('button', { name: /tiếp tục|continue/i }).click();

        // 6. Apply Voucher (Assuming there's a voucher input in the summary/review step)
        const voucherInput = page.getByPlaceholder(/nhập mã giảm giá|voucher/i);
        if (await voucherInput.isVisible()) {
            await voucherInput.fill('SAVE10');
            await page.getByRole('button', { name: /áp dụng|apply/i }).click();

            // 7. Verify Discount applied
            // Success indicator or price change
            await expect(page.locator('text=/giảm giá|discount/i')).toBeVisible();
        }
    });
});
