const fs = require('fs');

function cleanJson(filePath, marker) {
    const lines = fs.readFileSync(filePath, 'utf8').split('\n');
    let outputLines = [];
    let skipping = false;
    let foundCount = 0;

    for (let i = 0; i < lines.length; i++) {
        const line = lines[i];
        if (line.includes('"auth": {')) {
            foundCount++;
            if (foundCount > 1) {
                skipping = true;
                // Try to remove the comma from the previous line if it exist
                if (outputLines.length > 0 && outputLines[outputLines.length - 1].trim().endsWith(',')) {
                    outputLines[outputLines.length - 1] = outputLines[outputLines.length - 1].replace(/,(\s*)$/, '$1');
                }
            }
        }

        if (!skipping) {
            outputLines.push(line);
        }

        if (skipping && line.trim() === '}') {
            // Check if this closes the 'auth' or the root
            // Our auth blocks usually end at the second to last line
            if (i < lines.length - 2) {
                skipping = false;
            }
        }
    }

    // Ensure root is closed
    if (outputLines[outputLines.length - 1].trim() !== '}') {
        outputLines.push('}');
    }

    fs.writeFileSync(filePath, outputLines.join('\n'), 'utf8');
    console.log(`Cleaned ${filePath}`);
}

const localesDir = 'D:/VNVTStoreApp/VNVTStore.Frontend/src/locales';
cleanJson(localesDir + '/vi.json');
cleanJson(localesDir + '/en.json');
