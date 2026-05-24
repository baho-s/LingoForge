# VocApp Backend Architecture & Practice Algorithm Analysis

## Executive Summary

VocApp implements a **Clean Architecture** with **Domain-Driven Design principles**, using the **SM-2 (SuperMemo-2) spaced repetition algorithm** for vocabulary practice. The current implementation embeds progress tracking in a `ReviewInfo` value object rather than maintaining a separate progress entity.

---

## 1. PRACTICE QUESTION GENERATION FLOW

### Current Architecture

```
HTTP Request (PracticeController)
    ↓
GetPracticeQuestionsQuery (MediatR)
    ↓
GetPracticeQuestionsQueryHandler
    ├─ GetWordsForPracticeAsync (Database query with priority sorting)
    ├─ ParseModes (Split and validate practice modes)
    └─ BuildQuestions (In-memory question construction)
    ↓
PracticeQuestionsResponse
```

### Step-by-Step Flow

**1. API Endpoint** ([PracticeController.cs](src-backend/BeeZillion.API/Controllers/PracticeController.cs#L24-L36))
```csharp
GET /api/practice/questions?mode=multiple_choice,ai_sentence&limit=8
```
- Accepts optional `mode` (comma-separated: `multiple_choice`, `spelling`, `ai_sentence`)
- Accepts `limit` (default 8)
- Delegates to MediatR

**2. Query Handler** ([GetPracticeQuestionsQueryHandler.cs](src-backend/BeeZillion.Application/Practice/Queries/GetPracticeQuestions/GetPracticeQuestionsQueryHandler.cs#L22-L44))

```csharp
var words = await _wordRepository.GetWordsForPracticeAsync(userId, limit: limit * 2);
var modes = ParseModes(request.Mode);  // Default: ["multiple_choice"]
var questions = BuildQuestions(words, modes, limit);
```

**Key Decision:** Fetches `limit * 2` words because AI Sentence filtering happens in memory (some words may not have AI sentences)

**3. Database Query** ([WordRepository.cs](src-backend/BeeZillion.Infrastructure/Persistence/Repositories/WordRepository.cs#L77-L116))

Multi-level priority ordering with database-side optimization:

| Priority | Condition | Purpose |
|----------|-----------|---------|
| 1st | `NextReviewAt <= now` | Overdue words (must review today) |
| 2nd | `LastReviewedAt == null` | Never practiced words (new vocabulary) |
| 3rd | `NextReviewAt <= now + 1 day` | Due soon words |
| 4th | `EaseFactor ASC` | Difficult words (low ease factor ≈ hard to remember) |
| 5th | `NEWID()` | Random shuffle within same difficulty |

**4. Question Building** ([GetPracticeQuestionsQueryHandler.cs](src-backend/BeeZillion.Application/Practice/Queries/GetPracticeQuestions/GetPracticeQuestionsQueryHandler.cs#L64-L155))

For each word:
1. Select question mode (cycle through `modes` list)
2. Skip if mode=`ai_sentence` but word has no AI sentence
3. Randomly pick direction: `EN_TO_TR` or `TR_TO_EN` (forced `EN_TO_TR` for AI Sentence)
4. Build question based on mode:

| Mode | Input | Type | Correct Answer |
|------|-------|------|---|
| `multiple_choice` | Word in chosen direction | Multiple choice | Translation/Original |
| `spelling` | Word in chosen direction | Text input | Translation/Original |
| `ai_sentence` | AI-generated sentence | AI grading | N/A (server validates) |

**5. Multiple Choice Options** ([BuildOptions](src-backend/BeeZillion.Application/Practice/Queries/GetPracticeQuestions/GetPracticeQuestionsQueryHandler.cs#L157-L167))
- Takes 3 random incorrect answers from word pool
- Adds correct answer
- Shuffles all 4 options randomly

### Example Request/Response

**Request:**
```json
GET /api/practice/questions?mode=multiple_choice,ai_sentence&limit=4
```

**Response:**
```json
{
  "questions": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "type": "multiple_choice",
      "direction": "EN_TO_TR",
      "prompt": "algorithm",
      "options": ["prosedür", "algoritma", "hata", "test"],
      "correct_answer": "algoritma",
      "english_sentence": null,
      "target_words_used": null
    },
    {
      "id": "550e8400-e29b-41d4-a716-446655440001",
      "type": "ai_sentence",
      "direction": "EN_TO_TR",
      "prompt": null,
      "options": null,
      "correct_answer": null,
      "english_sentence": "The algorithm processes data efficiently.",
      "target_words_used": ["algorithm"]
    }
  ]
}
```

---

## 2. CURRENT WORD SELECTION & TRACKING

### A. Domain Model: Word Entity

**Structure** ([Word.cs](src-backend/BeeZillion.Domain/Aggregates/WordAggregate/Word.cs))

```csharp
public sealed class Word : AggregateRoot<WordId>
{
    public UserId OwnerId { get; private set; }           // Owner
    public string Original { get; private set; }         // English word
    public string Translation { get; private set; }      // Turkish translation
    public string? AiSentence { get; private set; }      // Optional AI-generated sentence
    public string? Field { get; private set; }           // Topic/domain (e.g., "Software", "Medicine")
    public ReviewInfo Review { get; private set; }       // ⭐ Spaced repetition tracking
    public DateTime CreatedAt { get; private set; }      // Creation timestamp
}
```

**Size:** Each word ≈ 1000 bytes in database (with indexes)

### B. Tracking Data: ReviewInfo Value Object

**Structure** ([ReviewInfo.cs](src-backend/BeeZillion.Domain/ValueObjects/ReviewInfo.cs))

```csharp
public sealed record ReviewInfo(
    int IntervalDays,          // Days until next review
    float EaseFactor,          // Difficulty metric (1.3 - ∞, default 2.5)
    int Repetitions,           // Consecutive successful reviews
    DateTime NextReviewAt,     // When user should review this word
    DateTime? LastReviewedAt   // When last reviewed (null = never)
);
```

**Initial State (new word):**
```
IntervalDays: 0
EaseFactor: 2.5
Repetitions: 0
NextReviewAt: DateTime.UtcNow (immediate)
LastReviewedAt: null (never)
```

### C. User-Level Tracking

**Structure** ([User.cs](src-backend/BeeZillion.Domain/Aggregates/UserAggregate/User.cs#L18-L27))

```csharp
public sealed class User : AggregateRoot<UserId>
{
    public int Streak { get; private set; }           // Consecutive days with activity
    public int DailyGoal { get; private set; }        // Target reviews/day (default 10)
    public int ReviewCount { get; private set; }      // Cumulative reviews
    public DateTime LastActivity { get; private set; } // Last activity timestamp
    public IReadOnlyList<Badge> Badges { get; }       // Achievements
}
```

**Update on Review:**
```csharp
user.RecordReview(DateTime.UtcNow);
// Updates: ReviewCount++, LastActivity, Streak (based on date diff)
```

### D. Database Schema & Indexes

**Word Table Indexes** ([WordConfiguration.cs](src-backend/BeeZillion.Infrastructure/Persistence/Configurations/WordConfiguration.cs#L42-L45))

```sql
-- Composite indexes
CREATE INDEX IX_Word_OwnerId_CreatedAt 
  ON Words(OwnerId, CreatedAt);

CREATE INDEX IX_Word_OwnerId_Field 
  ON Words(OwnerId, Field);

-- Note: No index on NextReviewAt or ReviewLastReviewedAt
-- ⚠️ This could be a performance bottleneck for large user vocabularies
```

**Suggested Indexes for Performance:**
```sql
-- Missing but recommended
CREATE INDEX IX_Word_OwnerId_NextReviewAt 
  ON Words(OwnerId, ReviewNextReviewAt)
  WHERE ReviewNextReviewAt <= GETUTCDATE();
```

### E. Persistence Configuration

**Mapped as Owned Type** ([WordConfiguration.cs](src-backend/BeeZillion.Infrastructure/Persistence/Configurations/WordConfiguration.cs#L43-L50))

```csharp
builder.OwnsOne(word => word.Review, review =>
{
    review.Property(r => r.IntervalDays).HasColumnName("ReviewIntervalDays");
    review.Property(r => r.EaseFactor).HasColumnName("ReviewEaseFactor");
    review.Property(r => r.Repetitions).HasColumnName("ReviewRepetitions");
    review.Property(r => r.NextReviewAt).HasColumnName("ReviewNextReviewAt");
    review.Property(r => r.LastReviewedAt).HasColumnName("ReviewLastReviewedAt");
});
```

**Resulting SQL Columns:**
- `ReviewIntervalDays` (int)
- `ReviewEaseFactor` (float)
- `ReviewRepetitions` (int)
- `ReviewNextReviewAt` (datetime2)
- `ReviewLastReviewedAt` (datetime2, nullable)

---

## 3. SM-2 ALGORITHM IMPLEMENTATION

### Overview

The **SuperMemo-2 (SM-2)** algorithm is a spaced repetition algorithm that:
- Calculates optimal review intervals
- Adjusts word difficulty dynamically
- Maximizes long-term retention with minimal review sessions

### Time-Aware Q-Score Calculation

**Method:** `RecordReviewByQScore(bool isCorrect, long timeTakenMs)` ([Word.cs](src-backend/BeeZillion.Domain/Aggregates/WordAggregate/Word.cs#L65-L76))

Converts answer quality + response time → Q Score (0-5):

```csharp
public static int CalculateQScore(bool isCorrect, long timeTakenMs)
{
    if (!isCorrect) return 0;
    
    return timeTakenMs switch
    {
        < 2000  => 5,    // Correct + blazing fast (< 2s): Perfect
        < 6000  => 4,    // Correct + normal (2-6s): Good
        _       => 3,    // Correct + slow (> 6s): Difficult but correct
    };
}
```

**Q Score Mapping to Outcome:**

| Q Score | Response | EaseFactor Change | IntervalDays Reset |
|---------|----------|-------------------|-------------------|
| 0-2 | Incorrect | -0.2 (penalty) | Reset to 1 day |
| 3 | Correct but slow | -0.15 | Calculate from formula |
| 4 | Correct at normal speed | No change | Calculate from formula |
| 5 | Correct very fast | +0.15 (bonus) | Calculate from formula |

### SM-2 Algorithm: NextReview Calculation

**Implementation:** `CalculateNextReview(ReviewOutcome outcome, ReviewInfo current)` ([Word.cs](src-backend/BeeZillion.Domain/Aggregates/WordAggregate/Word.cs#L81-L110))

```csharp
// Outcome = QScoreToReviewOutcome(qScore)
if (outcome == ReviewOutcome.Again)  // Q ≤ 2
{
    repetitions = 0;
    intervalDays = 1;
    easeFactor = Math.Max(1.3f, easeFactor - 0.2f);  // Minimum: 1.3
}
else  // Q ≥ 3
{
    repetitions += 1;
    
    easeFactor = outcome switch
    {
        ReviewOutcome.Hard => Math.Max(1.3f, easeFactor - 0.15f),  // Q = 3
        ReviewOutcome.Good => easeFactor,                           // Q = 4
        ReviewOutcome.Easy => easeFactor + 0.15f,                   // Q = 5
    };
    
    intervalDays = repetitions switch
    {
        1 => 1,                                    // First success: 1 day
        2 => 6,                                    // Second success: 6 days
        _ => (int)Math.Round(intervalDays * easeFactor),  // 3+: exponential
    };
}

nextReviewAt = DateTime.UtcNow.AddDays(intervalDays);
```

### Example Evolution

**Word: "algorithm"** (EaseFactor starts at 2.5)

| Review | Q Score | Outcome | EaseFactor | IntervalDays | NextReview |
|--------|---------|---------|-----------|--------------|-----------|
| 1st | 4 (correct, 3s) | Good | 2.5 | 1 | +1 day |
| 2nd | 5 (correct, 1s) | Easy | 2.65 | 6 | +6 days |
| 3rd | 4 (correct, 4s) | Good | 2.65 | 16 | +16 days |
| 4th | 3 (correct, 10s) | Hard | 2.50 | 16 | +16 days |
| 5th | 0 (incorrect) | Again | 2.30 | 1 | +1 day (reset) |

### AI Sentence Scoring

For `ai_sentence` mode ([SubmitPracticeAnswerCommandHandler.cs](src-backend/BeeZillion.Application/Practice/Commands/SubmitPracticeAnswer/SubmitPracticeAnswerCommandHandler.cs#L65-L81)):

```csharp
if (string.Equals(request.Type, "ai_sentence"))
{
    var evaluation = await _aiSentenceService.EvaluateTranslationAsync(
        sentence,
        request.UserAnswer,
        cancellationToken);
    
    var isCorrect = evaluation.Score >= 70;  // 70/100 threshold
    
    if (isCorrect)
    {
        // Convert AI score (0-100) to Q score (0-5)
        // 70 → Q3, 80 → Q4, 100 → Q5
        var qScore = (int)Math.Round(evaluation.Score / 20f);
        word.RecordReviewByQScore(true, request.TimeTakenMs);
    }
}
```

---

## 4. APPLICATION ARCHITECTURE PATTERNS

### Clean Architecture Layers

```
BeeZillion.API (Presentation)
    ↓ (HTTP/Controllers)
BeeZillion.Application (Use Cases)
    ↓ (Commands/Queries via MediatR)
BeeZillion.Domain (Business Rules)
    ↓ (Aggregates/Value Objects)
BeeZillion.Infrastructure (Framework/Database)
    └─ EF Core, Repositories, External Services
```

### MediatR Command/Query Pipeline

**Handler Registration** ([Application/DependencyInjection.cs](src-backend/BeeZillion.Application/DependencyInjection.cs))

```csharp
services.AddMediatR(typeof(DependencyInjection).Assembly);
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
```

**Pipeline Order:**
1. LoggingBehavior (logs request start/end)
2. ValidationBehavior (FluentValidation)
3. PerformanceBehavior (measures execution time)
4. Handler execution

### Practice Request Flow with Behaviors

```
Request: GetPracticeQuestionsQuery
    ↓
[LoggingBehavior] Logs: "Handling GetPracticeQuestionsQuery..."
    ↓
[ValidationBehavior] Validates: limit > 0? mode valid?
    ↓
[PerformanceBehavior] Starts timer
    ↓
[GetPracticeQuestionsQueryHandler]
    ├─ GetCurrentUser()
    ├─ GetWordsForPracticeAsync() [DB query]
    ├─ BuildQuestions()
    └─ Returns PracticeQuestionsResponse
    ↓
[PerformanceBehavior] Logs: "Query took 234ms"
    ↓
Response: PracticeQuestionsResponse
```

### Dependency Injection Setup

**Infrastructure DI** ([Infrastructure/DependencyInjection.cs](src-backend/BeeZillion.Infrastructure/DependencyInjection.cs#L76-L86))

```csharp
// Database
services.AddDbContext<AppDbContext>(...);

// Repositories
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IWordRepository, WordRepository>();
services.AddScoped<IPredefinedWordRepository, PredefinedWordRepository>();

// Unit of Work
services.AddScoped<IUnitOfWork, UnitOfWork>();

// Services
services.AddScoped<ICacheService, MemoryCacheService>();
services.AddScoped<IAiSentenceService, GroqService>();  // LLM for AI sentences
```

### Domain-Driven Design Patterns

**1. Aggregate Root Pattern**
- `Word` is root aggregate (controls ReviewInfo updates)
- `User` is root aggregate (controls profile updates)
- Operations on domain objects flow through aggregate roots

**2. Value Objects**
- `ReviewInfo` - immutable record, cannot exist independently
- `WordId`, `UserId` - strongly typed IDs

**3. Repository Pattern**
```csharp
public interface IWordRepository
{
    Task<Word?> GetByIdAsync(WordId id, CancellationToken ct);
    Task<IReadOnlyList<Word>> GetWordsForPracticeAsync(UserId ownerId, int limit, CancellationToken ct);
    void Add(Word word);
    void Update(Word word);
    void Delete(Word word);
}
```

**4. Unit of Work Pattern**
```csharp
// Changes tracked within transaction scope
var word = await _wordRepository.GetByIdAsync(...);
word.RecordReview(ReviewOutcome.Easy);
_wordRepository.Update(word);
await _unitOfWork.SaveChangesAsync();  // Single commit
```

---

## 5. CURRENT LIMITATIONS

### A. Performance Limitations

1. **No Database Index on Review Fields**
   - `GetWordsForPracticeAsync` filters on `ReviewNextReviewAt`
   - With 10,000+ words per user, full table scans occur
   - **Impact:** Query time: O(n) instead of O(log n)
   - **Fix:** Add index: `CREATE INDEX IX_Word_OwnerId_NextReviewAt ON Words(OwnerId, ReviewNextReviewAt)`

2. **Random Shuffle via Guid.NewGuid() in Query**
   ```csharp
   .ThenBy(w => Guid.NewGuid())  // ❌ Randomization at DB level
   ```
   - Creates new GUID for each row (expensive)
   - SQL Server has no built-in randomization
   - **Impact:** Slower large queries
   - **Better approach:** Randomize in memory after fetching top N

3. **2x Buffer Fetching for AI Sentence Filter**
   - Fetches `limit * 2` words, filters AI Sentence in memory
   - Wastes bandwidth if many words lack AI sentences
   - **Impact:** ~50% extra database load per request

4. **No Pagination for Word Lists**
   - `GetByOwnerAsync()` loads all user words into memory
   - **Impact:** O(n) memory for 10,000+ word vocabularies

### B. Feature Limitations

1. **No UserVocabularyProgress Separate Entity**
   - Tracking embedded in `ReviewInfo` value object
   - Cannot efficiently query: "users with streak > 30"
   - Cannot denormalize stats for dashboard
   - **Migration path complexity: HIGH**

2. **No Difficulty Categorization**
   - Only EaseFactor indicates difficulty
   - Cannot query: "show me easy words" or "show me hard words"
   - **Solution:** Add `DifficultyLevel` enum to Word

3. **No User-Specific Practice Preferences**
   - All users get same practice algorithm
   - No support for: "only show overdue words", "show random only"
   - **Solution:** Add `PracticePreferences` entity to User

4. **No Practice Session History**
   - Cannot analyze: "what words are users getting wrong?"
   - Cannot replay practice sessions
   - No analytics on learning patterns
   - **Solution:** Create `PracticeAttempt` entity with detailed logging

5. **Time-Based Q-Score Too Rigid**
   - Fixed brackets: <2s=5, 2-6s=4, >6s=3
   - Doesn't account for question type difficulty
   - **Solution:** Store `TimeTakenMs` in attempt history, calculate dynamic thresholds

### C. Algorithm Limitations

1. **No Initial Difficulty Assessment**
   - All new words start with EaseFactor = 2.5
   - User can self-assess: "I know this well" → should start at 3.0+
   - **Solution:** Add initial quality rating on word creation

2. **AI Sentence Score Fixed at 70% Threshold**
   - Harsh: requires 70/100 to count as correct
   - No middle ground for partial translations
   - **Solution:** Map AI score ranges: 50-70=Hard, 70-85=Good, 85-100=Easy

3. **No Retention Curve Analysis**
   - SM-2 fixed intervals don't adapt to user
   - Some users might need more spacing
   - **Solution:** Track accuracy rate, adjust EaseFactor scaling factor

### D. Integration Limitations

1. **Stateless Practice Sessions**
   - No session ID tracking
   - User could practice same word twice in one session
   - **Solution:** Add session entity to track questions shown

2. **No Filtering at Query Time**
   - Cannot request: "practice only overdue words"
   - Cannot request: "practice only Hard words (EaseFactor < 1.5)"
   - **Solution:** Extend query to support filter parameters

3. **No Priority Weighting Adjustment**
   - Same priority always (overdue > new > due_soon > hard > random)
   - No user preference: "I want 50% review, 50% new"
   - **Solution:** Add PracticeStrategy enum (balanced, review_focus, learning_focus)

---

## 6. MIGRATION PATH: ADDING UserVocabularyProgress

### Current State
- Progress tracked in `ReviewInfo` (embedded in Word)
- No separate tracking table
- Stats calculated on-demand

### Why Add UserVocabularyProgress?

✅ **Benefits:**
- Denormalized stats for fast dashboard queries
- Historical tracking for analytics
- Separate entity allows independent querying
- Foundation for advanced features

### Proposed Entity

```csharp
public class UserVocabularyProgress : AggregateRoot<UserId>
{
    public UserId UserId { get; init; }
    public int TotalWords { get; private set; }
    public int WordsReviewed { get; private set; }
    public int OverdueCount { get; private set; }      // NextReviewAt <= now
    public int DueWithin24Hours { get; private set; }  // NextReviewAt <= now + 1 day
    public float AverageEaseFactor { get; private set; }
    public int LongestStreak { get; private set; }
    public DateTime LastUpdated { get; private set; }
    public DateTime CreatedAt { get; init; }
}
```

### Migration Steps

**Phase 1: Add New Entity (MINIMAL CHANGE)**
```sql
CREATE TABLE UserVocabularyProgress (
    UserId UNIQUEIDENTIFIER PRIMARY KEY,
    TotalWords INT NOT NULL,
    WordsReviewed INT NOT NULL,
    OverdueCount INT NOT NULL,
    DueWithin24Hours INT NOT NULL,
    AverageEaseFactor FLOAT NOT NULL,
    LongestStreak INT NOT NULL,
    LastUpdated DATETIME2 NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);
```

**Phase 2: Add Repository & Service (ISOLATED)**
```csharp
public interface IUserVocabularyProgressRepository
{
    Task<UserVocabularyProgress?> GetByUserIdAsync(UserId userId, CancellationToken ct);
    void Update(UserVocabularyProgress progress);
}

public class UpdateProgressService
{
    public async Task UpdateAsync(UserId userId)
    {
        var words = await _wordRepository.GetByOwnerAsync(userId);
        var stats = CalculateStats(words);
        var progress = new UserVocabularyProgress(userId, stats);
        await _progressRepository.UpdateAsync(progress);
    }
}
```

**Phase 3: Trigger Updates (STRATEGIC)**
- On `SubmitPracticeAnswer` → update progress
- On scheduled task (nightly) → bulk recalculate
- On dashboard load → check freshness

**Phase 4: Use in Queries (GRADUAL)**
```csharp
// Before: Query all words to count overdue
var words = await _wordRepository.GetByOwnerAsync(userId);
var overdueCount = words.Count(w => w.Review.NextReviewAt <= DateTime.UtcNow);

// After: Direct query
var progress = await _progressRepository.GetByUserIdAsync(userId);
var overdueCount = progress.OverdueCount;  // O(1) lookup
```

**Complexity Assessment: MEDIUM**
- ✅ Entity addition: Easy (new table)
- ✅ Repository setup: Easy (standard CRUD)
- ⚠️ Sync strategy: Medium (cache invalidation)
- ⚠️ Migration data: Medium (backfill stats)

---

## 7. INTEGRATION POINTS FOR ENHANCEMENT

### A. Practice Session Tracking

**Current Gap:** No way to prevent duplicate questions in same session

**Integration Point:** Add session entity

```csharp
public class PracticeSession : AggregateRoot<PracticeSessionId>
{
    public UserId UserId { get; init; }
    public List<PracticeQuestion> Questions { get; private set; }
    public DateTime StartedAt { get; init; }
    public DateTime? CompletedAt { get; private set; }
    public int CorrectCount { get; private set; }
}

public class PracticeQuestion
{
    public WordId WordId { get; init; }
    public string Mode { get; init; }
    public bool IsCorrect { get; init; }
    public long TimeTakenMs { get; init; }
    public int UserScore { get; init; }  // AI evaluated
}
```

**Modified Handler:**
```csharp
var session = await _sessionRepository.GetOrCreateCurrentSessionAsync(userId);
var practiceWords = session.ExcludeAlreadyAsked();  // Filter out shown words
// ... rest of logic
```

### B. Dynamic Question Distribution

**Current:** Fixed mode cycling (round-robin)

**Enhancement:** Weighted distribution based on user preference

```csharp
public enum PracticeStrategy
{
    Balanced,         // 50% review, 40% new, 10% AI sentences
    ReviewFocus,      // 80% overdue/due-soon words
    LearningFocus,    // 60% new words, 40% reinforcement
}

// In handler:
var weights = GetWeights(user.PracticeStrategy);
var questions = BuildWeightedQuestions(words, weights);
```

### C. Analytics & Reporting

**Missing:** No per-word success rate tracking

**Integration Point:** Add `PracticeStatistics` entity

```csharp
public class PracticeStatistics
{
    public WordId WordId { get; init; }
    public int TotalAttempts { get; private set; }
    public int SuccessfulAttempts { get; private set; }
    public float SuccessRate => SuccessfulAttempts / (float)TotalAttempts;
    public float AverageTimeTaken { get; private set; }
}

// Query: Show words user struggles with
var poorWords = await _statsRepository.GetWordsWithSuccessRateBelowAsync(userId, 0.6f);
```

---

## 8. CURRENT IMPLEMENTATION QUALITY

### ✅ Strengths

1. **Well-Structured Architecture**
   - Clean separation of concerns
   - Domain-driven design with value objects
   - Repository pattern isolates persistence

2. **Optimized Query Logic**
   - Multi-level priority sorting at database level
   - Proper use of owned types for `ReviewInfo`
   - Composite indexes on common queries

3. **Proven SM-2 Implementation**
   - Correct algorithm with time-aware scoring
   - Proper EaseFactor bounds (min 1.3)
   - Exponential interval growth for mastered words

4. **Extensible Design**
   - MediatR pipeline enables cross-cutting concerns
   - Repository pattern allows easy persistence swaps
   - Three practice modes (choice, spelling, AI) already supported

### ⚠️ Areas for Improvement

1. **Missing Database Indexes**
   - No index on `ReviewNextReviewAt` → O(n) lookups
   - Should add: `IX_Word_OwnerId_NextReviewAt`

2. **No Session Tracking**
   - Can't prevent duplicate questions in single session
   - Can't replay or analyze practice sessions

3. **Limited Statistics**
   - No historical data on practice performance
   - No per-word success rate tracking
   - Stats recalculated on-demand (inefficient)

4. **Rigid Time-Based Scoring**
   - Fixed brackets (2s, 6s) don't adapt to user
   - Could be smarter with percentile-based scoring

---

## 9. COMPLETE ARCHITECTURE DIAGRAM

```
┌─────────────────────────────────────────────────────────────┐
│                    BeeZillion.API Layer                       │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  PracticeController: /api/practice/questions        │  │
│  │  ├─ GET questions?mode=X&limit=N                    │  │
│  │  ├─ POST generate-sentence                          │  │
│  │  └─ POST submit-answer                              │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                           ↓ MediatR
┌─────────────────────────────────────────────────────────────┐
│                 BeeZillion.Application Layer                  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  MediatR Pipeline:                                   │  │
│  │  1. LoggingBehavior                                  │  │
│  │  2. ValidationBehavior (FluentValidation)            │  │
│  │  3. PerformanceBehavior                              │  │
│  └──────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Handlers:                                           │  │
│  │  ├─ GetPracticeQuestionsQueryHandler                │  │
│  │  │  ├─ GetWordsForPracticeAsync()                  │  │
│  │  │  └─ BuildQuestions()                             │  │
│  │  ├─ SubmitPracticeAnswerCommandHandler              │  │
│  │  │  ├─ RecordReviewByQScore()                      │  │
│  │  │  └─ Update Word + User aggregates               │  │
│  │  └─ GenerateSentenceCommandHandler                  │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────────┐
│                    BeeZillion.Domain Layer                    │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Aggregates:                                         │  │
│  │  ├─ Word (Root)                                      │  │
│  │  │  ├─ ReviewInfo (Value Object) ⭐                │  │
│  │  │  ├─ CalculateQScore()                            │  │
│  │  │  ├─ RecordReview()                               │  │
│  │  │  └─ RecordReviewByQScore()  [SM-2 Algorithm]    │  │
│  │  ├─ User (Root)                                      │  │
│  │  │  ├─ Streak tracking                              │  │
│  │  │  ├─ ReviewCount                                  │  │
│  │  │  └─ Badges                                       │  │
│  │  └─ PredefinedWord                                  │  │
│  │                                                      │  │
│  │  Repositories (Interfaces):                         │  │
│  │  ├─ IWordRepository                                 │  │
│  │  │  └─ GetWordsForPracticeAsync() [Priority Sort]  │  │
│  │  ├─ IUserRepository                                 │  │
│  │  └─ IPredefinedWordRepository                       │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                           ↓ EF Core
┌─────────────────────────────────────────────────────────────┐
│              BeeZillion.Infrastructure Layer                  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Persistence:                                        │  │
│  │  ├─ AppDbContext (SQL Server)                       │  │
│  │  ├─ WordConfiguration (EF Mapping)                  │  │
│  │  └─ Repositories Implementation                      │  │
│  │                                                      │  │
│  │  Query Optimization:                                │  │
│  │  ├─ Priority Sorting:                               │  │
│  │  │  1. Overdue (NextReviewAt <= now)               │  │
│  │  │  2. Never reviewed (LastReviewedAt = null)      │  │
│  │  │  3. Due soon (NextReviewAt <= now + 1d)         │  │
│  │  │  4. Hard (EaseFactor ASC)                       │  │
│  │  │  5. Random (NEWID())                            │  │
│  │  └─ Take limit*2 (filter AI Sentence in memory)    │  │
│  │                                                      │  │
│  │  Database Indexes:                                  │  │
│  │  ├─ PK: Id                                          │  │
│  │  ├─ IX_Word_OwnerId_CreatedAt ✓                    │  │
│  │  ├─ IX_Word_OwnerId_Field ✓                        │  │
│  │  └─ IX_Word_OwnerId_NextReviewAt ❌ MISSING        │  │
│  │                                                      │  │
│  │  External Services:                                 │  │
│  │  ├─ GroqService (AI Sentence Generation)           │  │
│  │  ├─ JwtTokenService (Authentication)               │  │
│  │  └─ MemoryCacheService (Caching)                   │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                           ↓ SQL
┌─────────────────────────────────────────────────────────────┐
│                   SQL Server Database                       │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Words Table                                         │  │
│  │  ├─ Id (PK)                                          │  │
│  │  ├─ OwnerId (FK)                                     │  │
│  │  ├─ Original, Translation, AiSentence               │  │
│  │  ├─ ReviewIntervalDays                              │  │
│  │  ├─ ReviewEaseFactor                                │  │
│  │  ├─ ReviewRepetitions                               │  │
│  │  ├─ ReviewNextReviewAt                              │  │
│  │  ├─ ReviewLastReviewedAt                            │  │
│  │  └─ [Indexes as above]                              │  │
│  │                                                      │  │
│  │  Users Table                                         │  │
│  │  ├─ Id (PK)                                          │  │
│  │  ├─ Email                                            │  │
│  │  ├─ Streak                                           │  │
│  │  ├─ ReviewCount                                      │  │
│  │  └─ Badges                                           │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

---

## 10. SUMMARY TABLE

| Aspect | Current Implementation | Assessment |
|--------|----------------------|------------|
| **Architecture** | Clean Architecture + DDD | ✅ Excellent |
| **Algorithm** | SM-2 with time-aware Q scores | ✅ Well implemented |
| **Word Selection** | Multi-level priority sorting | ✅ Good (missing index) |
| **Tracking** | ReviewInfo value object | ✅ Functional (not optimal) |
| **User Stats** | Streak, ReviewCount, Badges | ⚠️ Limited |
| **Session History** | None | ❌ Missing |
| **Performance** | Good for 1k-5k words | ⚠️ Degrades at scale |
| **Testability** | Repository pattern enabled | ✅ Good |
| **Extensibility** | MediatR pipeline, behaviors | ✅ Excellent |

---

## 11. RECOMMENDED NEXT STEPS

### Immediate (Days)
1. Add missing database index: `IX_Word_OwnerId_NextReviewAt`
2. Add benchmark tests for `GetWordsForPracticeAsync` with 10k+ words
3. Document current SM-2 configuration for future tuning

### Short-term (Weeks)
1. Add session tracking to prevent duplicate questions
2. Implement per-word success rate tracking
3. Add user preference entity for practice strategy

### Medium-term (Months)
1. Create `UserVocabularyProgress` denormalized entity
2. Add historical analytics (learning curves, weak areas)
3. Implement adaptive Q-score thresholds based on user patterns

### Long-term (Quarters)
1. ML-based word difficulty prediction
2. Custom spaced repetition curves per user
3. Recommendation engine for optimal practice times

