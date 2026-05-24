# 📊 BeeZillion Backend - Detaylı Mimari Analiz Raporu

## 📋 İçerik
1. [Project Architecture](#proje-mimarisi)
2. [CQRS / MediatR](#cqrs--mediatr-kullanimi)
3. [DDD Patterns](#ddd-patterns)
4. [Cache Implementation](#cache-implementation)
5. [Controllers](#controllers-8-adet)
6. [Services](#services)
7. [Middleware](#middleware)
8. [Dependency Injection](#dependency-injection)
9. [🔴 Bulunmuş Sorunlar](#-bulunus-sorunlar)

---

## Proje Mimarisi

### Katman Yapısı (4-Tier Clean Architecture)

```
BeeZillion.API (Presentation Layer)
    ├── Controllers/ (8 controller)
    ├── Middleware/ (Exception Handling)
    ├── Services/ (CurrentUserService)
    └── DependencyInjection.cs

BeeZillion.Application (Application/Business Layer)
    ├── Auth/ (Commands: Login, Register + Dtos)
    ├── Words/ (Commands: 7 item, Queries: 4 item)
    ├── Practice/ (Commands: 2 item, Queries: 1 item)
    ├── PredefinedWords/ (Commands: 1 item, Queries: 2 item)
    ├── Users/ (Queries: 2 item)
    └── Common/ (Behaviors, Exceptions, Events, Interfaces)

BeeZillion.Domain (Domain Layer - DDD)
    ├── Aggregates/ (User, Word, PredefinedWord)
    ├── ValueObjects/ (UserId, WordId, ReviewInfo, vb.)
    ├── Events/ (Domain Events)
    ├── Entities/ (Badge, UserVocabularyProgress)
    ├── Repositories/ (Interfaces only)
    └── Enums/ (ReviewOutcome, BadgeType)

BeeZillion.Infrastructure (Infrastructure/Persistence Layer)
    ├── Persistence/ (EF Core DbContext, Repositories, UnitOfWork)
    ├── Auth/ (JWT Token, BCrypt Password Hashing)
    ├── AI/ (Groq Integration)
    ├── Cache/ (Memory Cache Service)
    └── Events/ (Domain Event Dispatcher)
```

---

## CQRS / MediatR Kullanımı ✅

### Yapı Standardı
**CQRS pattern tam olarak uygulanmış:**

```csharp
Command Pattern:
  Command → CommandHandler → IRequestHandler<TCommand, TResponse>
  
Query Pattern:
  Query → QueryHandler → IRequestHandler<TQuery, TResponse>

Example Command Structure:
  Auth/Commands/Login/
    ├── LoginCommand.cs
    ├── LoginCommandHandler.cs
    └── LoginCommandValidator.cs
```

### Commands (9 toplam):
- **Auth:** Register, Login
- **Words:** CreateWord, DeleteWord, RecordReview, BulkGenerate, BulkDeleteByField
- **Practice:** GenerateSentence, SubmitPracticeAnswer
- **PredefinedWords:** ImportPredefinedWordsByField

### Queries (9 toplam):
- **Words:** GetWordList, GetWordOfDay, GetReviewSessionWords, GetReviewWords
- **Practice:** GetPracticeQuestions
- **PredefinedWords:** GetFieldsList, GetPredefinedWordsByField
- **Users:** GetDashboard, GetStats

### MediatR Pipeline Behaviors (3 adet):
1. **ValidationBehavior** → FluentValidation integration ✅
2. **PerformanceBehavior** → Slow request logging (>500ms) ✅
3. **LoggingBehavior** → Request/Response logging ✅

---

## DDD Patterns

### Aggregates (3 root):

#### 🟦 User Aggregate
```csharp
public sealed class User : AggregateRoot<UserId>
{
    // Value Objects
    public Email { get; }
    public int Streak { get; }
    public int DailyGoal { get; }
    
    // Aggregate functionality
    public void RecordActivity(DateTime utcNow)
    public int RecordReview(DateTime utcNow)
    public void AwardBadge(BadgeType badgeType) // Domain Event trigger
    
    // Private constructor + Factory method (DDD pattern)
    private User() { }
    public static User Create(string email, string passwordHash)
}
```
**Strengths:** Proper encapsulation, domain logic, event raising

#### 🟦 Word Aggregate
```csharp
public sealed class Word : AggregateRoot<WordId>
{
    // Value Objects
    public UserId OwnerId { get; }
    public ReviewInfo Review { get; } // SM-2 Algorithm embedded
    
    // Business logic
    public void RecordReview(ReviewOutcome outcome)
    public void RecordReviewByQScore(bool isCorrect, long timeTakenMs)
    public void AttachAiSentence(string sentence)
    
    // Spaced Repetition Implementation
    private static ReviewInfo CalculateNextReview(ReviewOutcome outcome, ReviewInfo current)
    {
        // SM-2 Algoritması:
        // - Repetitions tracking
        // - Ease Factor calculation (1.3 - 2.5)
        // - Interval Days calculation
    }
}
```
**Advanced:** SM-2 spaced repetition logic, QScore time-aware learning

#### 🟦 PredefinedWord Aggregate
- Simpler aggregate
- IsActive flag for soft delete pattern

### Value Objects (6 adet):

| Value Object | Purpose |
|---|---|
| **UserId** | User identity |
| **WordId** | Word identity |
| **PredefinedWordId** | Predefined word identity |
| **UserVocabularyProgressId** | Progress tracking |
| **ReviewInfo** | SM-2 state (EaseFactor, Repetitions, IntervalDays, NextReviewAt) |
| **Field** | Word categorization (Software, Medicine, Law) |

### Domain Events (3 adet):

```csharp
public class BadgeEarnedEvent : IDomainEvent
public class UserStreakUpdatedEvent : IDomainEvent
public class WordReviewedEvent : IDomainEvent
```

**Event Dispatcher:** EF Core SaveChangesInterceptor ile otomatik dispatch ✅

### Repository Pattern:

```csharp
// Domain interfaces (Abstraction)
IUserRepository
IWordRepository
IPredefinedWordRepository
IUserVocabularyProgressRepository
IUnitOfWork

// Infrastructure implementations
UserRepository : IUserRepository
WordRepository : IWordRepository
// ... (proper async repository pattern)
```

---

## Cache Implementation

### Current Implementation:
- **Type:** In-Memory Cache (Microsoft.Extensions.Caching.Memory)
- **Service:** MemoryCacheService.cs

### Usage Examples:

**1. Session Cache (GetReviewSessionWords):**
```csharp
var sessionCacheKey = $"review-session:{userId.Value}:{today:yyyy-MM-dd}";
var shownWordIds = await _cacheService.GetAsync<List<Guid>>(sessionCacheKey, cancellationToken);
// TTL: Midnight'a kadar (cache reset each day)
```

**⚠️ Issue:** Cache invalidation strategy limited. No distributed cache for scaling.

---

## Controllers (8 Adet)

| Controller | Auth | Endpoints | Purpose |
|---|---|---|---|
| **AuthController** | ❌ | POST /register, POST /login | User authentication |
| **WordsController** | ✅ | 8 endpoints | CRUD + Reviews |
| **PracticeController** | ✅ | 3 endpoints | Practice questions & sentence generation |
| **DashboardController** | ✅ | 1 endpoint | User stats dashboard |
| **StatsController** | ✅ | 1 endpoint | Detailed statistics |
| **PredefinedWordsController** | Mixed | 3 endpoints | Predefined word library |
| **HealthController** | ❌ | GET /health | Health check |
| **DevController** | ❌ | POST /dev/seed | **DEBUG only** - Test data seeding |

### DevController Security:
```csharp
#if DEBUG
/// Only available in DEBUG builds. Completely excluded from Release.
#endif
```
✅ **Good:** Conditional compilation ensures excluded from Release builds

---

## Services

### API Layer Services:

**CurrentUserService** - JWT token'dan UserId extraction
```csharp
public UserId GetUserId()
{
    var value = user?.FindFirstValue(ClaimTypes.NameIdentifier)
             ?? user?.FindFirstValue(JwtRegisteredClaimNames.Sub);
    return new UserId(userId);
}
```

### Infrastructure Layer Services:

**1. JWT Token Service**
```csharp
IJwtTokenService : IPasswordHasher
- GenerateToken(User)
- 7 days expiry (configurable)
```

**2. Password Hashing**
```csharp
IPasswordHasher (BCryptPasswordHasher)
- Secure password hashing
```

**3. AI Sentence Service** - Groq LLM Integration
```csharp
IAiSentenceService (GroqService)
- GenerateSentenceAsync(word) → B2 level English
- EvaluateTranslationAsync(englishSentence, turkishTranslation)
  → JSON response: {score: 0-100, feedback: string}
```

---

## Middleware

### ExceptionHandlingMiddleware
```csharp
Handles 3 custom exceptions:
- ValidationException → 422 Unprocessable Entity
- NotFoundException → 404 Not Found  
- ForbiddenException → 403 Forbidden
- Generic Exception → 500 Internal Server Error

Proper logging for 500 errors
```

### Authentication:
- JWT Bearer scheme ✅
- Token validation (issuer, audience, expiry) ✅
- ClockSkew: 1 minute ✅

---

## Dependency Injection

### Program.cs Setup Flow:

```
1. CORS Configuration
2. AddApplicationServices (MediatR, Validators, Behaviors)
3. AddInfrastructureServices (JWT, DbContext, Repositories, Cache)
4. AddApiServices (Controllers, Swagger, Health Checks)

DI Container Contents:
- DbContext (SQL Server)
- All Repositories (Scoped)
- UnitOfWork (Scoped)
- MediatR (Assembly scanning)
- JWT Token Service (Singleton)
- BCrypt Hashing (Singleton)
- AI Service (HttpClient + Polly retry)
- Memory Cache (Singleton)
- Health Checks
- Swagger/OpenAPI
```

### Polly Retry Policy for AI:
```csharp
HttpPolicyExtensions
  .HandleTransientHttpError()
  .WaitAndRetryAsync(
    retryCount: 3,
    sleepDurationProvider: exponential backoff)
```

---

# 🔴 Bulunmuş Sorunlar

## 🔴 KRITIK - Dead Code

### ❌ 1. Boşta Command Klasörleri (2 adet):
- `BeeZillion.Application/Words/Commands/RecordPracticeAnswer/` → **EMPTY** 
- `BeeZillion.Application/Words/Commands/RecordQuizAnswer/` → **EMPTY**

**Impact:** Bu commands hiç kullanılmıyor. Klasör yapısı var fakat handler dosyaları yok.

**Çözüm:** Bu boş klasörleri sil ve proje yapısını temizle.

---

## 🟡 ORTA SEVİYE - Performance Sorunları

### ⚠️ 1. N+1 Query Problemi (GetReviewSessionWords)
```csharp
// ❌ PROBLEM: Tüm words'ü belleğe yüklüyor
var allWords = await _wordRepository.GetByOwnerAsync(userId, cancellationToken);

// Sonra bellek içinde filtreleme yapıyor
var dueWords = allWords.Where(w => w.Review.NextReviewAt.Date <= today)
```

**Sorun:** Kullanıcının 5000 kelimesi varsa, hepsi yükleniyor sonra bellek içinde filtreleme yapılıyor.

**Çözüm:** Veritabanı seviyesinde filtreleme query'si ekle.

---

### ⚠️ 2. Bulk Generate Performance Sorunu
```csharp
public async Task<BulkGenerateResult> Handle(...)
{
    var words = await _wordRepository.GetWordsWithoutSentenceAsync(userId, ct);
    
    foreach (var word in words)  // ❌ N tane sırayla Groq API çağrısı
    {
        var sentence = await _aiSentenceService.GenerateSentenceAsync(word.Original, ct);
        word.AttachAiSentence(sentence);
        _wordRepository.Update(word);  // ❌ Her word ayrı update
    }
}
```

**Sorunlar:**
- Sırayla API çağrıları (paralelleştirilebilir)
- Her word ayrı güncelleniliyor

**Çözüm:** `Task.WhenAll()` kullanarak paralel işlem yap.

---

### ⚠️ 3. Cache Loading Null Check Hatası
```csharp
var shownWordIds = shownWordIdsObj ?? new List<Guid>();
```
Cache null döndürürse eksik null check var.

**Çözüm:** Proper null handling ekle veya cache default değer döndürsün.

---

## 🟡 ORTA SEVİYE - Mimari Sorunları

### ⚠️ 1. Sadece In-Memory Cache
```csharp
public sealed class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
}
```

**Sorun:** Distributed sistem için ölçeklenebilir değil (birden fazla API instance)

**Çözüm:** Production için Redis (IDistributedCache) implementasyonu yap.

---

### ⚠️ 2. Explicit Transaction Handling Yok
```csharp
// Örnek:
_wordRepository.Add(word);
_userRepository.Update(user);  // Bu fail olursa, word zaten eklenmişti
await _unitOfWork.SaveChangesAsync();
```

**Çözüm:** Kritik operasyonlar için explicit transaction wrapper kullan.

---

### ⚠️ 3. Basit AI Evaluation Fallback
```csharp
catch (JsonException ex)
{
    var defaultScore = userTranslation.Length > englishSentence.Length / 2 
        ? 60 
        : 30;  // ❌ Çok basit ve güvenilmez
}
```

**Sorun:** Fallback score sadece uzunluk oranına göre - güvenilmez değerlendirme.

**Çözüm:** Daha sağlam bir fallback mekanizması geliştir (örn: keyword matching).

---

## 🟢 MINOR - Kod Kalitesi Sorunları

### 📌 1. Hard-coded Magic Numbers
```csharp
// WordRepository.cs
.Take(limit * 2)  // Neden 2x?

// GetReviewSessionWords
var todayCount = (int)Math.Ceiling(limit * 0.7);  // %70 hard-coded
var futureCount = limit - todayCount;  // %30 hard-coded

// SubmitPracticeAnswerCommandHandler
var isCorrect = evaluation.Score >= 70;  // Neden 70?
```

**Çözüm:** Constantlara ekstrak et ve açıklamalarıyla dokümante et.

---

### 📌 2. Soft Delete Yok
```csharp
// Words sadece hard delete yapıyor
public void Delete(Word word)
{
    _dbContext.Words.Remove(word);  // Kalıcı silme
}
```

**Öneri:** Soft delete (IsActive flag) ekle - audit trail için.

---

### 📌 3. Bazı Handler'larda Input Validasyon Eksik
```csharp
// GenerateSentenceCommandHandler
if (request.TargetVocab == null || request.TargetVocab.Count == 0)
{
    throw new ArgumentException("target_vocab is required.");  // ✅ İyi
}

// Ama bazı handler'larda benzer checks yok
```

---

## 🟢 BİLGİ - Kod Tekrarı

### 📌 1. Review Recording Logic Tekrarı
```csharp
// CreateWordCommandHandler'da:
if (existingWords.Count == 0)
    user.AwardBadge(BadgeType.FirstWord);

// SubmitPracticeAnswerCommandHandler'da:
if (isCorrect)
{
    word.RecordReviewByQScore(true, request.TimeTakenMs);
    user.RecordReview(DateTime.UtcNow);
}

// Benzer pattern'ler handler'larda tekrarlanıyor
```

**Çözüm:** Domain service'e çıkar veya domain event handler'larını kullan.

---

## 📊 Kod Metrikleri Özeti

| Metrik | Değer | Durum |
|--------|-------|-------|
| **Controllers** | 8 | ✅ Makul |
| **Commands** | 9 | ✅ İyi organize |
| **Queries** | 9 | ✅ Net ayrılmış |
| **Domain Events** | 3 | ✅ Aktif kullanım |
| **Aggregates** | 3 | ✅ Uygun DDD |
| **Repositories** | 4 | ✅ Soyutlanmış interface'ler |
| **Pipeline Behaviors** | 3 | ✅ Cross-cutting concerns |
| **Boşta Klasörler** | 2 ❌ | **RecordPracticeAnswer, RecordQuizAnswer** |
| **Exception Türleri** | 3 | ✅ Yeterli |

---

## ✅ Güçlü Yönler

1. ✅ **Clean Architecture** - Katmanlar iyi ayrılmış
2. ✅ **CQRS/MediatR** - Standart olarak uygulanmış
3. ✅ **DDD Patterns** - Proper aggregates, value objects, domain events
4. ✅ **SM-2 Algoritması** - Advanced spaced repetition logic
5. ✅ **JWT Authentication** - Secure token implementation
6. ✅ **Error Handling** - Custom exceptions ve middleware
7. ✅ **Dependency Injection** - Proper DI container setup
8. ✅ **API Integration** - Groq AI service with retry policy
9. ✅ **Health Checks** - Monitoring endpoints
10. ✅ **DevController** - DEBUG-only conditional compilation

---

## 🎯 Yapılması Gerekenler (Prioriteli)

### Hemen (P0)
- [ ] `RecordPracticeAnswer` ve `RecordQuizAnswer` boş klasörlerini sil
- [ ] N+1 query sorununu düzelt (GetReviewSessionWords)
- [ ] Bulk generate'de parallelization ekle

### Yakında (P1)
- [ ] Redis cache implementasyonu yap
- [ ] Soft delete pattern ekle
- [ ] Magic numbers'ı constants'a çıkar
- [ ] AI evaluation fallback'i iyileştir

### Sonra (P2)
- [ ] Domain logic tekrarını refactor et
- [ ] Transaction handling'i iyileştir
- [ ] Tüm handler'lara input validation ekle

