import React from 'react';
import { cn } from '@/utils/cn';
import { useAuthStore } from '@/store';
import { getImageUrl } from '@/utils/format';

interface UserAvatarProps {
  className?: string;
  size?: 'xs' | 'sm' | 'md' | 'lg' | 'xl';
  avatarUrl?: string;
  fullName?: string;
  email?: string;
  showBorder?: boolean;
}

export const UserAvatar: React.FC<UserAvatarProps> = ({
  className,
  size = 'md',
  avatarUrl,
  fullName,
  email,
  showBorder = true
}) => {
  const { user } = useAuthStore();
  
  // Handle various property names from backend DTOs (avatar, Avatar, avatarUrl, AvatarUrl)
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const userAvatar = user?.avatar || (user as any)?.Avatar || (user as any)?.avatarUrl || (user as any)?.AvatarUrl;
  const currentAvatar = getImageUrl(avatarUrl || userAvatar);
  const currentName = fullName || user?.fullName;
  const currentEmail = email || user?.email;
  
  const sizeClasses = {
    xs: 'w-6 h-6 text-[10px]',
    sm: 'w-8 h-8 text-xs',
    md: 'w-10 h-10 text-sm',
    lg: 'w-16 h-16 text-2xl',
    xl: 'w-32 h-32 text-5xl',
  };

  const gradients = [
    'from-indigo-500 to-purple-500',
    'from-blue-500 to-cyan-500',
    'from-purple-500 to-pink-500',
    'from-emerald-500 to-teal-500',
    'from-orange-500 to-red-500'
  ];
  
  const initials = currentName?.charAt(0).toUpperCase() || currentEmail?.charAt(0).toUpperCase() || '?';
  const gradientIndex = initials.charCodeAt(0) % gradients.length;
  const gradient = gradients[gradientIndex];

  const [hasError, setHasError] = React.useState(false);

  // Reset error if avatar changes
  React.useEffect(() => {
    setHasError(false);
  }, [currentAvatar]);

  return (
    <div className={cn(
      "rounded-full overflow-hidden flex items-center justify-center relative shrink-0",
      showBorder && "border-2 border-white dark:border-slate-800 shadow-sm",
      sizeClasses[size],
      className
    )}>
      {(currentAvatar && !hasError) ? (
        <img 
          src={currentAvatar} 
          alt={initials} 
          className="w-full h-full object-cover"
          onError={() => {
            console.error('[UserAvatar] Failed to load image:', currentAvatar);
            setHasError(true);
          }}
        />
      ) : (
        <div className={cn(
          "w-full h-full flex items-center justify-center bg-gradient-to-br text-white font-bold shadow-inner",
          gradient
        )}>
          {initials}
        </div>
      )}
    </div>
  );
};
