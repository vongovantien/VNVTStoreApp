import React from 'react';
import { useTranslation } from 'react-i18next';

interface AdminPageHeaderProps {
  title: string;
  subtitle?: string;
  rightSection?: React.ReactNode;
}

export const AdminPageHeader = ({ rightSection }: AdminPageHeaderProps) => {
  if (!rightSection) return null;
  
  return (
    <div className="flex flex-row justify-end items-center gap-4 mb-6">
      <div className="flex items-center gap-2 ml-auto">
        {rightSection}
      </div>
    </div>
  );
};
