import React from 'react';

interface FormSkeletonProps {
  fields?: number;
}

export const FormSkeleton: React.FC<FormSkeletonProps> = ({ fields = 6 }) => {
  return (
    <div className="space-y-6 animate-pulse">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {[...Array(fields)].map((_, i) => (
          <div key={i} className="space-y-2">
            <div className="h-4 bg-gray-200 dark:bg-slate-700 rounded w-1/4" />
            <div className="h-10 bg-gray-100 dark:bg-slate-800 rounded w-full" />
          </div>
        ))}
      </div>
      
      {/* Footer / Buttons */}
      <div className="flex justify-end gap-3 pt-6 border-t border-border mt-6">
        <div className="h-10 bg-gray-200 dark:bg-slate-700 rounded w-24" />
        <div className="h-10 bg-indigo-200 dark:bg-indigo-900/30 rounded w-32" />
      </div>
    </div>
  );
};

export default FormSkeleton;
