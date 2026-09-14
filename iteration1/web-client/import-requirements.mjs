import { readdir, readFile, writeFile, mkdir, copyFile } from 'node:fs/promises';
import { createHash } from 'node:crypto';

const repository = new URL('../../', import.meta.url);
const names = await readdir(repository);
const htmlName = names.find(name => name.startsWith('FICC') && name.endsWith('.html'));
const assetsName = names.find(name => name.startsWith('FICC') && name.endsWith('_files'));
if (!htmlName || !assetsName) throw new Error('Original requirements HTML/assets are missing');
const destination = new URL('requirements/', import.meta.url);
await mkdir(destination, { recursive: true });
let html = await readFile(new URL(htmlName, repository), 'utf8');
html = html.replace('<script src="./local-preview.js"></script>', '<script src="src/requirements-guard.js"></script>');
html = html.replaceAll(`./${assetsName}/`, 'requirements/');
html = html.replace("connect-src &#39;self&#39; http://127.0.0.1:*", "connect-src &#39;self&#39; ws://127.0.0.1:18601");
html = html.replace('<script src="requirements/app.js"></script>', '<script src="requirements/app.js"></script>\n<script type="module" src="src/requirements-adapter.js"></script>');
// Saved browser-extension nodes follow the application body, and are not product UI.
const bodyEnd = html.indexOf('</body>');
if (bodyEnd >= 0) html = html.slice(0, bodyEnd + 7) + '</html>\n';
const files = [...new Set([...html.matchAll(/(?:src|href)="requirements\/([^"/]+)"/g)].map(match => match[1]))];
const manifest = {};
for (const file of files) {
    await copyFile(new URL(`${assetsName}/${file}`, repository), new URL(file, destination));
    const content = await readFile(new URL(file, destination));
    manifest[file] = createHash('sha256').update(content).digest('hex');
}
await writeFile(new URL('index.html', import.meta.url), html);
await writeFile(new URL('source-manifest.json', destination), JSON.stringify({ source: htmlName, files: manifest }, null, 2));
console.log(`Imported original requirements page and ${files.length} unchanged assets.`);
