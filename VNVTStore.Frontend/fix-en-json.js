const fs = require('fs');
const path = require('path');

const enPath = path.resolve('src/locales/en.json');
let content = fs.readFileSync(enPath, 'utf8');

// The file is corrupted at the beginning. 
// Let's try to fix the string before parsing.
content = content.replace(/},\s+},/g, '},');

try {
    const obj = JSON.parse(content);
    // Restore social if missing
    if (!obj.social) {
        obj.social = {
            "view": "is viewing this product",
            "buy": "just purchased a product",
            "wishlist": "added to wishlist"
        };
    }
    // Re-stringify with proper formatting (mimic the existing one - 4 spaces/double spaces)
    // The current file seems to use 4 spaces for indentation of values.
    fs.writeFileSync(enPath, JSON.stringify(obj, null, 4), 'utf8');
    console.log('Successfully fixed en.json');
} catch (e) {
    console.error('Failed to parse JSON even after simple fix:', e.message);
    // Find the error location
    const pos = e.message.match(/at position (\d+)/);
    if (pos) {
        const index = parseInt(pos[1]);
        console.error('Error around:', content.substring(index - 50, index + 50));
    }
}
