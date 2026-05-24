# BeeZillion Veritabanı SQL Script

Bu dosya BeeZillion projesi için SQL Server'da veritabanını kurmak için gerekli tüm DDL ve DML komutlarını içerir.

---

## 1. Ana Tablolar

### Users Tablosu
```sql
CREATE TABLE [Users] (
    [Id] [uniqueidentifier] NOT NULL,
    [Email] [nvarchar](320) NOT NULL,
    [PasswordHash] [nvarchar](500) NOT NULL,
    [Streak] [int] NOT NULL DEFAULT (0),
    [DailyGoal] [int] NOT NULL DEFAULT (8),
    [LastActivity] [datetime2] NOT NULL DEFAULT (GETUTCDATE()),
    [ReviewCount] [int] NOT NULL DEFAULT (0),
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id])
);

CREATE UNIQUE INDEX [IX_Users_Email] ON [Users]([Email]);
```

### Words Tablosu
```sql
CREATE TABLE [Words] (
    [Id] [uniqueidentifier] NOT NULL,
    [OwnerId] [uniqueidentifier] NOT NULL,
    [Original] [nvarchar](200) NOT NULL,
    [Translation] [nvarchar](400) NOT NULL,
    [AiSentence] [nvarchar](1000) NULL,
    [Field] [nvarchar](100) NULL,
    [CreatedAt] [datetime2] NOT NULL DEFAULT (GETUTCDATE()),
    -- Review Value Object Columns
    [ReviewIntervalDays] [int] NOT NULL DEFAULT (1),
    [ReviewEaseFactor] [real] NOT NULL DEFAULT (2.5),
    [ReviewRepetitions] [int] NOT NULL DEFAULT (0),
    [ReviewNextReviewAt] [datetime2] NOT NULL DEFAULT (GETUTCDATE()),
    [ReviewLastReviewedAt] [datetime2] NULL,
    CONSTRAINT [PK_Words] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_Words_Users_OwnerId] FOREIGN KEY ([OwnerId]) 
        REFERENCES [Users]([Id]) ON DELETE CASCADE
);

CREATE NONCLUSTERED INDEX [IX_Words_OwnerId_CreatedAt] ON [Words]([OwnerId], [CreatedAt]);
CREATE NONCLUSTERED INDEX [IX_Words_OwnerId_Field] ON [Words]([OwnerId], [Field]);
```

### PredefinedWords Tablosu
```sql
CREATE TABLE [PredefinedWords] (
    [Id] [uniqueidentifier] NOT NULL,
    [Field] [nvarchar](100) NOT NULL,
    [Category] [nvarchar](100) NULL,
    [Original] [nvarchar](200) NOT NULL,
    [Translation] [nvarchar](400) NOT NULL,
    [AiSentence] [nvarchar](1000) NULL,
    [CreatedAt] [datetime2] NOT NULL DEFAULT (GETUTCDATE()),
    [IsActive] [bit] NOT NULL DEFAULT (1),
    CONSTRAINT [PK_PredefinedWords] PRIMARY KEY CLUSTERED ([Id])
);

CREATE NONCLUSTERED INDEX [IX_PredefinedWords_Original] ON [PredefinedWords]([Original]);
CREATE NONCLUSTERED INDEX [IX_PredefinedWords_Field_Category] ON [PredefinedWords]([Field], [Category]);
CREATE NONCLUSTERED INDEX [IX_PredefinedWords_Field_IsActive] ON [PredefinedWords]([Field], [IsActive]);
```

