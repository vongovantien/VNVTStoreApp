import type { Meta, StoryObj } from '@storybook/react';
import { Skeleton, SkeletonText, SkeletonAvatar, SkeletonCard } from './Skeleton';

const meta = {
  title: 'UI/Skeleton',
  component: Skeleton,
  parameters: {
    layout: 'centered',
  },
  tags: ['autodocs'],
  argTypes: {
    width: { control: 'text' },
    height: { control: 'text' },
    circle: { control: 'boolean' },
    lines: { control: 'number' },
    animation: { control: 'select', options: ['pulse', 'wave', 'none'] },
  },
} satisfies Meta<typeof Skeleton>;

export default meta;
type Story = StoryObj<typeof meta>;

export const Default: Story = {
  args: {
    width: 200,
    height: 20,
  },
};

export const Circle: Story = {
  args: {
    width: 50,
    height: 50,
    circle: true,
  },
};

export const Text: Story = {
  render: () => <SkeletonText lines={3} />,
};

export const Avatar: Story = {
  render: () => <SkeletonAvatar size={60} />,
};

export const Card: Story = {
  render: () => <SkeletonCard />,
};

export const WaveAnimation: Story = {
  args: {
    width: 200,
    height: 100,
    animation: 'wave',
  },
};
