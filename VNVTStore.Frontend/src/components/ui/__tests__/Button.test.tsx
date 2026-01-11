import { render, screen, fireEvent } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import { Button } from '@/components/ui';

describe('Button Component', () => {
    it('renders correctly', () => {
        render(<Button>Click me</Button>);
        expect(screen.getByText('Click me')).toBeInTheDocument();
    });

    it('handles click events', () => {
        const handleClick = vi.fn();
        render(<Button onClick={handleClick}>Click me</Button>);

        fireEvent.click(screen.getByText('Click me'));
        expect(handleClick).toHaveBeenCalledTimes(1);
    });

    it('renders loading state', () => {
        render(<Button isLoading loadingText="Loading...">Click me</Button>);
        expect(screen.getByText('Loading...')).toBeInTheDocument();
        expect(screen.getByRole('button')).toBeDisabled();
    });

    it('renders variants correctly', () => {
        const { container } = render(<Button variant="outline">Outline</Button>);
        // Tailwind border class check
        expect(container.firstChild).toHaveClass('border-2');
    });
});
