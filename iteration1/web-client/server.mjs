import http from 'node:http';
import { readFile } from 'node:fs/promises';
import { fileURLToPath } from 'node:url';
import path from 'node:path';

const root = fileURLToPath(new URL('.', import.meta.url));
const allowed = /^(index\.html|requirements\/[a-z-]+(?:\.min|\.full\.min)?\.(js|css)|src\/[a-z-]+\.js)$/;
const types = { '.html': 'text/html', '.css': 'text/css', '.js': 'text/javascript' };
const server = http.createServer(async (request, response) => {
    const pathname = new URL(request.url, 'http://127.0.0.1').pathname;
    const relative = pathname === '/' ? 'index.html' : pathname.slice(1);
    if (request.method !== 'GET' || !allowed.test(relative)) {
        response.writeHead(404);
        response.end();
        return;
    }
    try {
        const content = await readFile(path.join(root, relative));
        response.writeHead(200, {
            'Content-Type': `${types[path.extname(relative)]}; charset=utf-8`,
            'Cache-Control': 'no-store', 'X-Content-Type-Options': 'nosniff'
        });
        response.end(content);
    } catch {
        response.writeHead(404);
        response.end();
    }
});
server.listen(8088, '127.0.0.1', () => console.log('Web client: http://127.0.0.1:8088'));
