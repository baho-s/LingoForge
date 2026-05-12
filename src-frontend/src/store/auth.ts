import { create } from 'zustand';

interface AuthState {
  token: string | null;
  isAuthenticated: boolean;
  login: (token: string) => void;
  logout: () => void;
}

const TOKEN_KEY = 'vocabapp_token';

const normalizeToken = (raw: string | null) => {
  if (!raw || raw === 'undefined' || raw === 'null') return null;
  return raw;
};

const initialToken = normalizeToken(localStorage.getItem(TOKEN_KEY));

export const useAuthStore = create<AuthState>((set) => ({
  token: initialToken,
  isAuthenticated: !!initialToken,
  login: (token: string) => {
    const normalized = normalizeToken(token);
    if (!normalized) {
      localStorage.removeItem(TOKEN_KEY);
      set({ token: null, isAuthenticated: false });
      return;
    }
    localStorage.setItem(TOKEN_KEY, normalized);
    set({ token: normalized, isAuthenticated: true });
  },
  logout: () => {
    localStorage.removeItem(TOKEN_KEY);
    set({ token: null, isAuthenticated: false });
  },
}));
