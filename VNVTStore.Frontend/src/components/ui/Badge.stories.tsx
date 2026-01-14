import type { Meta, StoryObj } from '@storybook/react';
import { Badge } from './Badge';
import { Star, X } from 'lucide-react';

const meta = {
  title: 'UI/Badge',
  component: Badge,
  parameters: {
    layout: 'centered',
  },
  tags: ['autodocs'],
  argTypes: {
    variant: { control: 'select', options: ['solid', 'soft', 'outline'] },
    color: { control: 'select', options: ['default', 'primary', 'secondary', 'success', 'warning', 'error', 'info'] },
    size: { control: 'select', options: ['sm', 'md', 'lg'] },
    rounded: { control: 'boolean' },
    dot: { control: 'boolean' },
    closable: { control: 'boolean' },
  },
} satisfies Meta<typeof Badge>;

export default meta;
type Story = StoryObj<typeof meta>;

export const Default: Story = {
  args: {
    children: 'Badge',
    color: 'default',
  },
};

export const Primary: Story = {
  args: {
    children: 'Primary',
    color: 'primary',
  },
};

export const Secondary: Story = {
  args: {
    children: 'Secondary',
    color: 'secondary',
  },
};

export const Success: Story = {
  args: {
    children: 'Success',
    color: 'success',
  },
};

export const Warning: Story = {
  args: {
    children: 'Warning',
    color: 'warning',
  },
};

export const Error: Story = {
  args: {
    children: 'Error',
    color: 'error',
  },
};

export const Info: Story = {
  args: {
    children: 'Info',
    color: 'info',
  },
};

export const Soft: Story = {
  args: {
    children: 'Soft Variant',
    variant: 'soft',
    color: 'primary',
  },
};

export const Outline: Story = {
  args: {
    children: 'Outline Variant',
    variant: 'outline',
    color: 'primary',
  },
};

export const WithIcon: Story = {
  args: {
    children: 'Rating',
    color: 'warning',
    leftIcon: <Star size={12} fill="currentColor" />,
  },
};

export const Closable: Story = {
  args: {
    children: 'Closable',
    closable: true,
    onClose: () => alert('Closed!'),
  },
};

export const Dot: Story = {
  args: {
    dot: true,
    color: 'error',
  },
};
