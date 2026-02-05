import React, { useState } from 'react';
import defaultImage from '../../assets/default-image.png';

interface ImageProps extends React.ImgHTMLAttributes<HTMLImageElement> {
  fallbackSrc?: string;
}

const Image: React.FC<ImageProps> = ({ src, fallbackSrc = defaultImage, alt, ...props }) => {
  const [lastSrc, setLastSrc] = useState(src);
  const [hasError, setHasError] = useState(false);
  const [currentSrc, setCurrentSrc] = useState(src);

  // Update state during render when src prop changes
  // This is a recommended pattern for syncing state to props
  if (src !== lastSrc) {
    setLastSrc(src);
    setHasError(false);
    setCurrentSrc(src);
  }

  const handleError = () => {
    if (!hasError) {
      setCurrentSrc(fallbackSrc);
      setHasError(true);
    }
  };

  return (
    <img
      src={currentSrc || fallbackSrc}
      alt={alt}
      onError={handleError}
      {...props}
    />
  );
};

export default Image;
