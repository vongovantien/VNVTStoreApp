import React from 'react';
import { ImageIcon } from 'lucide-react';
import { cn } from '@/utils/cn';
import defaultImage from '../../assets/default-image.png';

interface ImageProps extends React.ImgHTMLAttributes<HTMLImageElement> {
  fallbackSrc?: string;
  showPlaceholder?: boolean;
}

const Image: React.FC<ImageProps> = ({ 
  src, 
  fallbackSrc = defaultImage, 
  alt, 
  className,
  showPlaceholder = true,
  ...props 
}) => {
  const [isLoaded, setIsLoaded] = React.useState(false);
  const [hasError, setHasError] = React.useState(false);
  const [currentSrc, setCurrentSrc] = React.useState(src);

  React.useEffect(() => {
    setCurrentSrc(src);
    setHasError(false);
    setIsLoaded(false);
  }, [src]);

  const handleError = () => {
    if (currentSrc !== fallbackSrc) {
      setCurrentSrc(fallbackSrc);
      setHasError(true);
    }
  };

  return (
    <div className={cn("relative overflow-hidden bg-slate-50 flex items-center justify-center", className)}>
      {/* Loading state / Image */}
      <img
        src={currentSrc || fallbackSrc}
        alt={alt}
        onLoad={() => setIsLoaded(true)}
        onError={handleError}
        className={cn(
          "transition-opacity duration-300 w-full h-full object-cover",
          isLoaded ? "opacity-100" : "opacity-0"
        )}
        {...props}
      />

      {/* Placeholder / Error UI */}
      {(!isLoaded || (!currentSrc && showPlaceholder)) && (
        <div className="absolute inset-0 flex flex-col items-center justify-center text-slate-300 bg-slate-50/50">
          <ImageIcon size={24} strokeWidth={1.5} />
          {!isLoaded && !hasError && (
             <div className="absolute inset-0 bg-gradient-to-r from-transparent via-white/20 to-transparent -translate-x-full animate-shimmer" />
          )}
        </div>
      )}
    </div>
  );
};

export default Image;
