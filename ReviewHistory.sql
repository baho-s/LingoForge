IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511093201_InitialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] uniqueidentifier NOT NULL,
        [Email] nvarchar(320) NOT NULL,
        [PasswordHash] nvarchar(500) NOT NULL,
        [Streak] int NOT NULL,
        [DailyGoal] int NOT NULL,
        [LastActivity] datetime2 NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511093201_InitialCreate'
)
BEGIN
    CREATE TABLE [Words] (
        [Id] uniqueidentifier NOT NULL,
        [OwnerId] uniqueidentifier NOT NULL,
        [Original] nvarchar(200) NOT NULL,
        [Translation] nvarchar(400) NOT NULL,
        [AiSentence] nvarchar(1000) NULL,
        [ReviewIntervalDays] int NOT NULL,
        [ReviewEaseFactor] real NOT NULL,
        [ReviewRepetitions] int NOT NULL,
        [ReviewNextReviewAt] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Words] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511093201_InitialCreate'
)
BEGIN
    CREATE TABLE [UserBadges] (
        [Id] uniqueidentifier NOT NULL,
        [Type] int NOT NULL,
        [AwardedAt] datetime2 NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_UserBadges] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserBadges_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511093201_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserBadges_UserId] ON [UserBadges] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511093201_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511093201_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Words_OwnerId_CreatedAt] ON [Words] ([OwnerId], [CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511093201_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260511093201_InitialCreate', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511094945_AddReviewCount'
)
BEGIN
    ALTER TABLE [Users] ADD [ReviewCount] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511094945_AddReviewCount'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260511094945_AddReviewCount', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513095009_AddLastReviewedAtToReview'
)
BEGIN
    ALTER TABLE [Words] ADD [ReviewLastReviewedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513095009_AddLastReviewedAtToReview'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260513095009_AddLastReviewedAtToReview', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513201951_AddPredefinedWordsTable'
)
BEGIN
    CREATE TABLE [PredefinedWords] (
        [Id] uniqueidentifier NOT NULL,
        [Field] nvarchar(100) NOT NULL,
        [Category] nvarchar(100) NULL,
        [Original] nvarchar(200) NOT NULL,
        [Translation] nvarchar(400) NOT NULL,
        [AiSentence] nvarchar(1000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_PredefinedWords] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513201951_AddPredefinedWordsTable'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AiSentence', N'Category', N'CreatedAt', N'Field', N'IsActive', N'Original', N'Translation') AND [object_id] = OBJECT_ID(N'[PredefinedWords]'))
        SET IDENTITY_INSERT [PredefinedWords] ON;
    EXEC(N'INSERT INTO [PredefinedWords] ([Id], [AiSentence], [Category], [CreatedAt], [Field], [IsActive], [Original], [Translation])
    VALUES (''084e7fe8-13c8-4773-9b85-eb054b6b549a'', N''Diagnosis is the identification of a disease or condition.'', N''General'', ''2026-05-13T20:19:50.3469976Z'', N''Medicine'', CAST(1 AS bit), N''diagnosis'', N''tanı''),
    (''16b6999a-d974-48f5-9cdd-2a8221be9b39'', N''To debug means to find and fix errors in code.'', N''General'', ''2026-05-13T20:19:50.3469928Z'', N''Software'', CAST(1 AS bit), N''debug'', N''hata ayıklamak''),
    (''1b7a5571-9388-436e-ae9b-792959c973da'', N''A bug is an error or flaw in a software program.'', N''General'', ''2026-05-13T20:19:50.3469920Z'', N''Software'', CAST(1 AS bit), N''bug'', N''hata''),
    (''20fd5b19-adb8-49df-9786-b3118ae0c031'', N''Prognosis is the likely outcome of a disease.'', N''General'', ''2026-05-13T20:19:50.3469983Z'', N''Medicine'', CAST(1 AS bit), N''prognosis'', N''hastalık gidişi''),
    (''2ae720d9-5830-42b8-927d-997cdc5bc95a'', N''A lawsuit is a legal action brought in court.'', N''General'', ''2026-05-13T20:19:50.3470040Z'', N''Law'', CAST(1 AS bit), N''lawsuit'', N''dava''),
    (''5b28f01b-18ec-4431-b1c6-097d6930fc89'', N''An algorithm is a step-by-step procedure for solving a problem.'', N''General'', ''2026-05-13T20:19:50.3469823Z'', N''Software'', CAST(1 AS bit), N''algorithm'', N''algoritma''),
    (''6287df7b-8098-4954-9289-ad6523e46c35'', N''A symptom is a sign of illness or disease.'', N''General'', ''2026-05-13T20:19:50.3470016Z'', N''Medicine'', CAST(1 AS bit), N''symptom'', N''semptom''),
    (''6f1bcfdf-1fcd-4369-8d91-1c90c9c5b8cf'', N''Treatment is the medical care given for an illness.'', N''General'', ''2026-05-13T20:19:50.3470022Z'', N''Medicine'', CAST(1 AS bit), N''treatment'', N''tedavi''),
    (''72056339-4756-498f-a266-e05d77613a77'', N''A framework provides a foundation for building applications.'', N''General'', ''2026-05-13T20:19:50.3469936Z'', N''Software'', CAST(1 AS bit), N''framework'', N''framework''),
    (''72d2cf05-3dce-4b16-b125-ada4365580ac'', N''A defendant is a person accused of a crime.'', N''General'', ''2026-05-13T20:19:50.3470030Z'', N''Law'', CAST(1 AS bit), N''defendant'', N''davalı''),
    (''759719e5-a651-4ad9-a67d-2ae778331e83'', N''A verdict is the decision made by a court.'', N''General'', ''2026-05-13T20:19:50.3470035Z'', N''Law'', CAST(1 AS bit), N''verdict'', N''karar''),
    (''8d8343b9-de16-4113-9cd4-30614122a953'', N''An attorney is a lawyer who represents clients.'', N''General'', ''2026-05-13T20:19:50.3470045Z'', N''Law'', CAST(1 AS bit), N''attorney'', N''avukat''),
    (''a6265f55-78b6-495a-98cd-69588c5c58b3'', N''A repository is a central storage location for code.'', N''General'', ''2026-05-13T20:19:50.3469942Z'', N''Software'', CAST(1 AS bit), N''repository'', N''depo'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AiSentence', N'Category', N'CreatedAt', N'Field', N'IsActive', N'Original', N'Translation') AND [object_id] = OBJECT_ID(N'[PredefinedWords]'))
        SET IDENTITY_INSERT [PredefinedWords] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513201951_AddPredefinedWordsTable'
)
BEGIN
    CREATE INDEX [IX_PredefinedWords_Field_Category] ON [PredefinedWords] ([Field], [Category]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513201951_AddPredefinedWordsTable'
)
BEGIN
    CREATE INDEX [IX_PredefinedWords_Field_IsActive] ON [PredefinedWords] ([Field], [IsActive]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513201951_AddPredefinedWordsTable'
)
BEGIN
    CREATE INDEX [IX_PredefinedWords_Original] ON [PredefinedWords] ([Original]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513201951_AddPredefinedWordsTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260513201951_AddPredefinedWordsTable', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260515174532_AddUserVocabularyProgress'
)
BEGIN
    EXEC(N'DELETE FROM [PredefinedWords]
    WHERE [Id] = ''259b50bf-7c68-42f9-a671-64ab6b1b2281'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260515174532_AddUserVocabularyProgress'
)
BEGIN
    EXEC(N'DELETE FROM [PredefinedWords]
    WHERE [Id] = ''2eb089a7-4d99-4778-bde9-35ea32e46939'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260515174532_AddUserVocabularyProgress'
)
BEGIN
    EXEC(N'DELETE FROM [PredefinedWords]
    WHERE [Id] = ''3eb376f5-3412-4534-b342-03e3e13cab3a'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260515174532_AddUserVocabularyProgress'
)
BEGIN
    EXEC(N'DELETE FROM [PredefinedWords]
    WHERE [Id] = ''454b4275-6c87-4c55-ad76-0837d44993a5'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260515174532_AddUserVocabularyProgress'
)
BEGIN
    EXEC(N'DELETE FROM [PredefinedWords]
    WHERE [Id] = ''4a5f9438-b2a4-46be-8be7-5cb8c775309d'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260515174532_AddUserVocabularyProgress'
)
BEGIN
    EXEC(N'DELETE FROM [PredefinedWords]
    WHERE [Id] = ''5d090354-b672-4635-a76d-93a01e782b29'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260515174532_AddUserVocabularyProgress'
)
BEGIN
    EXEC(N'DELETE FROM [PredefinedWords]
    WHERE [Id] = ''8c2a3f5e-da7b-4052-ac2e-b73bc1bd7b10'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260515174532_AddUserVocabularyProgress'
)
BEGIN
    EXEC(N'DELETE FROM [PredefinedWords]
    WHERE [Id] = ''9518b5d7-3aea-4c4b-b001-969cef8b84ce'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260515174532_AddUserVocabularyProgress'
)
BEGIN
    EXEC(N'DELETE FROM [PredefinedWords]
    WHERE [Id] = ''a985bcd6-f85f-42c2-b4fc-fe5a80a32490'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260515174532_AddUserVocabularyProgress'
)
BEGIN
    EXEC(N'DELETE FROM [PredefinedWords]
    WHERE [Id] = ''c207887d-8c34-4b75-9065-c0f759e0bc50'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260515174532_AddUserVocabularyProgress'
)
BEGIN
    EXEC(N'DELETE FROM [PredefinedWords]
    WHERE [Id] = ''d8bc78d6-767c-44b1-9c49-8c8ea0d70983'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260515174532_AddUserVocabularyProgress'
)
BEGIN
    EXEC(N'DELETE FROM [PredefinedWords]
    WHERE [Id] = ''efa5d740-577a-4b2b-9e9b-b1926cdc9d8d'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260515174532_AddUserVocabularyProgress'
)
BEGIN
    EXEC(N'DELETE FROM [PredefinedWords]
    WHERE [Id] = ''f9eec26d-4ad1-4788-82e3-e9e060ec81d8'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260515174532_AddUserVocabularyProgress'
)
BEGIN
    CREATE TABLE [UserVocabularyProgresses] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [WordId] uniqueidentifier NOT NULL,
        [TotalAttempts] int NOT NULL,
        [CorrectAttempts] int NOT NULL,
        [AverageTimeTakenMs] bigint NOT NULL,
        [MinTimeTakenMs] bigint NOT NULL,
        [MaxTimeTakenMs] bigint NOT NULL,
        [LastSelectedAt] datetime2 NULL,
        [ConsecutiveSelections] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_UserVocabularyProgresses] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260515174532_AddUserVocabularyProgress'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AiSentence', N'Category', N'CreatedAt', N'Field', N'IsActive', N'Original', N'Translation') AND [object_id] = OBJECT_ID(N'[PredefinedWords]'))
        SET IDENTITY_INSERT [PredefinedWords] ON;
    EXEC(N'INSERT INTO [PredefinedWords] ([Id], [AiSentence], [Category], [CreatedAt], [Field], [IsActive], [Original], [Translation])
    VALUES (''02275182-c135-474d-9403-5f7c77716e5c'', N''A framework provides a foundation for building applications.'', N''General'', ''2026-05-15T17:45:31.1732486Z'', N''Software'', CAST(1 AS bit), N''framework'', N''framework''),
    (''061be113-13f2-40eb-9c19-e405cc215e4a'', N''A defendant is a person accused of a crime.'', N''General'', ''2026-05-15T17:45:31.1732525Z'', N''Law'', CAST(1 AS bit), N''defendant'', N''davalı''),
    (''0c3cc2b1-0633-46d6-bd62-3d72d92d2988'', N''To debug means to find and fix errors in code.'', N''General'', ''2026-05-15T17:45:31.1732483Z'', N''Software'', CAST(1 AS bit), N''debug'', N''hata ayıklamak''),
    (''229e63c8-ef36-4b58-beed-498dd9b32e58'', N''A symptom is a sign of illness or disease.'', N''General'', ''2026-05-15T17:45:31.1732512Z'', N''Medicine'', CAST(1 AS bit), N''symptom'', N''semptom''),
    (''356055e5-e47d-4d71-b0bf-b18e543126ba'', N''A repository is a central storage location for code.'', N''General'', ''2026-05-15T17:45:31.1732490Z'', N''Software'', CAST(1 AS bit), N''repository'', N''depo''),
    (''6a53fbf4-61b6-4f88-a842-f0504cbb8dff'', N''Prognosis is the likely outcome of a disease.'', N''General'', ''2026-05-15T17:45:31.1732509Z'', N''Medicine'', CAST(1 AS bit), N''prognosis'', N''hastalık gidişi''),
    (''8828ce07-34f7-485c-b4ff-0317e3b67e63'', N''An attorney is a lawyer who represents clients.'', N''General'', ''2026-05-15T17:45:31.1732536Z'', N''Law'', CAST(1 AS bit), N''attorney'', N''avukat''),
    (''8b2e0123-1b72-4b21-be5f-8f223730103e'', N''A lawsuit is a legal action brought in court.'', N''General'', ''2026-05-15T17:45:31.1732532Z'', N''Law'', CAST(1 AS bit), N''lawsuit'', N''dava''),
    (''bca7af0e-6048-4038-a35a-023335c0bee8'', N''An algorithm is a step-by-step procedure for solving a problem.'', N''General'', ''2026-05-15T17:45:31.1732466Z'', N''Software'', CAST(1 AS bit), N''algorithm'', N''algoritma''),
    (''c38d48ab-e002-4283-b736-98f7c28c9f01'', N''A verdict is the decision made by a court.'', N''General'', ''2026-05-15T17:45:31.1732529Z'', N''Law'', CAST(1 AS bit), N''verdict'', N''karar''),
    (''cf44c157-53ad-48b8-81c7-5e5e53bc9374'', N''A bug is an error or flaw in a software program.'', N''General'', ''2026-05-15T17:45:31.1732479Z'', N''Software'', CAST(1 AS bit), N''bug'', N''hata''),
    (''e42922af-d212-4459-a317-73c5a806aad1'', N''Treatment is the medical care given for an illness.'', N''General'', ''2026-05-15T17:45:31.1732520Z'', N''Medicine'', CAST(1 AS bit), N''treatment'', N''tedavi''),
    (''ed9e1d2f-12f4-45bb-84e0-7cc6252fdb30'', N''Diagnosis is the identification of a disease or condition.'', N''General'', ''2026-05-15T17:45:31.1732504Z'', N''Medicine'', CAST(1 AS bit), N''diagnosis'', N''tanı'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AiSentence', N'Category', N'CreatedAt', N'Field', N'IsActive', N'Original', N'Translation') AND [object_id] = OBJECT_ID(N'[PredefinedWords]'))
        SET IDENTITY_INSERT [PredefinedWords] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260515174532_AddUserVocabularyProgress'
)
BEGIN
    CREATE INDEX [IX_UserVocabularyProgresses_UserId_LastSelectedAt] ON [UserVocabularyProgresses] ([UserId], [LastSelectedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260515174532_AddUserVocabularyProgress'
)
BEGIN
    CREATE INDEX [IX_UserVocabularyProgresses_UserId_UpdatedAt] ON [UserVocabularyProgresses] ([UserId], [UpdatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260515174532_AddUserVocabularyProgress'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UserVocabularyProgresses_UserId_WordId] ON [UserVocabularyProgresses] ([UserId], [WordId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260515174532_AddUserVocabularyProgress'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260515174532_AddUserVocabularyProgress', N'8.0.0');
END;
GO

COMMIT;
GO

