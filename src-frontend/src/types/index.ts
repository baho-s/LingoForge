export interface AuthResponse {
  token: string;
  userId: string;
  email: string;
}

export interface WordDto {
  id: string;
  original: string;
  translation: string;
  aiSentence: string | null;
  field: string | null;
  intervalDays: number;
  easeFactor: number;
  repetitions: number;
  nextReviewAt: string;
  createdAt: string;
}

export interface AddWordPayload {
  original: string;
  translation: string;
  generateSentenceImmediately: boolean;
}

export interface ReviewPayload {
  outcome: ReviewOutcome;
}

export enum ReviewOutcome {
  Again = 0,
  Hard = 1,
  Good = 2,
  Easy = 3,
}

export interface BulkGenerateResult {
  generated: number;
  skipped: number;
}

export interface BadgeDto {
  type: string;
  awardedAt: string;
}

export interface WeeklyActivityPoint {
  date: string;
  wordsAdded: number;
}

export interface DashboardDto {
  streak: number;
  dailyGoal: number;
  reviewCount: number;
  lastActivity: string;
  badges: BadgeDto[];
  weeklyActivity: WeeklyActivityPoint[];
}


export type PracticeMode = 'multiple_choice' | 'spelling' | 'ai_sentence';

export type PracticeQuestionType = 'multiple_choice' | 'text_input' | 'ai_sentence';

export type PracticeDirection = 'EN_TO_TR' | 'TR_TO_EN';

export interface PracticeMultipleChoiceQuestion {
  id: string;
  type: 'multiple_choice';
  direction: PracticeDirection;
  prompt: string;
  options: string[];
  correct_answer: string;
}

export interface PracticeTextInputQuestion {
  id: string;
  type: 'text_input';
  direction: PracticeDirection;
  prompt: string;
  correct_answer: string;
}

export interface PracticeAiSentenceQuestion {
  id: string;
  type: 'ai_sentence';
  direction: PracticeDirection;
  english_sentence: string;
  prompt?: string;
  target_words_used?: string[];
}

export type PracticeQuestion =
  | PracticeMultipleChoiceQuestion
  | PracticeTextInputQuestion
  | PracticeAiSentenceQuestion;

export interface PracticeQuestionsResponse {
  questions: PracticeQuestion[];
}

export interface PracticeAnswerPayload {
  question_id: string;
  type: PracticeQuestionType;
  user_answer: string;
}

export interface PracticeAnswerResponse {
  is_correct: boolean;
  accuracy_score?: number;
  feedback?: string;
}

export interface GenerateSentencePayload {
  target_vocab: string[];
}

export interface GeneratedSentenceResponse {
  sentence_id: string;
  english_sentence: string;
  target_words_used: string[];
}

export interface StatsDto {
  totalWords: number;
  wordsLearnedThisWeek: number;
  averageEaseFactor: number;
}
