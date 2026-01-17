import React from 'react';
import { useTranslation } from 'react-i18next';

interface AdminPageHeaderProps {
  title: string;
  subtitle?: string;
  rightSection?: React.ReactNode;
}

export const AdminPageHeader = ({ title, subtitle, rightSection }: AdminPageHeaderProps) => {
  const { t } = useTranslation();
  
  return (
    <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-6">
      <div>
        <h1 className="text-2xl font-bold bg-gradient-to-r from-indigo-600 to-purple-600 bg-clip-text text-transparent">
          {t(title)}
        </h1>
        {subtitle && (
          <p className="text-sm text-gray-500 mt-1">
            {t(subtitle)}
          </p>
        )}
      </div>
      {rightSection && (
        <div className="flex items-center gap-2">
          {rightSection}
        </div>
      )}
    </div>
  );
};
