# Predefined Words Feature - Ön Analizi
**Tarih:** May 13, 2026  
**Durum:** Planning  
**DDD/CQRS/MediatR Uyumlu:** ✅

---

## 📋 Gereklilikler

1. Admin'in DB'de alan-özel ingilizce kelime seti hazırlaması (Software, Medicine, Law, vb.)
2. User "Alan Seçiniz" seçeneğinden hazır kelimeleri seçebilmesi
3. Seçilen alan'ın kelimeleri user'ın collection'ına toplu olarak eklenmesi
4. User kendi kelimeleri de ekleyebilmesine devam etmesi
5. Mimariye zarar vermemesi

---

## 🏗️ Domain Model Changes

### Yeni Entity: `PredefinedWord`

```csharp
// Domain/Aggregates/PredefinedWord/PredefinedWord.cs
public sealed class PredefinedWord
{
    public PredefinedWordId Id { get; private set; }
    public string Field { get; private set; }        // "Software", "Medicine", "Law" vb.
    public string Category { get; private set; }     // İsteğe bağlı: "Networking", "Database" vb.
    public string Original { get; private set; }     // English word
    public string Translation { get; private set; }  // Turkish translation
    public string? AiSentence { get; private set; }  // Example sentence
    public DateTime CreatedAt { get; private set; }
    public bool IsActive { get; private set; }
    
    // Constructor & Factory methods
}
```

### Değişiklikler: `Word` Aggregate
- ✅ **Değişim YOK** - Word'de Field/Category eklemeyeceğiz
- User words ile predefined words'ü ayrı tutuyoruz (Separation of Concerns)

### Yeni ValueObject: `FieldId`
```csharp
public sealed record FieldId(Guid Value);
public sealed record PredefinedWordId(Guid Value);
```

---

## 📊 Database Schema

### Mevcut: `Words` Table
```sql
-- Unchanged
id (Guid) PRIMARY KEY
owner_id (Guid) FOREIGN KEY -> Users
original (string)
translation (string)
ai_sentence (string?)
created_at (DateTime)
review_* (ReviewInfo columns)
```

### Yeni: `PredefinedWords` Table
```sql
CREATE TABLE predefined_words (
    id UUID PRIMARY KEY,
    field VARCHAR(100) NOT NULL,           -- "Software", "Medicine"
    category VARCHAR(100),                 -- Optional: "Networking", "Pharmacology"
    original VARCHAR(200) NOT NULL,        -- English word
    translation VARCHAR(400) NOT NULL,     -- Turkish translation
    ai_sentence VARCHAR(1000),             -- Example sentence
    created_at TIMESTAMP NOT NULL,
    is_active BOOLEAN DEFAULT true,
    
    UNIQUE(field, category, original)      -- Aynı alan-kategori'de duplicate word yok
);

CREATE INDEX idx_predefined_words_field ON predefined_words(field, is_active);
```

### Yeni: `UserPredefinedFields` Table (Tracking)
```sql
CREATE TABLE user_predefined_fields (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL,
    field VARCHAR(100) NOT NULL,
    imported_at TIMESTAMP NOT NULL,
    
    FOREIGN KEY (user_id) REFERENCES users(id),
    UNIQUE(user_id, field)  -- User her alanı bir kez import edebilir
);
```

---

## 🔄 Architecture Layers

### 1️⃣ **Domain Layer** 
```
Domain/
├── Aggregates/
│   └── PredefinedWord/
│       ├── PredefinedWord.cs       (New Entity Root)
│       ├── PredefinedWordId.cs     (New ValueObject)
│       └── Field.cs                (ValueObject - field adı)
├── ValueObjects/
│   ├── FieldId.cs                 (New)
│   └── ... (mevcut)
├── Repositories/
│   └── IPredefinedWordRepository.cs (New Interface)
└── Entities/
    └── UserPredefinedFieldImport.cs (Optional - tracking)
```

