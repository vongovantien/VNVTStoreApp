import type { Meta, StoryObj } from '@storybook/react';
import { Card, CardHeader, CardBody, CardFooter } from './Card';
import { Button } from './Button';

const meta = {
  title: 'UI/Card',
  component: Card,
  parameters: {
    layout: 'centered',
  },
  tags: ['autodocs'],
  argTypes: {
    variant: { control: 'select', options: ['default', 'elevated', 'outline', 'glass'] },
    padding: { control: 'select', options: ['none', 'sm', 'md', 'lg'] },
    hoverable: { control: 'boolean' },
    clickable: { control: 'boolean' },
  },
} satisfies Meta<typeof Card>;

export default meta;
type Story = StoryObj<typeof meta>;

export const Default: Story = {
  render: (args) => (
    <Card {...args} className="w-[350px]">
      <CardHeader title="Card Title" subtitle="Card Subtitle" />
      <CardBody>
        <p className="text-sm text-gray-600">
          This is the body content of the card. It can hold any content you want.
        </p>
      </CardBody>
      <CardFooter>
        <Button size="sm" fullWidth>Action</Button>
      </CardFooter>
    </Card>
  ),
  args: {
    variant: 'default',
  },
};

export const ElevatedWithHover: Story = {
  render: (args) => (
    <Card {...args} className="w-[350px]">
      <CardHeader title="Elevated Card" />
      <CardBody>
        <p className="text-sm text-gray-600">
          Hover over me to see the elevation effect.
        </p>
      </CardBody>
    </Card>
  ),
  args: {
    variant: 'elevated',
    hoverable: true,
    padding: 'none',
  },
};

export const Outline: Story = {
  render: (args) => (
    <Card {...args} className="w-[350px]">
      <CardBody>
        <h4 className="font-bold">Simple Outline Card</h4>
        <p className="text-sm text-gray-600 mt-2">Just the content without header or footer components.</p>
      </CardBody>
    </Card>
  ),
  args: {
    variant: 'outline',
    padding: 'none',
  },
};
