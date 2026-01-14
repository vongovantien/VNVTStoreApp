import { render, screen, fireEvent } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import { Select } from '../Select';
import userEvent from '@testing-library/user-event';

describe('Select', () => {
  const options = [
    { value: '1', label: 'Option 1' },
    { value: '2', label: 'Option 2' },
  ];

  it('renders correctly', () => {
    render(<Select options={options} />);
    expect(screen.getByRole('combobox')).toBeInTheDocument();
    expect(screen.getByRole('option', { name: 'Option 1' })).toBeInTheDocument();
  });

  it('handles change', async () => {
    const user = userEvent.setup();
    const handleChange = vi.fn();
    render(<Select options={options} onChange={handleChange} />);
    
    const select = screen.getByRole('combobox');
    await user.selectOptions(select, '2');
    
    expect(handleChange).toHaveBeenCalled();
    expect(select).toHaveValue('2');
  });

  it('shows error', () => {
    render(<Select options={options} error="Selection required" />);
    expect(screen.getByText('Selection required')).toBeInTheDocument();
  });
});