### UserVocabularyProgresses Tablosu
```sql
CREATE TABLE [UserVocabularyProgresses] (
    [Id] [uniqueidentifier] NOT NULL,
    [UserId] [uniqueidentifier] NOT NULL,
    [WordId] [uniqueidentifier] NOT NULL,
    [TotalAttempts] [int] NOT NULL DEFAULT (0),
    [CorrectAttempts] [int] NOT NULL DEFAULT (0),
    [AverageTimeTakenMs] [bigint] NOT NULL DEFAULT (0),
    [MinTimeTakenMs] [bigint] NOT NULL DEFAULT (0),
    [MaxTimeTakenMs] [bigint] NOT NULL DEFAULT (0),
    [ConsecutiveSelections] [int] NOT NULL DEFAULT (0),
    [LastSelectedAt] [datetime2] NULL,
    [CreatedAt] [datetime2] NOT NULL DEFAULT (GETUTCDATE()),
    [UpdatedAt] [datetime2] NOT NULL DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_UserVocabularyProgresses] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_UserVocabularyProgresses_Users_UserId] FOREIGN KEY ([UserId]) 
        REFERENCES [Users]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserVocabularyProgresses_Words_WordId] FOREIGN KEY ([WordId]) 
        REFERENCES [Words]([Id]) ON DELETE CASCADE,
    CONSTRAINT [UX_UserVocabularyProgresses_UserId_WordId] UNIQUE ([UserId], [WordId])
);

CREATE NONCLUSTERED INDEX [IX_UserVocabularyProgresses_UserId_LastSelectedAt] ON [UserVocabularyProgresses]([UserId], [LastSelectedAt]);
CREATE NONCLUSTERED INDEX [IX_UserVocabularyProgresses_UserId_UpdatedAt] ON [UserVocabularyProgresses]([UserId], [UpdatedAt]);
```

### UserBadges Tablosu (Owned Entity)
```sql
CREATE TABLE [UserBadges] (
    [Id] [uniqueidentifier] NOT NULL,
    [Type] [int] NOT NULL,
    [AwardedAt] [datetime2] NOT NULL DEFAULT (GETUTCDATE()),
    [UserId] [uniqueidentifier] NOT NULL,
    CONSTRAINT [PK_UserBadges] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_UserBadges_Users_UserId] FOREIGN KEY ([UserId]) 
        REFERENCES [Users]([Id]) ON DELETE CASCADE
);

CREATE NONCLUSTERED INDEX [IX_UserBadges_UserId] ON [UserBadges]([UserId]);
```

---

## 2. Seed Data - Predefined Words

### Software Alanı
```sql
INSERT INTO [PredefinedWords] ([Id], [Field], [Category], [Original], [Translation], [AiSentence], [CreatedAt], [IsActive])
VALUES 
    (NEWID(), 'Software', 'General', 'algorithm', 'algoritma', 'An algorithm is a step-by-step procedure for solving a problem.', GETUTCDATE(), 1),
    (NEWID(), 'Software', 'General', 'bug', 'hata', 'A bug is an error or flaw in a software program.', GETUTCDATE(), 1),
    (NEWID(), 'Software', 'General', 'debug', 'hata ayıklamak', 'To debug means to find and fix errors in code.', GETUTCDATE(), 1),
    (NEWID(), 'Software', 'General', 'framework', 'framework', 'A framework provides a foundation for building applications.', GETUTCDATE(), 1),
    (NEWID(), 'Software', 'General', 'repository', 'depo', 'A repository is a central storage location for code.', GETUTCDATE(), 1);
```

### Medicine Alanı
```sql
INSERT INTO [PredefinedWords] ([Id], [Field], [Category], [Original], [Translation], [AiSentence], [CreatedAt], [IsActive])
VALUES 
    (NEWID(), 'Medicine', 'General', 'diagnosis', 'tanı', 'Diagnosis is the identification of a disease or condition.', GETUTCDATE(), 1),
    (NEWID(), 'Medicine', 'General', 'prognosis', 'hastalık gidişi', 'Prognosis is the likely outcome of a disease.', GETUTCDATE(), 1),
    (NEWID(), 'Medicine', 'General', 'symptom', 'semptom', 'A symptom is a sign of illness or disease.', GETUTCDATE(), 1),
    (NEWID(), 'Medicine', 'General', 'treatment', 'tedavi', 'Treatment is the medical care given for an illness.', GETUTCDATE(), 1);
```