### 2️⃣ **Application Layer**
```
Application/
├── PredefinedWords/
│   ├── Queries/
│   │   ├── GetFieldsList/
│   │   │   ├── GetFieldsListQuery.cs
│   │   │   └── GetFieldsListQueryHandler.cs
│   │   └── GetPredefinedWordsByField/
│   │       ├── GetPredefinedWordsByFieldQuery.cs
│   │       └── GetPredefinedWordsByFieldQueryHandler.cs
│   └── Commands/
│       └── ImportPredefinedWordsByField/
│           ├── ImportPredefinedWordsByFieldCommand.cs
│           ├── ImportPredefinedWordsByFieldCommandHandler.cs
│           └── ImportPredefinedWordsByFieldCommandValidator.cs
└── Words/
    └── Commands/CreateWord/  (Unchanged)
```

### 3️⃣ **Infrastructure Layer**
```
Infrastructure/
├── Persistence/
│   ├── Configurations/
│   │   ├── PredefinedWordConfiguration.cs (New)
│   │   └── WordConfiguration.cs (Unchanged)
│   └── Repositories/
│       ├── PredefinedWordRepository.cs (New)
│       └── WordRepository.cs (Unchanged)
└── Migrations/
    └── AddPredefinedWordsTable.cs (New)
```

### 4️⃣ **API Layer**
```
API/
└── Controllers/
    ├── PredefinedWordsController.cs (New) - GetFields, GetWordsByField
    └── WordsController.cs (Enhanced) - ImportPredefinedWords endpoint
```

---

## 🔗 User Flow

### Step 1: Frontend - Alan Seçimi
```
Dashboard/Words
├─ Dropdown: "Alan Seçiniz" (Select Field)
│  ├─ Software
│  ├─ Medicine
│  ├─ Law
│  └─ ...
└─ Button: "Alan Kelimelerini Ekle" (Import Field Words)
```

### Step 2: Backend - Data Flow
```
ImportPredefinedWordsByFieldCommand
    ↓ (MediatR ISender)
ImportPredefinedWordsByFieldCommandHandler
    ├─ 1. GetFieldName → Validate
    ├─ 2. GetPredefinedWordsByField → PredefinedWordRepository
    ├─ 3. Bulk Create Words (OwnerId = CurrentUserId)
    ├─ 4. Add each to repository
    ├─ 5. Track in UserPredefinedFieldImport
    ├─ 6. UnitOfWork.SaveChanges
    └─ ✅ Return: ImportResult { FieldName, ImportedCount }
```

### Step 3: Frontend - Feedback
```
Toast: "Software kelimeleri başarıyla eklendi (45 kelime)"
```

---

## 📝 Implementation Tasks

### Phase 1: Domain & Database
- [ ] Create `PredefinedWord` aggregate + ValueObjects
- [ ] Create `IPredefinedWordRepository` interface
- [ ] Add `PredefinedWordConfiguration` (EF mapping)
- [ ] Create migration: `AddPredefinedWordsTable`
- [ ] Seed predefined words (Admin migration script)

### Phase 2: Application Layer
- [ ] `GetFieldsListQueryHandler` - Available fields
- [ ] `GetPredefinedWordsByFieldQueryHandler` - Preview words
- [ ] `ImportPredefinedWordsByFieldCommandHandler` - Bulk import logic
- [ ] Validation & error handling

### Phase 3: Infrastructure & Persistence
- [ ] `PredefinedWordRepository` implementation
- [ ] `AppDbContext` updates (new DbSet)
- [ ] Seed data scripts

### Phase 4: API & Frontend
- [ ] `PredefinedWordsController` endpoints
- [ ] `WordsController.ImportPredefinedWords` endpoint
- [ ] Frontend: Field selection UI
- [ ] Frontend: Import button & toast notifications

---

## 🎯 Key Design Decisions

| Karar | Gerekçe |
|------|---------|
| **Ayrı PredefinedWord Entity** | User's Words'ten izole edilmiş, admin yönetilen, DDD boundary'si net |
| **Bulk Import Command** | CQRS uyumlu, single action = single command, atomik transaction |
| **UserPredefinedFieldImport tracking** | User'ın hangi alanı import ettiğini bilmek, duplicate prevention |
| **No Field property in Word** | Word aggregate'ı basit kalır, polymorph değildir, domain model clean |
| **Separate Repository** | Repository pattern consistency, IPredefinedWordRepository distinct |
| **Is_Active flag** | Soft delete, yönetim kolaylığı |

