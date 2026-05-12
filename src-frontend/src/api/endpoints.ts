import client from './client';
import type {
  AuthResponse,
  DashboardDto,
  WordDto,
  QuizWordDto,
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
import { QuizMode } from '../types';
import type { PracticeMode } from '../types';

export const authApi = {
  login: (email: string, password: string) =>
    client.post<AuthResponse>('/api/auth/login', { email, password }),
  register: (email: string, password: string) =>
    client.post<AuthResponse>('/api/auth/register', { email, password }),
};

export const wordsApi = {
  getAll: () => client.get<WordDto[]>('/api/words'),
  add: (payload: AddWordPayload) => client.post<WordDto>('/api/words', payload),
  bulkGenerate: () => client.post<BulkGenerateResult>('/api/words/bulk-generate'),
  review: (id: string, payload: ReviewPayload) =>
    client.post<WordDto>(`/api/words/${id}/review`, payload),
  delete: (id: string) => client.delete(`/api/words/${id}`),
  getWordOfDay: () => client.get<WordDto>('/api/words/word-of-day'),
};

export const dashboardApi = {
  get: () => client.get<DashboardDto>('/api/dashboard'),
  getWordOfDay: () => client.get<WordDto>('/api/words/word-of-day'),
};

export const quizApi = {
  get: (mode: QuizMode = QuizMode.FillBlank, count = 10) =>
    client.get<QuizWordDto[]>('/api/quiz', { params: { mode, count } }),
};

export const practiceApi = {
  getQuestions: (modes: PracticeMode[] = ['multiple_choice'], limit = 10) =>
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
