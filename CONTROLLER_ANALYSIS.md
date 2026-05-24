# Controller Mimarisi Analizi - CQRS ve DDD Uygunluğu

## 📊 Özet
- **UYGUN**: AuthController ✅, WordsController ✅, QuizController ✅
- **UYGUN OLMAYAN**: PracticeController ❌

---

## 🚨 PracticeController - UYGUN OLMAYAN

### ❌ Sorunlar

#### 1. **MediatR Kullanmıyor - CQRS İhlali**
```csharp
// ❌ İLGİLİ OLMAYAN YAPILAR
public PracticeController(
    IWordRepository wordRepository,           // Repository direkt bağımlılığı
    IUserRepository userRepository,           // Repository direkt bağımlılığı
    ICurrentUserService currentUser,
    IAiSentenceService aiSentenceService,
    IUnitOfWork unitOfWork)                   // UnitOfWork direkt bağımlılığı
```

**Sorun**: Diğer controllerler gibi `ISender` (MediatR) kullanmıyor, bunun yerine:
- Repository'lere direkt erişim
- Service'lere direkt erişim  
- UnitOfWork direkt yönetimi

#### 2. **Business Logic Controller'da - DDD İhlali**
```csharp
// ❌ Endpoint: GetQuestions
public async Task<ActionResult<PracticeQuestionsResponse>> GetQuestions(...)
{
    var words = await _wordRepository.GetByOwnerAsync(userId, cancellationToken);
    var questions = BuildQuestions(words, modes, limit);  // ← BUSINESS LOGIC
    return Ok(new PracticeQuestionsResponse(questions));
}

// ❌ 150+ satırlık BuildQuestions() metodu
private static List<PracticeQuestionDto> BuildQuestions(
    IReadOnlyList<Word> words,
    List<string> modes,
    int limit)
{
    // Soru oluşturma lojiği
    // Question seçme algoritması
    // Mode belirleme
    // Option building vb...
}
```

**Sorun**: 
- Question generation logic Application layer'da olmalı
- Controller'da validation ve option building yapılıyor

#### 3. **Repository Direkt Kullanımı**
```csharp
// ❌ Submit Answer'da User ve Word repository'sine direkt erişim
var word = await _wordRepository.GetByIdAsync(new WordId(questionId), cancellationToken);
var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

// ❌ Entity'de update işlemi
user.RecordReview(DateTime.UtcNow);
_userRepository.Update(user);
await _unitOfWork.SaveChangesAsync(cancellationToken);
```

**Sorun**: 
- Repository erişimi Command handler'da olmalı
- Entity state changes Command'de manage edilmeli

#### 4. **AI Service Direkt Çağrısı**
```csharp
// ❌ Controller'da AI service servis çağrısı
var sentence = await _aiSentenceService.GenerateSentenceAsync(
    prompt, cancellationToken);
```

**Sorun**: 
- External service çağrısı Application layer'da olmalı

---

## ✅ Uygun Controllers - Best Practice

### AuthController
```csharp
// ✅ Düzgün CQRS yapısı
public AuthController(ISender sender)  // Sadece ISender
{
    _sender = sender;
}

[HttpPost("login")]
public async Task<ActionResult<AuthResponse>> Login(
    LoginCommand command,              // Command modeli
    CancellationToken cancellationToken)
{
    var result = await _sender.Send(command, cancellationToken);
    return Ok(result);
}
```

**Neden iyi**: Command pattern, business logic delegating, clean separation

### WordsController
```csharp
// ✅ CQRS pattern ile Commands ve Queries
[HttpGet]
public async Task<ActionResult<IReadOnlyList<WordDto>>> GetList(
    CancellationToken cancellationToken)
{
    var result = await _sender.Send(new GetWordListQuery(), cancellationToken);
    return Ok(result);
}

[HttpPost]
public async Task<ActionResult<WordDto>> Create(
    CreateWordCommand command,
    CancellationToken cancellationToken)
{
    var result = await _sender.Send(command, cancellationToken);
    return Ok(result);
}
```

**Neden iyi**:
- Commands (Create, RecordReview)
- Queries (GetWordList, GetWordOfDay)
- Business logic Application/Handlers'da
- Repository abstracted away

