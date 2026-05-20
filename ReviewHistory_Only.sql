BEGIN TRANSACTION;
GO

CREATE TABLE [ReviewHistories] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [WordId] uniqueidentifier NOT NULL,
    [IsCorrect] bit NOT NULL,
    [Outcome] int NOT NULL,
    [QScore] int NULL,
    [TimeTakenMs] bigint NULL,
    [ReviewedAt] datetime2 NOT NULL,
    [NextReviewAt] datetime2 NOT NULL,
    [IntervalDays] int NOT NULL,
    [EaseFactor] real NOT NULL,
    [Repetitions] int NOT NULL,
    [Source] int NOT NULL,
    [SessionId] uniqueidentifier NULL,
    [ClientVersion] nvarchar(50) NULL,
    CONSTRAINT [PK_ReviewHistories] PRIMARY KEY ([Id])
);
GO

CREATE INDEX [IX_ReviewHistories_UserId_WordId] ON [ReviewHistories] ([UserId], [WordId]);
GO

CREATE INDEX [IX_ReviewHistories_UserId_ReviewedAt] ON [ReviewHistories] ([UserId], [ReviewedAt]);
GO

CREATE INDEX [IX_ReviewHistories_WordId_ReviewedAt] ON [ReviewHistories] ([WordId], [ReviewedAt]);
GO

CREATE INDEX [IX_ReviewHistories_UserId_Outcome] ON [ReviewHistories] ([UserId], [Outcome]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260518170000_AddReviewHistory', N'8.0.0');
GO

COMMIT;
GO

