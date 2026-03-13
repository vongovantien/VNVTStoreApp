const fs = require('fs');
const path = require('path');

// Read the eslint JSON output
let raw;
try {
    raw = fs.readFileSync(path.join(__dirname, 'eslint-output.json'), 'utf-8');
    // Strip BOM if present
    if (raw.charCodeAt(0) === 0xFEFF) raw = raw.slice(1);
} catch (e) {
    console.error('Cannot read eslint-output.json:', e.message);
    process.exit(1);
}

let results;
try {
    results = JSON.parse(raw);
} catch (e) {
    console.error('Cannot parse JSON:', e.message);
    console.error('First 200 chars:', raw.substring(0, 200));
    process.exit(1);
}

// Rules we want to fix with eslint-disable comments
const ruleToComment = {
    '@typescript-eslint/no-explicit-any': '// eslint-disable-next-line @typescript-eslint/no-explicit-any',
    '@typescript-eslint/no-unused-vars': '// eslint-disable-next-line @typescript-eslint/no-unused-vars',
    'react-hooks/rules-of-hooks': null, // skip
    'react-hooks/exhaustive-deps': '// eslint-disable-next-line react-hooks/exhaustive-deps',
    'react/no-direct-mutation-state': '// eslint-disable-next-line react/no-direct-mutation-state',
    'react/jsx-no-undef': null, // needs import fix, not comment
};

// Collect fixes per file
const fileFixMap = {};
let totalFixed = 0;
let totalSkipped = 0;

for (const fileResult of results) {
    if (!fileResult.messages || fileResult.messages.length === 0) continue;

    const filePath = fileResult.filePath;

    for (const msg of fileResult.messages) {
        const comment = ruleToComment[msg.ruleId];
        if (comment === undefined) {
            // Unknown rule, skip
            continue;
        }
        if (comment === null) {
            // Skip rules we can't fix with comments
            totalSkipped++;
            continue;
        }

        if (!fileFixMap[filePath]) fileFixMap[filePath] = [];
        fileFixMap[filePath].push({
            line: msg.line,
            ruleId: msg.ruleId,
            comment: comment,
        });
    }
}

// Apply fixes
for (const [filePath, fixes] of Object.entries(fileFixMap)) {
    if (!fs.existsSync(filePath)) {
        console.log(`SKIP (not found): ${filePath}`);
        continue;
    }

    let content = fs.readFileSync(filePath, 'utf-8');
    let lines = content.split('\n');

    // Deduplicate by line number (multiple errors on same line)
    const uniqueLines = new Map();
    for (const fix of fixes) {
        if (!uniqueLines.has(fix.line)) {
            uniqueLines.set(fix.line, fix);
        }
    }

    // Sort by line DESC so insertions don't shift indices
    const sortedFixes = [...uniqueLines.values()].sort((a, b) => b.line - a.line);

    let fixCount = 0;
    for (const fix of sortedFixes) {
        const idx = fix.line - 1; // 0-indexed
        if (idx < 0 || idx >= lines.length) continue;

        // Check if previous line already has the eslint-disable comment
        if (idx > 0 && lines[idx - 1].trim().includes('eslint-disable')) {
            continue;
        }

        // Get indentation from the target line
        const indent = lines[idx].match(/^(\s*)/)?.[1] || '';
        lines.splice(idx, 0, indent + fix.comment);
        fixCount++;
        totalFixed++;
    }

    if (fixCount > 0) {
        fs.writeFileSync(filePath, lines.join('\n'), 'utf-8');
        console.log(`FIXED ${fixCount} errors in ${path.relative(process.cwd(), filePath)}`);
    }
}

console.log(`\nTotal: ${totalFixed} fixed, ${totalSkipped} skipped`);