### QuizController
```csharp
// ✅ Query pattern ile read işlemi
[HttpGet]
public async Task<ActionResult<IReadOnlyList<QuizWordDto>>> Get(
    [FromQuery] QuizMode mode = QuizMode.FillBlank,
    [FromQuery] int count = 10,
    CancellationToken cancellationToken = default)
{
    var result = await _sender.Send(
        new GetQuizWordsQuery(mode, count), 
        cancellationToken);
    return Ok(result);
}
```

**Neden iyi**: Query pattern, clean responsibility separation

---

## 🔧 PracticeController'ı Düzeltme Planı

### Adım 1: CQRS Command/Query Oluştur
```csharp
// Gerekli Commands:
- GetPracticeQuestionsQuery
- GenerateSentenceCommand  
- SubmitPracticeAnswerCommand

// Her biri:
- Business logic'i kapsayacak
- Repository'leri Application handler'da kullanacak
- AI service çağrısı Application'da olacak
```

### Adım 2: Handler'lar Oluştur
```csharp
namespace BeeZillion.Application.Practice.Queries;

public sealed class GetPracticeQuestionsQueryHandler 
    : IRequestHandler<GetPracticeQuestionsQuery, PracticeQuestionsResponse>
{
    private readonly IWordRepository _wordRepository;
    // BuildQuestions() lojiği burada olacak
    
    public async Task<PracticeQuestionsResponse> Handle(
        GetPracticeQuestionsQuery request,
        CancellationToken cancellationToken)
    {
        // Business logic
    }
}
```

### Adım 3: Controller'ı Sadeleştir
```csharp
public sealed class PracticeController : ControllerBase
{
    private readonly ISender _sender;

    public PracticeController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("questions")]
    public async Task<ActionResult<PracticeQuestionsResponse>> GetQuestions(
        [FromQuery] string? mode = null,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new GetPracticeQuestionsQuery(mode, limit), 
            cancellationToken);
        return Ok(result);
    }

    [HttpPost("generate-sentence")]
    public async Task<ActionResult<GeneratedSentenceResponse>> GenerateSentence(
        [FromBody] GenerateSentenceRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new GenerateSentenceCommand(request.TargetVocab), 
            cancellationToken);
        return Ok(result);
    }

    [HttpPost("submit-answer")]
    public async Task<ActionResult<PracticeAnswerResponse>> SubmitAnswer(
        [FromBody] PracticeAnswerRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new SubmitPracticeAnswerCommand(
                request.QuestionId,
                request.UserAnswer,
                request.Type),
            cancellationToken);
        return Ok(result);
    }
}
```

---

## 📋 Arşitektürel Kıyaslama Tablosu

| Kriter | PracticeController | AuthController | WordsController | QuizController |
|--------|------------------|-----------------|-----------------|----------------|
| **MediatR Kullanım** | ❌ Yok | ✅ Evet | ✅ Evet | ✅ Evet |
| **ISender Bağımlılığı** | ❌ Hayır | ✅ Evet | ✅ Evet | ✅ Evet |
| **Repository Direkt Erişim** | ❌ Evet | ✅ Hayır | ✅ Hayır | ✅ Hayır |
| **Business Logic Delegating** | ❌ Hayır | ✅ Evet | ✅ Evet | ✅ Evet |
| **CQRS Pattern** | ❌ Hayır | ✅ Evet | ✅ Evet | ✅ Evet |
| **DDD Uyumluluk** | ❌ Zayıf | ✅ İyi | ✅ İyi | ✅ İyi |
| **Lines of Logic** | ❌ 250+ | ✅ ~35 | ✅ ~60 | ✅ ~20 |

---

## 🎯 Sonuç

**PracticeController** modern CQRS/DDD mimarisinin aksine design edilmiştir. Diğer controller'lar best practice'e uyarken, bu controller:

1. ❌ MediatR pattern'ı görmezden gelir
2. ❌ Business logic'i presentation layer'da tutar
3. ❌ Infrastructure'a sıkı bağımlılık yaratır
4. ❌ DDD separation of concerns'i ihlal eder

**Refactor edilmesi şiddetle önerilir.**

