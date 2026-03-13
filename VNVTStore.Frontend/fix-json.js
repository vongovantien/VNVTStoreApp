const fs = require('fs');
const path = require('path');

function fixJson(filePath) {
    const content = fs.readFileSync(filePath, 'utf8');
    // We can't use JSON.parse if there are duplicate keys, but actually JSON.parse in Node handles it by taking the last one!
    // BUT if there are multiple root keys or other structural issues, it might fail.
    // However, our files are mostly valid.

    try {
        const json = JSON.parse(content);
        const pretty = JSON.stringify(json, null, 2);
        fs.writeFileSync(filePath + '.fixed', pretty, 'utf8');
        console.log(`Fixed ${filePath}`);
    } catch (e) {
        console.error(`Error fixing ${filePath}: ${e.message}`);

        // If it fails, try a more robust approach using regex to remove duplicate sections at the end
        // But let's see if this works first.
    }
}

const localesDir = 'D:/VNVTStoreApp/VNVTStore.Frontend/src/locales';
fixJson(path.join(localesDir, 'vi.json'));
fixJson(path.join(localesDir, 'en.json'));
