#if USE_DATA_CACHING
const cacheName = {{{ JSON.stringify(COMPANY_NAME + "-" + PRODUCT_NAME + "-" + PRODUCT_VERSION)}}};

importScripts('TemplateData/utils.js');

async function getDataFileToCache() {
    try {
        const dataTypes = await self.utils.getDataTypes();
        if (dataTypes) {
            const selectedFile = self.utils.selectDataFile(dataTypes);
            return selectedFile ? [`Build/${selectedFile}`] : [];
        }
    } catch (error) {
        console.warn('[Service Worker] Failed to load data-types.json:', error);
    }
    return [];
}

const staticContentToCache = [
    "Build/{{{ LOADER_FILENAME }}}",
    "Build/{{{ FRAMEWORK_FILENAME }}}",
    #if USE_THREADS
    "Build/{{{ WORKER_FILENAME }}}",
    #endif
    "Build/{{{ CODE_FILENAME }}}",
    "TemplateData/style.css",
    "Build/data-types.json"
];
#endif

self.addEventListener('install', function (e) {
    console.log('[Service Worker] Install');

    #if USE_DATA_CACHING
    e.waitUntil((async function () {
        const cache = await caches.open(cacheName);
        console.log('[Service Worker] Caching static content');

        await cache.addAll(staticContentToCache);

        const dataFileToCache = await getDataFileToCache();
        if (dataFileToCache.length > 0) {
            console.log('[Service Worker] Caching optimal data file:', dataFileToCache);
            await cache.addAll(dataFileToCache);
        }
    })());
    #endif
});

#if USE_DATA_CACHING
self.addEventListener('fetch', function (e) {
    e.respondWith((async function () {
        let response = await caches.match(e.request);
        console.log(`[Service Worker] Fetching resource: ${e.request.url}`);
        if (response) { return response; }

        response = await fetch(e.request);
        const cache = await caches.open(cacheName);
        console.log(`[Service Worker] Caching new resource: ${e.request.url}`);
        cache.put(e.request, response.clone());
        return response;
    })());
});
#endif