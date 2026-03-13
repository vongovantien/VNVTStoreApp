const fs = require('fs');
const path = require('path');

// Find all ts/tsx files and remove lines that are ONLY eslint-disable-next-line comments
// that were added by the fix script
const srcDir = path.join(__dirname, 'src');

function walkDir(dir) {
    const entries = fs.readdirSync(dir, { withFileTypes: true });
    const files = [];
    for (const entry of entries) {
        const full = path.join(dir, entry.name);
        if (entry.isDirectory() && !entry.name.startsWith('.') && entry.name !== 'node_modules') {
            files.push(...walkDir(full));
        } else if (entry.isFile() && /\.(ts|tsx)$/.test(entry.name)) {
            files.push(full);
        }
    }
    return files;
}

const disablePatterns = [
    '// eslint-disable-next-line @typescript-eslint/no-explicit-any',
    '// eslint-disable-next-line @typescript-eslint/no-unused-vars',
    '// eslint-disable-next-line react-hooks/exhaustive-deps',
    '// eslint-disable-next-line react-hooks/set-state-in-effect',
    '// eslint-disable-next-line react/no-direct-mutation-state',
];

let totalRemoved = 0;
const files = walkDir(srcDir);

for (const filePath of files) {
    const content = fs.readFileSync(filePath, 'utf-8');
    const lines = content.split('\n');
    const newLines = [];
    let removed = 0;

    for (let i = 0; i < lines.length; i++) {
        const trimmed = lines[i].trim();
        if (disablePatterns.includes(trimmed)) {
            removed++;
            continue; // skip this line
        }
        newLines.push(lines[i]);
    }

    if (removed > 0) {
        fs.writeFileSync(filePath, newLines.join('\n'), 'utf-8');
        console.log(`Removed ${removed} disable comments from ${path.relative(process.cwd(), filePath)}`);
        totalRemoved += removed;
    }
}

console.log(`\nTotal removed: ${totalRemoved}`);
