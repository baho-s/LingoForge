import { StrictMode, useEffect } from 'react';
import { createRoot } from 'react-dom/client';
import App from './App.tsx';
import './index.css';
import './i18n';

// Register Service Worker for automatic cache versioning
if ('serviceWorker' in navigator) {
  navigator.serviceWorker
    .register('/service-worker.js')
    .then((registration) => {
      console.log('[App] Service Worker registered');
      
      // Check for updates every hour
      setInterval(() => {
        registration.update().catch(() => {
          // Silently ignore errors
        });
      }, 60 * 60 * 1000);
    })
    .catch((error) => {
      console.log('[App] Service Worker registration failed:', error);
    });
}

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App />
  </StrictMode>
);
