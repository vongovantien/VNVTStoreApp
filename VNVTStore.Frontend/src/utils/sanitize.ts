import DOMPurify from 'dompurify';

/**
 * 🛡️ XSS Protection: Sanitize HTML content
 * Uses DOMPurify to remove malicious scripts while preserving safe HTML
 */

/**
 * Sanitize HTML for display (allows basic formatting)
 * Use for: Product descriptions, review comments from admin
 */
export const sanitizeHTML = (dirty: string): string => {
  return DOMPurify.sanitize(dirty, {
    ALLOWED_TAGS: ['p', 'br', 'strong', 'em', 'u', 'a', 'ul', 'ol', 'li', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6'],
    ALLOWED_ATTR: ['href', 'target', 'rel'],
    ALLOW_DATA_ATTR: false,
  });
};

/**
 * Sanitize user-generated content (minimal HTML)
 * Use for: User reviews, comments
 */
export const sanitizeUserContent = (dirty: string): string => {
  return DOMPurify.sanitize(dirty, {
    ALLOWED_TAGS: ['p', 'br', 'strong', 'em'],
    ALLOWED_ATTR: [],
    ALLOW_DATA_ATTR: false,
  });
};

/**
 * Strip all HTML tags - plain text only
 * Use for: Names, titles, search queries
 */
export const stripHTML = (dirty: string): string => {
  return DOMPurify.sanitize(dirty, {
    ALLOWED_TAGS: [],
    ALLOWED_ATTR: [],
  });
};

/**
 * React component helper: Create safe HTML props
 * Usage: <div dangerouslySetInnerHTML={createSafeHTML(content)} />
 */
export const createSafeHTML = (dirty: string, isUserContent = false) => {
  const clean = isUserContent ? sanitizeUserContent(dirty) : sanitizeHTML(dirty);
  return { __html: clean };
};
