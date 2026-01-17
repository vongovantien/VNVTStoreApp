
import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { Button } from '@/components/ui';

describe('Button Component', () => {
  it('renders correctly', () => {
    render(<Button>Click me</Button>);
    expect(screen.getByRole('button', { name: /click me/i })).toBeInTheDocument();
  });

  it('shows loading state', () => {
    render(<Button isLoading loadingText="Loading...">Click me</Button>);
    expect(screen.getByText(/loading.../i)).toBeInTheDocument();
    expect(screen.getByRole('button')).toBeDisabled();
  });
});
