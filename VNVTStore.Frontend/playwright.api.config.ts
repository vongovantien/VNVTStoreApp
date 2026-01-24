import { defineConfig } from '@playwright/test';

export default defineConfig({
    testDir: './e2e/api',
    fullyParallel: true,
    reporter: 'list',
    use: {
        // Using port 5176 from launchSettings.json (http profile)
        baseURL: 'http://localhost:5176',
        extraHTTPHeaders: {
            'Accept': 'application/json',
        },
    },
});
