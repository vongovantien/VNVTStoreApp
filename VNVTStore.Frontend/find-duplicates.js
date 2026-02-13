/* eslint-disable @typescript-eslint/no-require-imports */
const fs = require('fs');

function findDuplicateKeys(jsonText) {
    const lines = jsonText.split('\n');
    const stack = [{}];
    const duplicates = [];

    lines.forEach((line, index) => {
        const match = line.match(/"([^"]+)"\s*:/);
        if (match) {
            const key = match[1];
            const currentObj = stack[stack.length - 1];
            if (currentObj[key]) {
                duplicates.push({ key, line: index + 1 });
            }
            currentObj[key] = true;
        }
        if (line.includes('{')) {
            stack.push({});
        }
        if (line.includes('}')) {
            stack.pop();
        }
    });

    return duplicates;
}

const content = fs.readFileSync('d:/VNVTStoreApp/VNVTStore.Frontend/src/locales/vi.json', 'utf8');
const dupes = findDuplicateKeys(content);
console.log(JSON.stringify(dupes, null, 2));
