import axios from 'axios';
import { useAuthStore } from '../store/auth';

const client = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
});

client.interceptors.request.use((config) => {
  const token = useAuthStore.getState().token;
  if (token && token !== 'undefined' && token !== 'null') {
    config.headers.Authorization = `Bearer ${token}`;
  }
  
  // Content-Type header'ını sadece body varsa ve DELETE değilse ekle
  // DELETE request'inde body yoksa Content-Type kaldır (iOS Safari uyumluluğu)
  if (config.method !== 'delete' || config.data) {
    if (!config.headers['Content-Type']) {
      config.headers['Content-Type'] = 'application/json';
    }
  } else if (config.method === 'delete') {
    delete config.headers['Content-Type'];
  }
  
  return config;
});

client.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      useAuthStore.getState().logout();
      window.location.href = '/login';
    }
    return Promise.reject(error);
  },
);

export default client;
