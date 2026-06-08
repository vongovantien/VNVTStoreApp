import { defineConfig } from '@playwright/test';

export default defineConfig({
    testDir: './e2e/api',
    fullyParallel: true,
    reporter: 'list',
    use: {
        // Using port 5000 from launchSettings.json (http profile)
        baseURL: 'http://localhost:5000',
        extraHTTPHeaders: {
            'Accept': 'application/json',
        },
    },
});