### Law Alanı
```sql
INSERT INTO [PredefinedWords] ([Id], [Field], [Category], [Original], [Translation], [AiSentence], [CreatedAt], [IsActive])
VALUES 
    (NEWID(), 'Law', 'General', 'defendant', 'davalı', 'A defendant is a person accused of a crime.', GETUTCDATE(), 1),
    (NEWID(), 'Law', 'General', 'verdict', 'karar', 'A verdict is the decision made by a court.', GETUTCDATE(), 1),
    (NEWID(), 'Law', 'General', 'lawsuit', 'dava', 'A lawsuit is a legal action brought in court.', GETUTCDATE(), 1),
    (NEWID(), 'Law', 'General', 'attorney', 'avukat', 'An attorney is a lawyer who represents clients.', GETUTCDATE(), 1);
```

---

## 3. Veritabanı Oluşturma Script'i (Tek Komut)

Aşağıdaki script'i SQL Server Management Studio'da çalıştırarak tüm tabloları birden oluşturabilirsiniz:

```sql
-- Veritabanı seçimi
USE [BeeZillionDb];

-- Users Tablosu
CREATE TABLE [dbo].[Users] (
    [Id] [uniqueidentifier] NOT NULL,
    [Email] [nvarchar](320) NOT NULL,
    [PasswordHash] [nvarchar](500) NOT NULL,
    [Streak] [int] NOT NULL DEFAULT (0),
    [DailyGoal] [int] NOT NULL DEFAULT (8),
    [LastActivity] [datetime2] NOT NULL DEFAULT (GETUTCDATE()),
    [ReviewCount] [int] NOT NULL DEFAULT (0),
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id])
);

CREATE UNIQUE INDEX [IX_Users_Email] ON [dbo].[Users]([Email]);

-- Words Tablosu
CREATE TABLE [dbo].[Words] (
    [Id] [uniqueidentifier] NOT NULL,
    [OwnerId] [uniqueidentifier] NOT NULL,
    [Original] [nvarchar](200) NOT NULL,
    [Translation] [nvarchar](400) NOT NULL,
    [AiSentence] [nvarchar](1000) NULL,
    [Field] [nvarchar](100) NULL,
    [CreatedAt] [datetime2] NOT NULL DEFAULT (GETUTCDATE()),
    [ReviewIntervalDays] [int] NOT NULL DEFAULT (1),
    [ReviewEaseFactor] [real] NOT NULL DEFAULT (2.5),
    [ReviewRepetitions] [int] NOT NULL DEFAULT (0),
    [ReviewNextReviewAt] [datetime2] NOT NULL DEFAULT (GETUTCDATE()),
    [ReviewLastReviewedAt] [datetime2] NULL,
    CONSTRAINT [PK_Words] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_Words_Users_OwnerId] FOREIGN KEY ([OwnerId]) 
        REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE
);

CREATE NONCLUSTERED INDEX [IX_Words_OwnerId_CreatedAt] ON [dbo].[Words]([OwnerId], [CreatedAt]);
CREATE NONCLUSTERED INDEX [IX_Words_OwnerId_Field] ON [dbo].[Words]([OwnerId], [Field]);

-- PredefinedWords Tablosu
CREATE TABLE [dbo].[PredefinedWords] (
    [Id] [uniqueidentifier] NOT NULL,
    [Field] [nvarchar](100) NOT NULL,
    [Category] [nvarchar](100) NULL,
    [Original] [nvarchar](200) NOT NULL,
    [Translation] [nvarchar](400) NOT NULL,
    [AiSentence] [nvarchar](1000) NULL,
    [CreatedAt] [datetime2] NOT NULL DEFAULT (GETUTCDATE()),
    [IsActive] [bit] NOT NULL DEFAULT (1),
    CONSTRAINT [PK_PredefinedWords] PRIMARY KEY CLUSTERED ([Id])
);

CREATE NONCLUSTERED INDEX [IX_PredefinedWords_Original] ON [dbo].[PredefinedWords]([Original]);
CREATE NONCLUSTERED INDEX [IX_PredefinedWords_Field_Category] ON [dbo].[PredefinedWords]([Field], [Category]);
CREATE NONCLUSTERED INDEX [IX_PredefinedWords_Field_IsActive] ON [dbo].[PredefinedWords]([Field], [IsActive]);

-- UserVocabularyProgresses Tablosu
CREATE TABLE [dbo].[UserVocabularyProgresses] (
    [Id] [uniqueidentifier] NOT NULL,
    [UserId] [uniqueidentifier] NOT NULL,
    [WordId] [uniqueidentifier] NOT NULL,
    [TotalAttempts] [int] NOT NULL DEFAULT (0),
    [CorrectAttempts] [int] NOT NULL DEFAULT (0),
    [AverageTimeTakenMs] [bigint] NOT NULL DEFAULT (0),
    [MinTimeTakenMs] [bigint] NOT NULL DEFAULT (0),
    [MaxTimeTakenMs] [bigint] NOT NULL DEFAULT (0),
    [ConsecutiveSelections] [int] NOT NULL DEFAULT (0),
    [LastSelectedAt] [datetime2] NULL,
    [CreatedAt] [datetime2] NOT NULL DEFAULT (GETUTCDATE()),
    [UpdatedAt] [datetime2] NOT NULL DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_UserVocabularyProgresses] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_UserVocabularyProgresses_Users_UserId] FOREIGN KEY ([UserId]) 
        REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserVocabularyProgresses_Words_WordId] FOREIGN KEY ([WordId]) 
        REFERENCES [dbo].[Words]([Id]) ON DELETE CASCADE,
    CONSTRAINT [UX_UserVocabularyProgresses_UserId_WordId] UNIQUE ([UserId], [WordId])
);

CREATE NONCLUSTERED INDEX [IX_UserVocabularyProgresses_UserId_LastSelectedAt] 
    ON [dbo].[UserVocabularyProgresses]([UserId], [LastSelectedAt]);
CREATE NONCLUSTERED INDEX [IX_UserVocabularyProgresses_UserId_UpdatedAt] 
    ON [dbo].[UserVocabularyProgresses]([UserId], [UpdatedAt]);

-- UserBadges Tablosu
CREATE TABLE [dbo].[UserBadges] (
    [Id] [uniqueidentifier] NOT NULL,
    [Type] [int] NOT NULL,
    [AwardedAt] [datetime2] NOT NULL DEFAULT (GETUTCDATE()),
    [UserId] [uniqueidentifier] NOT NULL,
    CONSTRAINT [PK_UserBadges] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_UserBadges_Users_UserId] FOREIGN KEY ([UserId]) 
        REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE
);

CREATE NONCLUSTERED INDEX [IX_UserBadges_UserId] ON [dbo].[UserBadges]([UserId]);

-- Predefined Words Seed Data
INSERT INTO [dbo].[PredefinedWords] ([Id], [Field], [Category], [Original], [Translation], [AiSentence], [CreatedAt], [IsActive])
VALUES 
    -- Software
    (NEWID(), 'Software', 'General', 'algorithm', 'algoritma', 'An algorithm is a step-by-step procedure for solving a problem.', GETUTCDATE(), 1),
    (NEWID(), 'Software', 'General', 'bug', 'hata', 'A bug is an error or flaw in a software program.', GETUTCDATE(), 1),
    (NEWID(), 'Software', 'General', 'debug', 'hata ayıklamak', 'To debug means to find and fix errors in code.', GETUTCDATE(), 1),
    (NEWID(), 'Software', 'General', 'framework', 'framework', 'A framework provides a foundation for building applications.', GETUTCDATE(), 1),
    (NEWID(), 'Software', 'General', 'repository', 'depo', 'A repository is a central storage location for code.', GETUTCDATE(), 1),
    -- Medicine
    (NEWID(), 'Medicine', 'General', 'diagnosis', 'tanı', 'Diagnosis is the identification of a disease or condition.', GETUTCDATE(), 1),
    (NEWID(), 'Medicine', 'General', 'prognosis', 'hastalık gidişi', 'Prognosis is the likely outcome of a disease.', GETUTCDATE(), 1),
    (NEWID(), 'Medicine', 'General', 'symptom', 'semptom', 'A symptom is a sign of illness or disease.', GETUTCDATE(), 1),
    (NEWID(), 'Medicine', 'General', 'treatment', 'tedavi', 'Treatment is the medical care given for an illness.', GETUTCDATE(), 1),
    -- Law
    (NEWID(), 'Law', 'General', 'defendant', 'davalı', 'A defendant is a person accused of a crime.', GETUTCDATE(), 1),
    (NEWID(), 'Law', 'General', 'verdict', 'karar', 'A verdict is the decision made by a court.', GETUTCDATE(), 1),
    (NEWID(), 'Law', 'General', 'lawsuit', 'dava', 'A lawsuit is a legal action brought in court.', GETUTCDATE(), 1),
    (NEWID(), 'Law', 'General', 'attorney', 'avukat', 'An attorney is a lawyer who represents clients.', GETUTCDATE(), 1);

-- Tüm tablolar başarıyla oluşturuldu
PRINT 'Veritabanı başarıyla oluşturuldu!';
```

