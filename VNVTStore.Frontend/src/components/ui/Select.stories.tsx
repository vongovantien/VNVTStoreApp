import type { Meta, StoryObj } from '@storybook/react';
import { Select } from './Select';
import { User } from 'lucide-react';

const meta = {
  title: 'UI/Select',
  component: Select,
  parameters: {
    layout: 'centered',
  },
  tags: ['autodocs'],
  argTypes: {
    size: { control: 'select', options: ['sm', 'md', 'lg'] },
    label: { control: 'text' },
    helperText: { control: 'text' },
    error: { control: 'text' },
    fullWidth: { control: 'boolean' },
    isRequired: { control: 'boolean' },
    disabled: { control: 'boolean' },
  },
} satisfies Meta<typeof Select>;

export default meta;
type Story = StoryObj<typeof meta>;

const options = [
  { value: 'option1', label: 'Option 1' },
  { value: 'option2', label: 'Option 2' },
  { value: 'option3', label: 'Option 3', disabled: true },
];

export const Default: Story = {
  args: {
    options,
    placeholder: 'Select an option',
  },
};

export const WithLabel: Story = {
  args: {
    label: 'Choose an option',
    options,
    placeholder: 'Select...',
  },
};

export const WithHelperText: Story = {
  args: {
    label: 'Category',
    helperText: 'Select the product category',
    options,
  },
};

export const WithError: Story = {
  args: {
    label: 'Status',
    error: 'Status is required',
    options,
  },
};

export const WithIcon: Story = {
  args: {
    label: 'User Role',
    options,
    leftIcon: <User size={16} />,
  },
};

export const Large: Story = {
  args: {
    size: 'lg',
    options,
    placeholder: 'Large Select',
  },
};
