const fs = require('fs');
const path = require('path');

const questionsDir = path.join(__dirname, 'src', 'config', 'questions');
const files = fs.readdirSync(questionsDir).filter(f => f.endsWith('.js'));

files.forEach(file => {
    const filePath = path.join(questionsDir, file);
    let content = fs.readFileSync(filePath, 'utf8');

    // Remove (Variation X)
    content = content.replace(/\s*\(Variation \d+\)/g, '');
    
    // Remove [Strategy X]
    content = content.replace(/\s*\[Strategy \d+\]/g, '');
    
    // Remove [after10], [after12], etc prefixes if they exist (based on user image)
    content = content.replace(/\[after\w+\]\s*/g, '');

    fs.writeFileSync(filePath, content, 'utf8');
    console.log(`Cleaned ${file}`);
});
