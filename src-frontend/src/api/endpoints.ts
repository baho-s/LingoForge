import client from './client';
import type {
  AuthResponse,
  DashboardDto,
  WordDto,
  StatsDto,
  BulkGenerateResult,
  AddWordPayload,
  ReviewPayload,
  PracticeQuestionsResponse,
  PracticeAnswerPayload,
  PracticeAnswerResponse,
  GenerateSentencePayload,
  GeneratedSentenceResponse,
} from '../types';
import type { PracticeMode } from '../types';

export const authApi = {
  login: (email: string, password: string) =>
    client.post<AuthResponse>('/api/auth/login', { email, password }),
  register: (email: string, password: string) =>
    client.post<AuthResponse>('/api/auth/register', { email, password }),
};

export const wordsApi = {
  getAll: (skip: number = 0, take: number = 100) =>
    client.get<WordDto[]>('/api/words', { params: { skip, take } }),
  getReviewSessionWords: (limit: number = 8) =>
    client.get<WordDto[]>('/api/words/review/session', { params: { limit } }),
  add: (payload: AddWordPayload) => client.post<WordDto>('/api/words', payload),
  bulkGenerate: () => client.post<BulkGenerateResult>('/api/words/bulk-generate'),
  review: (id: string, payload: ReviewPayload) =>
    client.post<WordDto>(`/api/words/${id}/review`, payload),
  delete: (id: string) => client.delete(`/api/words/${id}`),
  bulkDeleteByField: (field: string) =>
    client.delete<{ success: boolean; fieldName: string; deletedCount: number; message: string }>(
      `/api/words/by-field/${encodeURIComponent(field)}`
    ),
  getWordOfDay: () => client.get<WordDto>('/api/words/word-of-day'),
};

export const dashboardApi = {
  get: () => client.get<DashboardDto>('/api/dashboard'),
  getWordOfDay: () => client.get<WordDto>('/api/words/word-of-day'),
};

export const practiceApi = {
  getQuestions: (modes: PracticeMode[] = ['multiple_choice'], limit = 8) =>
    client.get<PracticeQuestionsResponse>('/api/practice/questions', {
      params: { mode: modes.join(','), limit },
    }),
  generateSentence: (payload: GenerateSentencePayload) =>
    client.post<GeneratedSentenceResponse>('/api/practice/generate-sentence', payload),
  submitAnswer: (payload: PracticeAnswerPayload) =>
    client.post<PracticeAnswerResponse>('/api/practice/submit-answer', payload),
};

export const statsApi = {
  get: () => client.get<StatsDto>('/api/stats'),
};

export const predefinedWordsApi = {
  getFields: () =>
    client.get<{ fields: string[] }>('/api/PredefinedWords/fields'),
  getWordsByField: (field: string) =>
    client.get<{
      field: string;
      totalCount: number;
      words: Array<{
        id: string;
        field: string;
        category?: string;
        original: string;
        translation: string;
        aiSentence?: string;
      }>;
    }>(`/api/PredefinedWords/fields/${field}`),
  importField: (field: string) =>
    client.post<{
      success: boolean;
      fieldName: string;
      importedCount: number;
      message: string;
    }>('/api/PredefinedWords/import-field', { field }),
};