---

## ✅ Mimariye Uygunluk Kontrol Listesi

- ✅ **DDD**: Aggregate Root (`PredefinedWord`), Bounded Context (Admin Context)
- ✅ **CQRS**: 
  - Queries: `GetFieldsList`, `GetPredefinedWordsByField` (read)
  - Command: `ImportPredefinedWordsByField` (write)
- ✅ **MediatR**: ISender pattern, IRequestHandler implementations
- ✅ **Layer Separation**: Domain → Application → Infrastructure → API
- ✅ **No Breaking Changes**: Word aggregate, existing commands untouched
- ✅ **Dependency Injection**: Repository → Handler → API
- ✅ **Validation**: FluentValidation on command + domain logic
- ✅ **EF Core**: IEntityTypeConfiguration pattern

---

## 🚀 Example Request/Response

### Request: Import Software Words
```json
POST /api/predefined-words/import
{
  "field": "Software",
  "importOption": "all"  // or "preview" (get count first)
}
```

### Response
```json
{
  "success": true,
  "fieldName": "Software",
  "importedCount": 52,
  "message": "52 kelime başarıyla eklendi",
  "timestamp": "2026-05-13T14:30:00Z"
}
```

### Alternative: Get Preview First
```json
GET /api/predefined-words/fields/Software/count
Response: { "fieldName": "Software", "totalWords": 52 }
```

---

## 📊 Veritabanı Büyüme Tahmini

| Tablo | Satır Sayısı (örn.) | Alan Sayısı | Toplam |
|------|----------|----------|--------|
| predefined_words | 50/alan × 10 alan | 500 | 500 rows |
| user_predefined_fields | 500 kullanıcı × 3 alan | 1500 | 1.5K rows |
| **Total** | | | **2K rows** |

💡 **Negligible** - Performans sorunu yok

---

## ⚠️ Olası İstisnalar & Handling

| Senaryo | Çözüm |
|---------|------|
| User aynı alanı 2× import ederse | Validation error: "Bu alanı zaten import ettiniz" |
| Seçilen alan boşsa (kelime yok) | Warning: "Bu alandan kelime bulunamadı" |
| Network hatası sırasında bulk import | Transaction rollback, user'ı notify et |
| Admin kelime silerse (soft delete) | Eski imported words kalır, yenileri alınmaz |
| User delete word'ü importe edildikten sonra | Word stays, user'ın kendisi, normal flow |

---

## 🔐 Security & Authorization

- ✅ `[Authorize]` attribute - authenticated users only
- ✅ `CurrentUserService.GetUserId()` - user isolation
- ✅ Admin only (optional): Field management - authorization policy
- ✅ Input validation: field name must be from allowed list
- ✅ SQL injection: Parameterized queries (EF Core handles)

---

## 📱 Frontend Integration Points

1. **Dashboard/Words Page**
   - Dropdown: List fields (`/api/predefined-words/fields`)
   - Button: Import field (`POST /api/predefined-words/import`)

2. **Word List**
   - Badge: "📌 Imported from Software" (if applicable)

3. **Settings/Profile** (optional)
   - Show imported fields history
   - Option to remove field import

---

## ✨ Summary

**Proposed Solution:**
- ✅ **Minimal Breaking Changes**: Zero changes to existing Word/User aggregates
- ✅ **Clean Architecture**: New bounded context (PredefinedWords), repository pattern
- ✅ **DDD Compliant**: Separate aggregate, value objects, domain logic
- ✅ **CQRS Pattern**: Queries (read) + Commands (write)
- ✅ **Scalable**: Easy to add new fields, bulk operations efficient
- ✅ **Maintainable**: Separation of concerns, testable handlers

---

## 🎬 Approval Checklist

- [ ] Mimariye uygun mu?
- [ ] Database schema uygun mu?
- [ ] User flow açık mı?
- [ ] İmplementasyona geçelim?

**Devam etmek istiyorsanız "Başla" deyin! 🚀**
