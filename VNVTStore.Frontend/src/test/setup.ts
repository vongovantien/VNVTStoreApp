
import '@testing-library/jest-dom';
import { cleanup } from '@testing-library/react';
import { afterEach, vi } from 'vitest';


// Runs a cleanup after each test case (e.g. clearing jsdom)
afterEach(() => {
    cleanup();

});

// Mock react-i18next
vi.mock('react-i18next', () => ({
    useTranslation: () => ({
        t: (key: string, ...args: any[]) => {
            let defaultValue = '';
            let options: any = {};

            if (args.length === 1) {
                if (typeof args[0] === 'string') defaultValue = args[0];
                else options = args[0];
            } else if (args.length === 2) {
                defaultValue = args[0];
                options = args[1];
            }

            let text = defaultValue || options.defaultValue || key;
            if (options) {
                Object.keys(options).forEach(k => {
                    text = text.replace(`{{${k}}}`, options[k]);
                });
            }
            return text;
        },
        i18n: {
            changeLanguage: vi.fn(),
            language: 'en',
        },
    }),
    initReactI18next: {
        type: '3rdParty',
        init: vi.fn(),
    },
    I18nextProvider: ({ children }: { children: React.ReactNode }) => children,
    Trans: ({ children }: { children: React.ReactNode }) => children,
    Translation: ({ children }: { children: (t: (key: string) => string, { i18n }: { i18n: unknown }) => React.ReactNode }) =>
        children((key: string) => key, { i18n: {} }),
}));
