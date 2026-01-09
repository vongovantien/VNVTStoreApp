import React from 'react';

export const ProductSkeleton = () => {
  return (
    <div className="bg-primary rounded-xl overflow-hidden border shadow-sm h-full flex flex-col">
      {/* Image Skeleton */}
      <div className="relative aspect-square bg-tertiary/20 animate-pulse">
        <div className="absolute top-2 left-2 w-12 h-6 bg-tertiary/30 rounded-full"></div>
        <div className="absolute top-2 right-2 w-8 h-8 bg-tertiary/30 rounded-full"></div>
      </div>

      {/* Content Skeleton */}
      <div className="p-4 flex flex-col flex-1 gap-3">
        {/* Category */}
        <div className="w-1/3 h-3 bg-tertiary/20 rounded-full animate-pulse"></div>
        
        {/* Title */}
        <div className="w-3/4 h-5 bg-tertiary/20 rounded-md animate-pulse"></div>
        
        {/* Rating */}
        <div className="flex gap-1">
          <div className="w-24 h-3 bg-tertiary/20 rounded-full animate-pulse"></div>
        </div>

        {/* Price */}
        <div className="mt-auto flex items-end justify-between">
          <div className="space-y-1">
            <div className="w-16 h-3 bg-tertiary/20 rounded-full animate-pulse"></div>
            <div className="w-24 h-6 bg-tertiary/20 rounded-md animate-pulse"></div>
          </div>
          <div className="w-10 h-10 bg-tertiary/20 rounded-full animate-pulse"></div>
        </div>
      </div>
    </div>
  );
};
