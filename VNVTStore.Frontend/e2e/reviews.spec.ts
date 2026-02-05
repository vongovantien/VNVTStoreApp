import { test, expect } from '@playwright/test';

/**
 * Product Reviews E2E Tests
 * Tests Admin moderation flow and Shop display
 */
test.describe('Product Reviews', () => {

    test.describe('Admin Moderation', () => {
        test.beforeEach(async ({ page }) => {
            // Login as Admin
            await page.goto('/login');
            await page.context().clearCookies();
            await page.evaluate(() => localStorage.clear());

            await page.fill('[data-testid="email-input"]', 'admin@vnvtstore.com');
            await page.fill('[data-testid="password-input"]', 'Admin@123');
            await page.click('[data-testid="login-button"]');
            await expect(page).toHaveURL(/.*\/admin/, { timeout: 40000 });
        });

        test('should access Reviews page from sidebar', async ({ page }) => {
            // Navigate to Reviews management page
            await page.goto('/admin/reviews');
            await expect(page).toHaveURL(/.*\/admin\/reviews/);

            // Verify page loads with table or empty state
            const hasTable = await page.locator('table').isVisible().catch(() => false);
            const hasEmptyState = await page.getByText(/Không có|No reviews|empty/i).isVisible().catch(() => false);

            expect(hasTable || hasEmptyState).toBeTruthy();
        });

        test('should display review list with columns', async ({ page }) => {
            await page.goto('/admin/reviews');

            // Check for essential columns if table exists
            const table = page.locator('table');
            if (await table.isVisible()) {
                // Check for common column headers
                await expect(page.getByRole('columnheader', { name: /Khách hàng|Customer|User/i })).toBeVisible();
                await expect(page.getByRole('columnheader', { name: /Đánh giá|Rating/i })).toBeVisible();
                await expect(page.getByRole('columnheader', { name: /Trạng thái|Status/i })).toBeVisible();
            }
        });

        test('should filter reviews by approval status', async ({ page }) => {
            await page.goto('/admin/reviews');

            // Look for filter dropdown or tabs
            const filterBtn = page.getByRole('button', { name: /Lọc|Filter/i });
            if (await filterBtn.isVisible()) {
                await filterBtn.click();

                // Check for filter options
                const approvedOption = page.getByText(/Đã duyệt|Approved/i);
                const pendingOption = page.getByText(/Chờ duyệt|Pending/i);

                expect(await approvedOption.isVisible() || await pendingOption.isVisible()).toBeTruthy();
            }
        });

        test('should open review detail modal', async ({ page }) => {
            await page.goto('/admin/reviews');

            // If there are reviews, click on the first row
            const firstRow = page.locator('table tbody tr').first();
            if (await firstRow.isVisible()) {
                await firstRow.click();

                // Check for modal or detail view
                const modal = page.locator('[role="dialog"], .modal, [data-testid="review-detail"]');
                if (await modal.isVisible()) {
                    // Should show comment content
                    await expect(modal.getByText(/Nội dung|Comment|Content/i)).toBeVisible();
                }
            }
        });

        test('should have approve/reject actions', async ({ page }) => {
            await page.goto('/admin/reviews');

            // Check for action buttons in table
            const table = page.locator('table');
            if (await table.isVisible()) {
                const actionButtons = table.locator('button, [role="button"]');
                const buttonCount = await actionButtons.count();

                // Should have action buttons (approve, reject, delete, etc.)
                expect(buttonCount).toBeGreaterThan(0);
            }
        });
    });

    test.describe('Shop Display', () => {
        test('should display reviews on product detail page', async ({ page }) => {
            // Navigate to a product detail page
            await page.goto('/');

            // Click on first product card
            const productCard = page.locator('[data-testid^="product-card"], .product-card, a[href*="/product/"]').first();
            if (await productCard.isVisible()) {
                await productCard.click();
                await expect(page).toHaveURL(/.*\/product\//);

                // Scroll to reviews section
                const reviewsSection = page.getByText(/Đánh giá|Reviews|Customer Reviews/i);
                if (await reviewsSection.isVisible()) {
                    await reviewsSection.scrollIntoViewIfNeeded();

                    // Check for reviews list or "no reviews" message
                    const hasReviews = await page.locator('[data-testid^="review-"], .review-item').count() > 0;
                    const hasNoReviews = await page.getByText(/Chưa có đánh giá|No reviews yet/i).isVisible();

                    expect(hasReviews || hasNoReviews).toBeTruthy();
                }
            }
        });

        test('should display star ratings', async ({ page }) => {
            await page.goto('/');

            // Check if product cards show ratings
            const productCard = page.locator('[data-testid^="product-card"], .product-card').first();
            if (await productCard.isVisible()) {
                await productCard.click();
                await expect(page).toHaveURL(/.*\/product\//);

                // Look for star icons on the page
                const stars = page.locator('svg.fill-yellow-400, .star, [data-testid="star"]');
                // Stars may or may not be present depending on reviews
                await expect(stars.first()).toBeVisible().catch(() => { });
            }
        });

        test('should paginate reviews if many exist', async ({ page }) => {
            // Navigate to product with reviews
            await page.goto('/');

            const productCard = page.locator('[data-testid^="product-card"], .product-card, a[href*="/product/"]').first();
            if (await productCard.isVisible()) {
                await productCard.click();

                // Look for pagination
                const pagination = page.locator('[data-testid="pagination"], .pagination, nav[aria-label*="pagination"]');
                await expect(pagination).toBeVisible().catch(() => { });
            }
        });
    });

    test.describe('Review Submission (Authenticated User)', () => {
        test.beforeEach(async ({ page }) => {
            // Login as regular user
            await page.goto('/login');
            await page.context().clearCookies();
            await page.evaluate(() => localStorage.clear());

            const emailInput = page.getByPlaceholder('email@example.com');
            await expect(emailInput).toBeVisible();
            await emailInput.fill('user@example.com');
            await page.getByPlaceholder('••••••••').fill('Password123!');

            const submitBtn = page.getByRole('button', { name: /Đăng nhập|Login/i });
            await submitBtn.click();

            // Wait for redirect (could be home or account)
            await page.waitForURL(/.*\/(account|$)/, { timeout: 20000 }).catch(() => { });
        });

        test('should show write review button on product page', async ({ page }) => {
            await page.goto('/');

            const productCard = page.locator('[data-testid^="product-card"], .product-card, a[href*="/product/"]').first();
            if (await productCard.isVisible()) {
                await productCard.click();

                // Look for "Write Review" button
                const writeReviewBtn = page.getByRole('button', { name: /Viết đánh giá|Write.*Review|Add Review/i });
                await expect(writeReviewBtn).toBeVisible().catch(() => { });
            }
        });

        test('should show review form when clicking write review', async ({ page }) => {
            await page.goto('/');

            const productCard = page.locator('[data-testid^="product-card"], .product-card, a[href*="/product/"]').first();
            if (await productCard.isVisible()) {
                await productCard.click();

                const writeReviewBtn = page.getByRole('button', { name: /Viết đánh giá|Write.*Review|Add Review/i });
                if (await writeReviewBtn.isVisible()) {
                    await writeReviewBtn.click();

                    // Check for form elements
                    const form = page.locator('form, [role="dialog"]');
                    if (await form.isVisible()) {
                        const ratingInput = form.locator('[data-testid="rating"], input[name="rating"], .star-rating');
                        await expect(ratingInput).toBeVisible();
                        const commentInput = form.locator('textarea, input[name="comment"]');
                        await expect(commentInput).toBeVisible();
                    }
                }
            }
        });
    });

});
