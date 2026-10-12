import axios from 'axios';
import { useAuthStore } from '../store/auth';

export const GUEST_AI_LIMIT_EVENT = 'guest-ai-limit';

const client = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
});

client.interceptors.request.use((config) => {
  const token = useAuthStore.getState().token;
  if (token && token !== 'undefined' && token !== 'null') {
    config.headers.Authorization = `Bearer ${token}`;
  }
  
  // iOS Safari uyumluluğu: her request'te Content-Type ekle
  // DELETE request'lerinde body olmasa da Content-Type gerekli (iOS pre-flight fix)
  if (!config.headers['Content-Type']) {
    config.headers['Content-Type'] = 'application/json';
  }
  
  return config;
});

client.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 429 && error.response?.data?.code === 'guest_ai_limit') {
      window.dispatchEvent(new CustomEvent(GUEST_AI_LIMIT_EVENT));
    }
    if (error.response?.status === 401) {
      useAuthStore.getState().logout();
      window.location.href = '/login';
    }
    return Promise.reject(error);
  },
);

export default client;
