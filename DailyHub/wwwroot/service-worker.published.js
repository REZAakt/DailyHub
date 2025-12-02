// Caution! Be sure you understand the caveats before publishing an application with
// offline support. See https://aka.ms/blazor-offline-considerations

self.importScripts('./service-worker-assets.js');
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
    const cache = await caches.open(cacheName);

    // فقط درخواست‌های GET رو هندل می‌کنیم
    if (event.request.method !== 'GET') {
        return fetch(event.request);
    }

    // ۱) اگر درخواست از نوع "navigate" هست (یعنی لود صفحه / روت)
    if (event.request.mode === 'navigate') {
        try {
            // اول سعی می‌کنیم از شبکه جواب بدیم
            const networkResponse = await fetch(event.request);
            return networkResponse;
        } catch (error) {
            console.warn('Network failed, serving offline page if possible.', error);

            // اگر نت قطع بود یا شبکه خطا داد → برو سراغ offline.html از کش
            const offlinePage = await cache.match('offline.html');
            if (offlinePage) {
                return offlinePage;
            }

            // اگر به هر دلیلی offline.html نبود، حداقل index.html رو بده
            const cachedIndex = await cache.match('index.html');
            if (cachedIndex) {
                return cachedIndex;
            }

            // آخرین fallback
            return new Response('Offline', {
                status: 503,
                statusText: 'Offline'
            });
        }
    }

    // ۲) برای بقیه درخواست‌ها (css/js/عکس و …) منطق قبلی کش
    let cachedResponse = null;

    const shouldServeIndexHtml = event.request.mode === 'navigate'
        && !manifestUrlList.some(url => url === event.request.url);

    const request = shouldServeIndexHtml ? 'index.html' : event.request;
    cachedResponse = await cache.match(request);

    return cachedResponse || fetch(event.request);
}
