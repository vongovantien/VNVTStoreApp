import type { Meta, StoryObj } from '@storybook/react';
import { ConfirmDialog } from './ConfirmDialog';
import { Button } from './Button';
import { useState } from 'react';

const meta = {
  title: 'UI/ConfirmDialog',
  component: ConfirmDialog,
  parameters: {
    layout: 'centered',
  },
  tags: ['autodocs'],
  argTypes: {
    title: { control: 'text' },
    message: { control: 'text' },
    confirmLabel: { control: 'text' },
    cancelLabel: { control: 'text' },
    variant: { control: 'select', options: ['danger', 'info', 'warning', 'success'] },
    isLoading: { control: 'boolean' },
  },
} satisfies Meta<typeof ConfirmDialog>;

export default meta;
type Story = StoryObj<typeof meta>;

const ConfirmDialogWrapper = (args: any) => {
  const [isOpen, setIsOpen] = useState(false);

  return (
    <div>
      <Button variant="danger" onClick={() => setIsOpen(true)}>Delete Item</Button>
      <ConfirmDialog
        {...args}
        isOpen={isOpen}
        onClose={() => setIsOpen(false)}
        onConfirm={() => {
          alert('Confirmed!');
          setIsOpen(false);
        }}
      />
    </div>
  );
};

export const Default: Story = {
  render: (args) => <ConfirmDialogWrapper {...args} />,
  args: {
    title: 'Delete Item',
    message: 'Are you sure you want to delete this item? This action cannot be undone.',
    confirmLabel: 'Delete',
    cancelLabel: 'Cancel',
    variant: 'danger',
  },
};

export const Info: Story = {
  render: (args) => <ConfirmDialogWrapper {...args} />,
  args: {
    title: 'Confirm Action',
    message: 'Do you want to proceed with this action?',
    confirmLabel: 'Yes',
    cancelLabel: 'No',
    variant: 'info',
  },
};

export const Loading: Story = {
  render: (args) => <ConfirmDialogWrapper {...args} />,
  args: {
    title: 'Processing',
    message: 'Please wait while we process your request...',
    isLoading: true,
  },
};
