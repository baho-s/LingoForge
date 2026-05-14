-- Migration: AddPredefinedWordsTable
-- Description: Add PredefinedWords table for field-specific vocabulary
-- Date: 2026-05-13

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PredefinedWords')
BEGIN
    CREATE TABLE PredefinedWords (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        Field NVARCHAR(100) NOT NULL,
        Category NVARCHAR(100) NULL,
        Original NVARCHAR(200) NOT NULL,
        Translation NVARCHAR(400) NOT NULL,
        AiSentence NVARCHAR(1000) NULL,
        CreatedAt DATETIME2 NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        
        CONSTRAINT UQ_PredefinedWords_FieldCategoryOriginal UNIQUE (Field, Category, Original)
    );

    CREATE NONCLUSTERED INDEX IX_PredefinedWords_FieldIsActive 
    ON PredefinedWords(Field, IsActive);
    
    CREATE NONCLUSTERED INDEX IX_PredefinedWords_FieldCategory 
    ON PredefinedWords(Field, Category);
    
    CREATE NONCLUSTERED INDEX IX_PredefinedWords_Original 
    ON PredefinedWords(Original);

    -- Insert seed data
    INSERT INTO PredefinedWords (Id, Field, Category, Original, Translation, AiSentence, CreatedAt, IsActive)
    VALUES
    -- Software field
    (NEWID(), 'Software', 'General', 'algorithm', 'algoritma', 'An algorithm is a step-by-step procedure for solving a problem.', GETUTCDATE(), 1),
    (NEWID(), 'Software', 'General', 'bug', 'hata', 'A bug is an error or flaw in a software program.', GETUTCDATE(), 1),
    (NEWID(), 'Software', 'General', 'debug', 'hata ayıklamak', 'To debug means to find and fix errors in code.', GETUTCDATE(), 1),
    (NEWID(), 'Software', 'General', 'framework', 'framework', 'A framework provides a foundation for building applications.', GETUTCDATE(), 1),
    (NEWID(), 'Software', 'General', 'repository', 'depo', 'A repository is a central storage location for code.', GETUTCDATE(), 1),
    (NEWID(), 'Software', 'General', 'deployment', 'dağıtım', 'Deployment is the process of releasing software to production.', GETUTCDATE(), 1),
    (NEWID(), 'Software', 'General', 'API', 'API', 'An API is a set of rules for software communication.', GETUTCDATE(), 1),
    (NEWID(), 'Software', 'General', 'cache', 'önbellek', 'Cache is temporary storage for frequently accessed data.', GETUTCDATE(), 1),
    (NEWID(), 'Software', 'General', 'authentication', 'kimlik doğrulama', 'Authentication is the process of verifying user identity.', GETUTCDATE(), 1),
    (NEWID(), 'Software', 'General', 'database', 'veritabanı', 'A database is an organized collection of data.', GETUTCDATE(), 1),
    
    -- Medicine field
    (NEWID(), 'Medicine', 'General', 'diagnosis', 'tanı', 'Diagnosis is the identification of a disease or condition.', GETUTCDATE(), 1),
    (NEWID(), 'Medicine', 'General', 'prognosis', 'hastalık gidişi', 'Prognosis is the likely outcome of a disease.', GETUTCDATE(), 1),
    (NEWID(), 'Medicine', 'General', 'symptom', 'semptom', 'A symptom is a sign of illness or disease.', GETUTCDATE(), 1),
    (NEWID(), 'Medicine', 'General', 'treatment', 'tedavi', 'Treatment is the medical care given for an illness.', GETUTCDATE(), 1),
    (NEWID(), 'Medicine', 'General', 'patient', 'hasta', 'A patient is a person receiving medical care.', GETUTCDATE(), 1),
    (NEWID(), 'Medicine', 'General', 'medication', 'ilaç', 'Medication is a substance used to treat disease.', GETUTCDATE(), 1),
    (NEWID(), 'Medicine', 'General', 'therapy', 'terapi', 'Therapy is treatment designed to cure or relieve symptoms.', GETUTCDATE(), 1),
    (NEWID(), 'Medicine', 'General', 'physician', 'doktor', 'A physician is a medical doctor.', GETUTCDATE(), 1),
    
    -- Law field
    (NEWID(), 'Law', 'General', 'defendant', 'davalı', 'A defendant is a person accused of a crime.', GETUTCDATE(), 1),
    (NEWID(), 'Law', 'General', 'verdict', 'karar', 'A verdict is the decision made by a court.', GETUTCDATE(), 1),
    (NEWID(), 'Law', 'General', 'lawsuit', 'dava', 'A lawsuit is a legal action brought in court.', GETUTCDATE(), 1),
    (NEWID(), 'Law', 'General', 'attorney', 'avukat', 'An attorney is a lawyer who represents clients.', GETUTCDATE(), 1),
    (NEWID(), 'Law', 'General', 'testimony', 'tanıklık', 'Testimony is a formal statement under oath.', GETUTCDATE(), 1),
    (NEWID(), 'Law', 'General', 'evidence', 'kanıt', 'Evidence is information used to prove a case.', GETUTCDATE(), 1),
    (NEWID(), 'Law', 'General', 'contract', 'sözleşme', 'A contract is a legally binding agreement.', GETUTCDATE(), 1),
    (NEWID(), 'Law', 'General', 'agreement', 'anlaşma', 'An agreement is a mutual understanding between parties.', GETUTCDATE(), 1);

    PRINT 'PredefinedWords table created successfully with seed data.'
END
ELSE
BEGIN
    PRINT 'PredefinedWords table already exists.'
END
