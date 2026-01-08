import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { Button } from '../Button';

describe('Button', () => {
    it('renders children correctly', () => {
        render(<Button>Click me</Button>);
        expect(screen.getByRole('button', { name: /click me/i })).toBeInTheDocument();
    });

    it('handles onClick event', () => {
        const handleClick = vi.fn();
        render(<Button onClick={handleClick}>Click me</Button>);
        
        fireEvent.click(screen.getByRole('button', { name: /click me/i }));
        expect(handleClick).toHaveBeenCalledTimes(1);
    });

    it('renders loading state', () => {
        render(<Button isLoading loadingText="Loading...">Click me</Button>);
        
        expect(screen.getByRole('button')).toBeDisabled();
        expect(screen.getByText('Loading...')).toBeInTheDocument();
        // Should not show children text when loading if loadingText is provided
        expect(screen.queryByText('Click me')).not.toBeInTheDocument();
    });

    it('renders disabled state', () => {
        render(<Button disabled>Disabled</Button>);
        expect(screen.getByRole('button', { name: /disabled/i })).toBeDisabled();
    });

    it('applies fullWidth class', () => {
        render(<Button fullWidth>Full Width</Button>);
        const button = screen.getByRole('button', { name: /full width/i });
        expect(button.className).toContain('w-full');
    });

    it('applies variant classes', () => {
        render(<Button variant="danger">Danger</Button>);
        const button = screen.getByRole('button', { name: /danger/i });
        expect(button.className).toContain('bg-red-500');
    });
});