---

## 4. Veritabanını Silme Script'i (Eğer baştan başlamak isterseniz)

```sql
-- Foreign Key'leri Sil
ALTER TABLE [dbo].[UserBadges] DROP CONSTRAINT [FK_UserBadges_Users_UserId];
ALTER TABLE [dbo].[UserVocabularyProgresses] DROP CONSTRAINT [FK_UserVocabularyProgresses_Users_UserId];
ALTER TABLE [dbo].[UserVocabularyProgresses] DROP CONSTRAINT [FK_UserVocabularyProgresses_Words_WordId];
ALTER TABLE [dbo].[Words] DROP CONSTRAINT [FK_Words_Users_OwnerId];

-- Tabloları Sil
DROP TABLE IF EXISTS [dbo].[UserBadges];
DROP TABLE IF EXISTS [dbo].[UserVocabularyProgresses];
DROP TABLE IF EXISTS [dbo].[PredefinedWords];
DROP TABLE IF EXISTS [dbo].[Words];
DROP TABLE IF EXISTS [dbo].[Users];

PRINT 'Tüm tablolar silindi!';
```

---

## 5. Tablo Açıklaması

| Tablo | Açıklama |
|-------|----------|
| **Users** | Kullanıcı hesaplarını depolayan ana tablo |
| **Words** | Kullanıcıların eklediği kelimeler ve review bilgileri |
| **PredefinedWords** | Sistem tarafından önceden tanımlanmış kelimeler |
| **UserVocabularyProgresses** | Kullanıcıların kelime üzerindeki ilerleme verisi |
| **UserBadges** | Kullanıcılar tarafından kazanılan rozetler |

---

## 6. Önemli Kolonlar

### Review Value Object (Words tablosunda)
- `ReviewIntervalDays`: SM-2 algoritmasına göre bir sonraki tekrar günü
- `ReviewEaseFactor`: Kelime zorluk faktörü (varsayılan: 2.5)
- `ReviewRepetitions`: Kaç kez tekrarlandığı
- `ReviewNextReviewAt`: Bir sonraki tekrar tarihi
- `ReviewLastReviewedAt`: Son tekrar tarihi

---

## 7. Bağlantı String'i (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=BeeZillionDb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=true;"
  }
}
```

---

## Notlar
- Tüm Guid'ler otomatik olarak oluşturulacaktır
- DateTime'lar UTC formatında tutulacaktır
- Foreign Key'ler CASCADE DELETE ile konfigüre edilmiştir
- Indexler sorgu performansını optimize etmek için oluşturulmuştur

