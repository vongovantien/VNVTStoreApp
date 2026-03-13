const fs = require('fs');
const path = require('path');

let raw = fs.readFileSync(path.join(__dirname, 'eslint-errors.json'), 'utf-8');
if (raw.charCodeAt(0) === 0xFEFF) raw = raw.slice(1);
const results = JSON.parse(raw);

const lines = [];
const errorsByRule = {};
let total = 0;

for (const fileResult of results) {
    if (!fileResult.messages || fileResult.messages.length === 0) continue;
    for (const msg of fileResult.messages) {
        const rule = msg.ruleId || 'unknown';
        if (!errorsByRule[rule]) errorsByRule[rule] = [];
        errorsByRule[rule].push({
            file: path.relative(process.cwd(), fileResult.filePath),
            line: msg.line,
            column: msg.column,
            message: msg.message,
        });
        total++;
    }
}

lines.push(`Total errors: ${total}`);
lines.push('');
lines.push('=== BY RULE ===');
for (const [rule, errors] of Object.entries(errorsByRule)) {
    lines.push(`${rule}: ${errors.length}`);
}
lines.push('');
lines.push('=== DETAILS ===');
for (const [rule, errors] of Object.entries(errorsByRule)) {
    lines.push(`\n--- ${rule} (${errors.length}) ---`);
    for (const e of errors) {
        lines.push(`  ${e.file}:${e.line}:${e.column} ${e.message}`);
    }
}

fs.writeFileSync(path.join(__dirname, 'lint-report.txt'), lines.join('\n'), 'utf-8');
console.log('Report written to lint-report.txt');
