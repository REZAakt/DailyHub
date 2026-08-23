// Caution! Be sure you understand the caveats before publishing an application with
// offline support. See https://aka.ms/blazor-offline-considerations

self.importScripts('./service-worker-assets.js');
console.info('Service worker: script loaded');

self.addEventListener('install', event => event.waitUntil(onInstall(event)));
self.addEventListener('activate', event => event.waitUntil(onActivate(event)));
self.addEventListener('fetch', event => event.respondWith(onFetch(event)));

const cacheNamePrefix = 'offline-cache-';
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;
const offlineAssetsInclude = [ /\.dll$/, /\.pdb$/, /\.wasm/, /\.html/, /\.js$/, /\.json$/, /\.css$/, /\.woff$/, /\.png$/, /\.jpe?g$/, /\.gif$/, /\.ico$/, /\.blat$/, /\.dat$/ ];
const offlineAssetsExclude = [ /^service-worker\.js$/ ];

// Replace with your base path if you are hosting on a subfolder. Ensure there is a trailing '/'.
const base = "/";
const baseUrl = new URL(base, self.origin);
const manifestUrlList = self.assetsManifest.assets.map(asset => new URL(asset.url, baseUrl).href);

async function onInstall(event) {
    console.info('Service worker: Install');
        
    // Fetch and cache all matching items from the assets manifest
    const assetsRequests = self.assetsManifest.assets
        .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
        .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
        .map(asset => new Request(asset.url, { integrity: asset.hash, cache: 'no-cache' }));

    await caches.open(cacheName).then(cache => cache.addAll(assetsRequests));

    // verify offline.html is in the cache for debugging
    const offlineCached = await caches.open(cacheName).then(cache => cache.match('offline.html')).catch(() => null);
    console.info('Service worker: offline.html cached?', !!offlineCached);
}

async function onActivate(event) {
    console.info('Service worker: Activate');

    // Delete unused caches
    const cacheKeys = await caches.keys();
    await Promise.all(cacheKeys
        .filter(key => key.startsWith(cacheNamePrefix) && key !== cacheName)
        .map(key => caches.delete(key)));
}


async function onFetch(event) {
    if (event.request.method !== 'GET') {
        return fetch(event.request);
    }

    const cache = await caches.open(cacheName);
    const isNavigate = event.request.mode === 'navigate';

    // ۱) برای درخواست‌های صفحه (route ها)
    if (isNavigate) {
        try {
            // سعی می‌کنیم آنلاین جواب بدیم
            const networkResponse = await fetch(event.request);
            return networkResponse;
        } catch (error) {
            console.warn('Network failed, serving offline page if possible.', error);

            // اول صفحه‌ی آفلاین
            const offlinePage = await cache.match('offline.html');
            console.info('Service worker: offline.html found in cache?', !!offlinePage);
            if (offlinePage) {
                return offlinePage;
            }

            // بعد index.html برای fallback
            const cachedIndex = await cache.match('index.html');
            if (cachedIndex) {
                return cachedIndex;
            }

            // آخرش یه جواب ساده
            return new Response('Offline', {
                status: 503,
                statusText: 'Offline'
            });
        }
    }

    // ۲) برای بقیه‌ی فایل‌ها (css/js/عکس و ...)
    const cachedResponse = await cache.match(event.request);
    if (cachedResponse) {
        return cachedResponse;
    }

    return fetch(event.request);
}
