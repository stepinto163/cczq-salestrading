// Loaded before the original application: unmigrated HTTP actions cannot reach its old backend.
(() => {
    const originalFetch = window.fetch.bind(window);
    window.fetch = (input, options) => {
        const url = new URL(typeof input === 'string' ? input : input.url, location.href);
        if (url.pathname.startsWith('/api/')) {
            const detail = '该功能尚未接入 GW；本轮支持报价录入、确认、行情与提醒。';
            return Promise.resolve(new Response(JSON.stringify({ success: false, ok: false, detail, error: detail }), {
                status: 501, headers: { 'Content-Type': 'application/json' }
            }));
        }
        return originalFetch(input, options);
    };
})();
