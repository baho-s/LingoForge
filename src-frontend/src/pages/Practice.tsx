import { useEffect, useState, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Brain, ArrowLeft, Loader2, AlertCircle, ListChecks, PenSquare, Sparkles } from 'lucide-react';
import { wordsApi, practiceApi } from '../api/endpoints';
import { useToast } from '../components/Toast';
import type { WordDto, PracticeQuestion, PracticeMode, PracticeAnswerResponse } from '../types';
import { ReviewOutcome } from '../types';

const UUID_RE = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

type Mode = 'select' | 'review' | 'practice';

export default function Practice() {
  const { t } = useTranslation();
  const [mode, setMode] = useState<Mode>('select');

  // Review state
  const [reviewWords, setReviewWords] = useState<WordDto[]>([]);
  const [reviewIndex, setReviewIndex] = useState(0);
  const [flipped, setFlipped] = useState(false);
  const [reviewLoading, setReviewLoading] = useState(false);
  const [reviewComplete, setReviewComplete] = useState(false);
  const [reviewedCount, setReviewedCount] = useState(0);

  // Practice state
  const [practiceQuestions, setPracticeQuestions] = useState<PracticeQuestion[]>([]);
  const [practiceIndex, setPracticeIndex] = useState(0);
  const [practiceMode, setPracticeMode] = useState<PracticeMode>('multiple_choice');
  const [practiceSelectedOption, setPracticeSelectedOption] = useState<string | null>(null);
  const [practiceAnswer, setPracticeAnswer] = useState('');
  const [practiceSubmitted, setPracticeSubmitted] = useState(false);
  const [practiceFeedback, setPracticeFeedback] = useState<PracticeAnswerResponse | null>(null);
  const [practiceLoading, setPracticeLoading] = useState(false);
  const [practiceBusy, setPracticeBusy] = useState(false);
  const [practiceCorrectCount, setPracticeCorrectCount] = useState(0);
  const [questionStartTime, setQuestionStartTime] = useState<number | null>(null);

  const [error, setError] = useState('');
  const { addToast } = useToast();

  const startReview = async () => {
    setReviewLoading(true);
    setError('');
    setReviewComplete(false);
    try {
      const { data } = await wordsApi.getReviewSessionWords(8);
      if (data.length === 0) {
        setError(t('practice.noDueWords'));
        setReviewLoading(false);
        return;
      }
      setReviewWords(data);
      setReviewIndex(0);
      setFlipped(false);
      setReviewedCount(0);
      setMode('review');
    } catch {
      setError(t('common.error'));
    } finally {
      setReviewLoading(false);
    }
  };

  const resetPracticeUi = () => {
    setPracticeSelectedOption(null);
    setPracticeAnswer('');
    setPracticeSubmitted(false);
    setPracticeFeedback(null);
    setPracticeBusy(false);
  };

  const getPracticeModeLabel = (m: PracticeMode) => {
    switch (m) {
      case 'multiple_choice':
        return t('practice.selectCorrectAnswer');
      case 'spelling':
        return t('practice.typeYourAnswer');
      case 'ai_sentence':
        return 'AI Cümle Anlama';
      default:
        return t('practice.practice');
    }
  };

  const normalizeAnswer = (value: string) => value.trim().toLowerCase();

  const startPractice = async (m: PracticeMode) => {
    setPracticeLoading(true);
    setPracticeMode(m);
    setError('');
    try {
      const { data } = await practiceApi.getQuestions([m], 8);
      if (!data.questions.length) {
        setError(t('practice.noPracticeQuestions'));
        return;
      }
      setPracticeQuestions(data.questions);
      setPracticeIndex(0);
      setPracticeCorrectCount(0);
      setQuestionStartTime(Date.now());
      resetPracticeUi();
      setMode('practice');
    } catch {
      setError(t('common.error'));
    } finally {
      setPracticeLoading(false);
    }
  };

  const handleReviewOutcome = useCallback(async (outcome: ReviewOutcome) => {
    const word = reviewWords[reviewIndex];
    if (!word || !UUID_RE.test(word.id)) return;
    try {
      await wordsApi.review(word.id, { outcome });
    } catch {
      addToast(t('common.error'), 'error');
    }
    const nextReviewed = reviewedCount + 1;
    setReviewedCount(nextReviewed);
    const next = reviewIndex + 1;
    if (next >= reviewWords.length) {
      setReviewComplete(true);
      addToast(`${nextReviewed} kelime tekrar tamamlandı!`, 'success');
      return;
    }
    setReviewIndex(next);
    setFlipped(false);
  }, [reviewWords, reviewIndex, reviewedCount, addToast, t]);

  const handlePracticeOption = async (question: PracticeQuestion, option: string) => {
    if (practiceSubmitted || question.type !== 'multiple_choice') return;
    const isCorrect = option === question.correct_answer;
    setPracticeSelectedOption(option);
    setPracticeSubmitted(true);
    setPracticeFeedback({ is_correct: isCorrect, feedback: isCorrect ? t('practice.correct') : t('practice.incorrect') });
    if (isCorrect) setPracticeCorrectCount((count) => count + 1);
    try {
      const timeTakenMs = Date.now() - (questionStartTime || Date.now());
      console.log('🎯 Answer submitted:', { isCorrect, timeTakenMs, questionStartTime });
      const { data } = await practiceApi.submitAnswer({
        question_id: question.id,
        type: question.type,
        user_answer: option,
        time_taken_ms: timeTakenMs,
      });
      setPracticeFeedback(data);
    } catch {
      addToast(t('common.error'), 'error');
    }
  };

  const handlePracticeCheck = async (question: PracticeQuestion) => {
    if (practiceSubmitted || question.type !== 'text_input') return;
    const isCorrect = normalizeAnswer(practiceAnswer) === normalizeAnswer(question.correct_answer);
    setPracticeSubmitted(true);
    setPracticeFeedback({
      is_correct: isCorrect,
      feedback: isCorrect ? t('practice.correct') : `Doğru cevap: ${question.correct_answer}`,
    });
    if (isCorrect) setPracticeCorrectCount((count) => count + 1);
    try {
      const timeTakenMs = Date.now() - (questionStartTime || Date.now());
      console.log('✍️ Text answer submitted:', { isCorrect, timeTakenMs, questionStartTime });
      const { data } = await practiceApi.submitAnswer({
        question_id: question.id,
        type: question.type,
        user_answer: practiceAnswer,
        time_taken_ms: timeTakenMs,
      });
      setPracticeFeedback(data);
    } catch {
      addToast(t('common.error'), 'error');
    }
  };

  const handlePracticeAiSubmit = async (question: PracticeQuestion) => {
    if (practiceSubmitted || question.type !== 'ai_sentence') return;
    setPracticeSubmitted(true);
    setPracticeBusy(true);
    try {
      const timeTakenMs = Date.now() - (questionStartTime || Date.now());
      console.log('🤖 AI answer submitted:', { timeTakenMs, questionStartTime });
      const { data } = await practiceApi.submitAnswer({
        question_id: question.id,
        type: question.type,
        user_answer: practiceAnswer,
        time_taken_ms: timeTakenMs,
      });
      setPracticeFeedback(data);
      if (data.is_correct) setPracticeCorrectCount((count) => count + 1);
    } catch {
      setPracticeFeedback({
        is_correct: false,
        feedback: 'Cevap gönderilemedi. Sonraki soruya geçebilirsin.',
      });
      addToast(t('common.error'), 'error');
    } finally {
      setPracticeBusy(false);
    }
  };

  const goToNextPracticeQuestion = () => {
    const next = practiceIndex + 1;
    setPracticeIndex(next);
    if (next < practiceQuestions.length) {
      resetPracticeUi();
      setQuestionStartTime(Date.now());
    }
  };

  // Reset question timer when moving to next question
  useEffect(() => {
    if (mode === 'practice' && practiceQuestions.length > 0) {
      setQuestionStartTime(Date.now());
    }
  }, [practiceIndex, mode, practiceQuestions.length]);

  // Keyboard shortcuts for review
  useEffect(() => {
    if (mode !== 'review' || !flipped) return;
    const handler = (e: KeyboardEvent) => {
      if (e.key === '1') handleReviewOutcome(ReviewOutcome.Again);
      else if (e.key === '2') handleReviewOutcome(ReviewOutcome.Hard);
      else if (e.key === '3') handleReviewOutcome(ReviewOutcome.Good);
      else if (e.key === '4') handleReviewOutcome(ReviewOutcome.Easy);
    };
    window.addEventListener('keydown', handler);
    return () => window.removeEventListener('keydown', handler);
  }, [mode, flipped, handleReviewOutcome]);

  if (error && mode === 'select') {
    return (
      <div className="max-w-3xl mx-auto">
        <div className="bg-amber-50 text-amber-700 rounded-xl px-4 py-3 text-sm flex items-center gap-2">
          <AlertCircle size={16} />
          {error}
        </div>
      </div>
    );
  }

  // Mode selection
  if (mode === 'select') {
    return (
      <div className="max-w-3xl mx-auto space-y-6">
        <h2 className="text-2xl font-bold text-gray-900">{t('practice.practice')}</h2>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <button
            onClick={startReview}
            disabled={reviewLoading}
            className="bg-white rounded-2xl shadow-sm p-8 border border-gray-100 hover:shadow-md hover:border-blue-200 transition-all text-left group"
          >
            <div className="w-12 h-12 rounded-xl bg-blue-50 flex items-center justify-center mb-4 group-hover:bg-blue-100 transition-colors">
              <Brain size={24} className="text-blue-600" />
            </div>
            <h3 className="text-lg font-semibold text-gray-900 mb-1">Tekrar Oturumu</h3>
            <p className="text-sm text-gray-500">Bugün tekrar için hazır olan kelimeleri kartlarla tekrar et</p>
          </button>
          <button
            onClick={() => startPractice('multiple_choice')}
            disabled={practiceLoading}
            className="bg-white rounded-2xl shadow-sm p-8 border border-gray-100 hover:shadow-md hover:border-green-200 transition-all text-left group"
          >
            <div className="w-12 h-12 rounded-xl bg-green-50 flex items-center justify-center mb-4 group-hover:bg-green-100 transition-colors">
              <ListChecks size={24} className="text-green-600" />
            </div>
            <h3 className="text-lg font-semibold text-gray-900 mb-1">Çoktan Seçmeli</h3>
            <p className="text-sm text-gray-500">Dört seçenekten doğru çeviriyi seç</p>
          </button>
          <button
            onClick={() => startPractice('spelling')}
            disabled={practiceLoading}
            className="bg-white rounded-2xl shadow-sm p-8 border border-gray-100 hover:shadow-md hover:border-amber-200 transition-all text-left group"
          >
            <div className="w-12 h-12 rounded-xl bg-amber-50 flex items-center justify-center mb-4 group-hover:bg-amber-100 transition-colors">
              <PenSquare size={24} className="text-amber-600" />
            </div>
            <h3 className="text-lg font-semibold text-gray-900 mb-1">Yazma Egzersizi</h3>
            <p className="text-sm text-gray-500">Kelimeyi yazarak aktif hatırlama becerisini test et</p>
          </button>
          <button
            onClick={() => startPractice('ai_sentence')}
            disabled={practiceLoading}
            className="bg-white rounded-2xl shadow-sm p-8 border border-gray-100 hover:shadow-md hover:border-teal-200 transition-all text-left group"
          >
            <div className="w-12 h-12 rounded-xl bg-teal-50 flex items-center justify-center mb-4 group-hover:bg-teal-100 transition-colors">
              <Sparkles size={24} className="text-teal-600" />
            </div>
            <h3 className="text-lg font-semibold text-gray-900 mb-1">AI Cümle Anlama</h3>
            <p className="text-sm text-gray-500">Cümleyi oku ve anlamını Türkçe olarak yaz</p>
          </button>
        </div>
        {(reviewLoading || practiceLoading) && (
          <div className="flex justify-center py-8">
            <Loader2 size={24} className="animate-spin text-blue-600" />
          </div>
        )}
      </div>
    );
  }

  // Review mode
  if (mode === 'review') {
    const word = reviewWords[reviewIndex];
    if (!word) return null;

    if (reviewComplete) {
      return (
        <div className="max-w-2xl mx-auto space-y-6">
          <div className="bg-white rounded-2xl shadow-md p-12 border border-gray-100 text-center">
            <div className="w-16 h-16 rounded-full bg-green-50 flex items-center justify-center mx-auto mb-4">
              <Brain size={32} className="text-green-600" />
            </div>
            <p className="text-3xl font-bold text-gray-900 mb-2">Session Complete!</p>
            <p className="text-gray-500 text-sm mb-6">
              You reviewed {reviewedCount} of {reviewWords.length} words.
            </p>
            <div className="flex gap-3 justify-center">
              <button
                onClick={() => { setMode('select'); setReviewComplete(false); }}
                className="px-6 py-3 bg-gray-100 text-gray-700 rounded-xl text-sm font-medium hover:bg-gray-200 transition-colors"
              >
                Back to Practice
              </button>
              <button
                onClick={startReview}
                className="px-6 py-3 bg-blue-600 text-white rounded-xl text-sm font-medium hover:bg-blue-700 transition-colors"
              >
                Review Again
              </button>
            </div>
          </div>
        </div>
      );
    }

    return (
      <div className="max-w-2xl mx-auto space-y-6">
        <button onClick={() => setMode('select')} className="flex items-center gap-2 text-sm text-gray-500 hover:text-gray-700">
          <ArrowLeft size={16} /> Back
        </button>
        <div className="flex items-center justify-between">
          <p className="text-sm text-gray-400">{reviewIndex + 1} of {reviewWords.length}</p>
          <div className="w-32 bg-gray-100 rounded-full h-2">
            <div
              className="bg-blue-600 h-2 rounded-full transition-all duration-300"
              style={{ width: `${((reviewIndex + 1) / reviewWords.length) * 100}%` }}
            />
          </div>
        </div>

        {/* Flashcard */}
        <div
          onClick={() => setFlipped(!flipped)}
          className="cursor-pointer"
          style={{ perspective: '1000px' }}
        >
          <div
            className="relative w-full transition-transform duration-500"
            style={{ transformStyle: 'preserve-3d', transform: flipped ? 'rotateY(180deg)' : 'rotateY(0deg)' }}
          >
            {/* Front */}
            <div className="bg-white rounded-2xl shadow-md p-12 border border-gray-100 text-center" style={{ backfaceVisibility: 'hidden' }}>
              <p className="text-3xl font-bold text-gray-900">{word.original}</p>
              <p className="text-sm text-gray-400 mt-3">Click to reveal</p>
            </div>
            {/* Back */}
            <div
              className="absolute inset-0 bg-white rounded-2xl shadow-md p-12 border border-gray-100 text-center"
              style={{ backfaceVisibility: 'hidden', transform: 'rotateY(180deg)' }}
            >
              <p className="text-2xl font-bold text-gray-900 mb-2">{word.translation}</p>
              {word.aiSentence && (
                <p className="text-sm text-gray-500 italic">"{word.aiSentence}"</p>
              )}
            </div>
          </div>
        </div>

        {/* Review buttons */}
        {flipped && (
          <div className="grid grid-cols-4 gap-3">
            {[
              { label: 'Again', value: ReviewOutcome.Again, color: 'bg-red-50 text-red-700 hover:bg-red-100', desc: 'Forgot' },
              { label: 'Hard', value: ReviewOutcome.Hard, color: 'bg-orange-50 text-orange-700 hover:bg-orange-100', desc: 'Difficult' },
              { label: 'Good', value: ReviewOutcome.Good, color: 'bg-green-50 text-green-700 hover:bg-green-100', desc: 'Correct' },
              { label: 'Easy', value: ReviewOutcome.Easy, color: 'bg-blue-50 text-blue-700 hover:bg-blue-100', desc: 'Perfect' },
            ].map((btn) => (
              <button
                key={btn.value}
                onClick={() => handleReviewOutcome(btn.value)}
                className={`py-3 rounded-xl text-sm font-medium transition-colors ${btn.color}`}
                title={btn.desc}
              >
                {btn.label}
                <span className="block text-xs opacity-60 mt-0.5">{btn.desc}</span>
              </button>
            ))}
          </div>
        )}
      </div>
    );
  }

  // Practice mode
  if (mode === 'practice') {
    const question = practiceQuestions[practiceIndex];
    const isComplete = practiceIndex >= practiceQuestions.length;
    const directionLabel = question?.direction === 'TR_TO_EN' ? 'TR -> EN' : 'EN -> TR';

    return (
      <div className="max-w-2xl mx-auto space-y-6">
        <button onClick={() => setMode('select')} className="flex items-center gap-2 text-sm text-gray-500 hover:text-gray-700">
          <ArrowLeft size={16} /> Back
        </button>

        {isComplete ? (
          <div className="bg-white rounded-2xl shadow-md p-12 border border-gray-100 text-center">
            <p className="text-4xl font-bold text-gray-900 mb-2">{practiceCorrectCount}/{practiceQuestions.length}</p>
            <p className="text-gray-500 text-sm mb-2">
              {practiceCorrectCount === practiceQuestions.length
                ? 'Perfect score!'
                : practiceCorrectCount >= practiceQuestions.length / 2
                  ? 'Good job!'
                  : 'Keep practicing!'}
            </p>
            <p className="text-xs text-gray-400 mb-6">
              {practiceQuestions.length === 0
                ? '0% accuracy'
                : `${Math.round((practiceCorrectCount / practiceQuestions.length) * 100)}% accuracy`}
            </p>
            <div className="flex gap-3 justify-center">
              <button
                onClick={() => setMode('select')}
                className="px-6 py-3 bg-gray-100 text-gray-700 rounded-xl text-sm font-medium hover:bg-gray-200 transition-colors"
              >
                Back to Practice
              </button>
              <button
                onClick={() => startPractice(practiceMode)}
                className="px-6 py-3 bg-blue-600 text-white rounded-xl text-sm font-medium hover:bg-blue-700 transition-colors"
              >
                Try Again
              </button>
            </div>
          </div>
        ) : (
          <>
            <div className="flex items-center justify-between">
              <p className="text-sm text-gray-400">Question {practiceIndex + 1} of {practiceQuestions.length}</p>
              <div className="flex items-center gap-3">
                <span className="text-xs uppercase tracking-wide text-gray-400">{getPracticeModeLabel(practiceMode)}</span>
                <span className="text-sm font-medium text-blue-600">{practiceCorrectCount} correct</span>
              </div>
            </div>
            <div className="w-full bg-gray-100 rounded-full h-2">
              <div
                className="bg-blue-600 h-2 rounded-full transition-all duration-300"
                style={{ width: `${((practiceIndex + 1) / practiceQuestions.length) * 100}%` }}
              />
            </div>

            <div className="bg-white rounded-2xl shadow-md p-8 border border-gray-100">
              <p className="text-xs text-gray-400 mb-2">{directionLabel}</p>
              {question.type === 'ai_sentence' ? (
                <>
                  <p className="text-xl font-semibold text-gray-900 leading-relaxed">
                    {question.english_sentence || question.prompt || ''}
                  </p>
                  {question.target_words_used?.length ? (
                    <p className="text-xs text-gray-400 mt-3">Target words: {question.target_words_used.join(', ')}</p>
                  ) : null}
                </>
              ) : (
                <p className="text-2xl font-bold text-gray-900 text-center">{question.prompt}</p>
              )}
            </div>

            {question.type === 'multiple_choice' && (
              <div className="space-y-3">
                {question.options.map((option) => {
                  let bg = 'bg-white border-gray-200 hover:border-blue-300';
                  if (practiceSubmitted) {
                    if (option === question.correct_answer) bg = 'bg-green-50 border-green-400';
                    else if (option === practiceSelectedOption) bg = 'bg-red-50 border-red-400';
                  }
                  return (
                    <button
                      key={option}
                      onClick={() => handlePracticeOption(question, option)}
                      disabled={practiceSubmitted}
                      className={`w-full text-left px-5 py-4 rounded-xl border text-sm font-medium transition-colors ${bg} disabled:cursor-default`}
                    >
                      {option}
                    </button>
                  );
                })}
              </div>
            )}

            {question.type === 'text_input' && (
              <div className="space-y-3">
                <input
                  value={practiceAnswer}
                  onChange={(e) => setPracticeAnswer(e.target.value)}
                  disabled={practiceSubmitted}
                  placeholder="Type your answer"
                  className="w-full rounded-xl border border-gray-200 px-4 py-3 text-sm focus:outline-none focus:ring-2 focus:ring-blue-200"
                />
                <button
                  onClick={() => handlePracticeCheck(question)}
                  disabled={practiceSubmitted || practiceAnswer.trim().length === 0}
                  className="px-5 py-3 rounded-xl bg-blue-600 text-white text-sm font-medium hover:bg-blue-700 transition-colors disabled:bg-blue-200"
                >
                  Check Answer
                </button>
              </div>
            )}

            {question.type === 'ai_sentence' && (
              <div className="space-y-3">
                <textarea
                  value={practiceAnswer}
                  onChange={(e) => setPracticeAnswer(e.target.value)}
                  disabled={practiceSubmitted}
                  placeholder="Write the Turkish meaning or main idea"
                  rows={4}
                  className="w-full rounded-xl border border-gray-200 px-4 py-3 text-sm focus:outline-none focus:ring-2 focus:ring-blue-200"
                />
                <button
                  onClick={() => handlePracticeAiSubmit(question)}
                  disabled={practiceSubmitted || practiceAnswer.trim().length === 0 || practiceBusy}
                  className="inline-flex items-center gap-2 px-5 py-3 rounded-xl bg-blue-600 text-white text-sm font-medium hover:bg-blue-700 transition-colors disabled:bg-blue-200"
                >
                  {practiceBusy && <Loader2 size={16} className="animate-spin" />}
                  Submit Answer
                </button>
              </div>
            )}

            {practiceSubmitted && practiceFeedback && (
              <div
                className={`rounded-xl px-4 py-3 text-sm ${practiceFeedback.is_correct ? 'bg-green-50 text-green-700' : 'bg-red-50 text-red-700'}`}
              >
                <p className="font-medium">
                  {practiceFeedback.is_correct ? 'Correct' : 'Incorrect'}
                </p>
                {practiceFeedback.feedback && (
                  <p className="text-xs mt-1 opacity-80">{practiceFeedback.feedback}</p>
                )}
                {practiceFeedback.accuracy_score !== undefined && (
                  <p className="text-xs mt-1 opacity-80">Accuracy: {practiceFeedback.accuracy_score}%</p>
                )}
              </div>
            )}

            {practiceSubmitted && (
              <div className="flex justify-end">
                <button
                  onClick={goToNextPracticeQuestion}
                  className="px-6 py-3 bg-gray-900 text-white rounded-xl text-sm font-medium hover:bg-gray-800 transition-colors"
                >
                  Next Question
                </button>
              </div>
            )}
          </>
        )}
      </div>
    );
  }

  return null;
}
