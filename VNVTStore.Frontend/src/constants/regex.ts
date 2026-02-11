/**
 * Common Regex Patterns
 */
export const REGEX = {
    // Vietnam phone number: starts with 03, 05, 07, 08, 09 followed by 8 digits
    PHONE: /^(0[3|5|7|8|9])([0-9]{8})$/,

    // Simple phone number: 10 to 11 digits
    PHONE_SIMPLE: /^[0-9]{10,11}$/,

    // Password: At least 6 chars, 1 upper, 1 lower, 1 digit
    PASSWORD: /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$/,

    // Email regex (if needed specifically, though Zod has .email())
    EMAIL: /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/,

    // Vietnam Tax Code: 10 or 13 digits
    TAX_CODE: /^[0-9]{10}(-[0-9]{3})?$/,

    // Number only
    NUMBER_ONLY: /^[0-9]+$/,

    // Slug: lowercase letters, numbers, and hyphens
    SLUG: /^[a-z0-9]+(?:-[a-z0-9]+)*$/,
};

export default REGEX;
