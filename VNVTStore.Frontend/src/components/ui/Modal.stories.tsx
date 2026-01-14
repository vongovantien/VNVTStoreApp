import type { Meta, StoryObj } from '@storybook/react';
import { Modal } from './Modal';
import { Button } from './Button';
import { useState } from 'react';

const meta = {
  title: 'UI/Modal',
  component: Modal,
  parameters: {
    layout: 'centered',
  },
  tags: ['autodocs'],
  argTypes: {
    size: { control: 'select', options: ['sm', 'md', 'lg', 'xl', 'full'] },
    centered: { control: 'boolean' },
  },
} satisfies Meta<typeof Modal>;

export default meta;
type Story = StoryObj<typeof meta>;

// Render function wrapper to handle state
const ModalWrapper = (args: any) => {
  const [isOpen, setIsOpen] = useState(false);

  return (
    <div>
      <Button onClick={() => setIsOpen(true)}>Open Modal</Button>
      <Modal {...args} isOpen={isOpen} onClose={() => setIsOpen(false)}>
        <p className="text-slate-600">
          This is the modal content. It can contain any elements.
        </p>
        <div className="mt-4 flex justify-end gap-2">
           <Button variant="outline" onClick={() => setIsOpen(false)}>Cancel</Button>
           <Button onClick={() => setIsOpen(false)}>Confirm</Button>
        </div>
      </Modal>
    </div>
  );
};

export const Default: Story = {
  render: (args) => <ModalWrapper {...args} />,
  args: {
    isOpen: false,
    onClose: () => {},
    title: 'Example Modal',
    size: 'md',
    children: 'Content', // Dummy value to satisfy type
  },
};

export const Large: Story = {
  render: (args) => <ModalWrapper {...args} />,
  args: {
    isOpen: false,
    onClose: () => {},
    title: 'Large Modal',
    size: 'lg',
    children: 'Content',
  },
};
