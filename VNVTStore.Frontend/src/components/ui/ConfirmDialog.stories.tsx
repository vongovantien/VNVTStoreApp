import type { Meta, StoryObj } from '@storybook/react';
import { ConfirmDialog, ConfirmDialogProps } from './ConfirmDialog';
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
    confirmText: { control: 'text' },
    cancelText: { control: 'text' },
    variant: { control: 'select', options: ['danger', 'info', 'warning', 'success'] },
    isLoading: { control: 'boolean' },
  },
} satisfies Meta<typeof ConfirmDialog>;

export default meta;
type Story = StoryObj<typeof meta>;

const ConfirmDialogWrapper = (args: Partial<ConfirmDialogProps>) => {
  const [isOpen, setIsOpen] = useState(false);

  return (
    <div>
      <Button variant="danger" onClick={() => setIsOpen(true)}>Delete Item</Button>
      <ConfirmDialog
        title={args.title || 'Are you sure?'}
        message={args.message || 'This action cannot be undone.'}
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
    confirmText: 'Delete Forever',
    cancelText: 'Cancel',
    variant: 'danger',
    isOpen: true,
    onClose: () => {},
    onConfirm: () => {},
  },
};

export const Info: Story = {
  render: (args) => <ConfirmDialogWrapper {...args} />,
  args: {
    title: 'Info Message',
    message: 'This is an informative dialog without a cancel button.',
    confirmText: 'Got it',
    variant: 'info',
    hideCancel: true,
    isOpen: true,
    onClose: () => {},
    onConfirm: () => {},
  },
};

export const Loading: Story = {
  render: (args) => <ConfirmDialogWrapper {...args} />,
  args: {
    title: 'Processing',
    message: 'Please wait while we process your request.',
    isLoading: true,
    isOpen: true,
    onClose: () => {},
    onConfirm: () => {},
  },
};
