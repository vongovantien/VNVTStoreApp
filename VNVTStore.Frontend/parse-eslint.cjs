const fs = require('fs');
let data = fs.readFileSync('eslint-report.json', 'utf16le');
if (data.charCodeAt(0) === 0xFEFF) data = data.slice(1);
const parsed = JSON.parse(data);
const errors = [];
parsed.forEach(f => {
  const errs = f.messages.filter(m => m.severity === 2);
  if (errs.length > 0) {
    errors.push({ file: f.filePath.replace(/\\/g, '/').split('VNVTStore.Frontend/src/')[1], rules: Array.from(new Set(errs.map(e => e.ruleId || e.message))) });
  }
});
errors.forEach(e => console.log(e.file + ' : ' + e.rules.join(', ')));
