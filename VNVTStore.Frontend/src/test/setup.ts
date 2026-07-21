import '@testing-library/jest-dom';
import { cleanup } from '@testing-library/react';
import { afterEach, vi } from 'vitest';

// Runs a cleanup after each test case (e.g. clearing jsdom)
afterEach(() => {
    cleanup();
});

// Mock react-i18next synchronously so that initReactI18next is immediately available
vi.mock('react-i18next', () => ({
    useTranslation: () => ({
        t: (key: string, ...args: unknown[]) => {
            let defaultValue = '';
            let options: Record<string, unknown> = {};

            if (args.length === 1) {
                if (typeof args[0] === 'string') defaultValue = args[0];
                else options = args[0] as Record<string, unknown>;
            } else if (args.length === 2) {
                defaultValue = args[0] as string;
                options = args[1] as Record<string, unknown>;
            }

            let text = defaultValue || (options.defaultValue as string) || key;
            if (options) {
                Object.keys(options).forEach(k => {
                    text = text.replace(`{{${k}}}`, String(options[k]));
                });
            }
            return text;
        },
        i18n: {
            changeLanguage: vi.fn(),
            language: 'vi',
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

// Mock IntersectionObserver
class IntersectionObserverMock {
  readonly root: Element | null = null;
  readonly rootMargin: string = '';
  readonly thresholds: ReadonlyArray<number> = [];
  disconnect = vi.fn();
  observe = vi.fn();
  takeRecords = vi.fn();
  unobserve = vi.fn();
}

Object.defineProperty(window, 'IntersectionObserver', {
  writable: true,
  configurable: true,
  value: IntersectionObserverMock,
});

if (typeof global !== 'undefined') {
  Object.defineProperty(global, 'IntersectionObserver', {
    writable: true,
    configurable: true,
    value: IntersectionObserverMock,
  });
}
