import { render, screen, fireEvent } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import Image from './Image';

describe('Image Component', () => {
    it('renders correctly with valid src', () => {
        render(<Image src="valid-image.jpg" alt="Test Image" />);
        const img = screen.getByRole('img');
        expect(img).toHaveAttribute('src', 'valid-image.jpg');
        expect(img).toHaveAttribute('alt', 'Test Image');
    });

    it('uses fallback image on error', () => {
        render(<Image src="invalid-image.jpg" alt="Broken Image" fallbackSrc="fallback.jpg" />);
        const img = screen.getByRole('img');
        
        // Simulate error
        fireEvent.error(img);
        
        expect(img).toHaveAttribute('src', 'fallback.jpg');
    });

    it('uses default fallback if none provided', () => {
        // We import the default image to check against it, but since it's an asset import, 
        // in test environment it usually mocks to the path string. 
        // We can just check that src changes after error.
        render(<Image src="invalid.jpg" alt="Default Fallback" />);
        const img = screen.getByRole('img');
        
        fireEvent.error(img);
        
        expect(img.getAttribute('src')).not.toBe('invalid.jpg');
        // The exact value depends on how Vite processes the asset import in tests, 
        // typically it's the path or a data URI.
    });
});
