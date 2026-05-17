// Service Worker for cache versioning
// Automatically clears old cache versions on app updates

const CACHE_PREFIX = 'vocapp';
const CACHE_VERSION = 'v1'; // Increment this when deploying new versions

self.addEventListener('install', (event) => {
  console.log('[SW] Installing...');
  self.skipWaiting();
});

self.addEventListener('activate', (event) => {
  console.log('[SW] Activating...');
  event.waitUntil(
    caches.keys().then((cacheNames) => {
      return Promise.all(
        cacheNames
          .filter((cacheName) => cacheName.startsWith(CACHE_PREFIX) && cacheName !== `${CACHE_PREFIX}-${CACHE_VERSION}`)
          .map((cacheName) => {
            console.log('[SW] Deleting old cache:', cacheName);
            return caches.delete(cacheName);
          })
      );
    })
  );
  self.clients.claim();
});
