export default {
    content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
    darkMode: ['class', '[data-theme="dark"]'],
    theme: {
        extend: {
            colors: {
                primary: 'var(--color-primary)',
                secondary: 'var(--color-secondary)',
                accent: {
                    DEFAULT: 'var(--accent-primary)',
                    hover: 'var(--accent-hover)',
                    foreground: 'var(--accent-foreground)',
                },
                bg: {
                    primary: 'var(--bg-primary)',
                    secondary: 'var(--bg-secondary)',
                    tertiary: 'var(--bg-tertiary)',
                    hover: 'var(--bg-hover)',
                },
                text: {
                    primary: 'var(--text-primary)',
                    secondary: 'var(--text-secondary)',
                    tertiary: 'var(--text-tertiary)',
                },
                border: 'var(--border-color)',
                error: 'var(--error)',
                success: {
                    DEFAULT: 'var(--success)',
                    hover: 'var(--success-hover)',
                    soft: 'var(--success-soft)',
                },
                warning: {
                    DEFAULT: 'var(--warning)',
                    hover: 'var(--warning-hover)',
                    soft: 'var(--warning-soft)',
                },
                info: {
                    DEFAULT: 'var(--info)',
                    hover: 'var(--info-hover)',
                    soft: 'var(--info-soft)',
                },
            },
            fontFamily: {
                sans: ['Inter', 'system-ui', 'sans-serif'],
            }
        },
    },
    plugins: [],
};
