/* =============================================================
   BATTLEGAME database - Database First script
   Creates database, tables, stored procedures and sample data.
   Run with: sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i BattleGame.sql
   ============================================================= */

IF DB_ID(N'BATTLEGAME') IS NULL
    CREATE DATABASE BATTLEGAME;
GO

USE BATTLEGAME;
GO

/* ---------------------- Drop existing objects ---------------------- */
IF OBJECT_ID(N'dbo.usp_GetAssetsByPlayer', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_GetAssetsByPlayer;
IF OBJECT_ID(N'dbo.usp_AddPlayerAsset', N'P')    IS NOT NULL DROP PROCEDURE dbo.usp_AddPlayerAsset;
IF OBJECT_ID(N'dbo.usp_CreateAsset', N'P')       IS NOT NULL DROP PROCEDURE dbo.usp_CreateAsset;
IF OBJECT_ID(N'dbo.usp_RegisterPlayer', N'P')    IS NOT NULL DROP PROCEDURE dbo.usp_RegisterPlayer;
IF OBJECT_ID(N'dbo.PlayerAsset', N'U') IS NOT NULL DROP TABLE dbo.PlayerAsset;
IF OBJECT_ID(N'dbo.Player', N'U')      IS NOT NULL DROP TABLE dbo.Player;
IF OBJECT_ID(N'dbo.Asset', N'U')       IS NOT NULL DROP TABLE dbo.Asset;
GO

/* ------------------------------ Tables ----------------------------- */
CREATE TABLE dbo.Asset
(
    AssetId      UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Asset_AssetId DEFAULT NEWID(),
    AssetName    NVARCHAR(64)     NOT NULL,
    LevelRequire INT              NOT NULL CONSTRAINT DF_Asset_LevelRequire DEFAULT 1,
    CONSTRAINT PK_Asset PRIMARY KEY (AssetId)
);
GO

CREATE TABLE dbo.Player
(
    PlayerId   UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Player_PlayerId DEFAULT NEWID(),
    PlayerName NVARCHAR(64)     NOT NULL,
    FullName   NVARCHAR(128)    NULL,
    Age        NVARCHAR(10)     NULL,
    [Level]    INT              NOT NULL CONSTRAINT DF_Player_Level DEFAULT 1,
    Email      NVARCHAR(64)     NULL,
    CONSTRAINT PK_Player PRIMARY KEY (PlayerId),
    CONSTRAINT UQ_Player_PlayerName UNIQUE (PlayerName)
);
GO

CREATE TABLE dbo.PlayerAsset
(
    PlayerId UNIQUEIDENTIFIER NOT NULL,
    AssetId  UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_PlayerAsset PRIMARY KEY (PlayerId, AssetId),
    CONSTRAINT FK_PlayerAsset_Player FOREIGN KEY (PlayerId) REFERENCES dbo.Player (PlayerId) ON DELETE CASCADE,
    CONSTRAINT FK_PlayerAsset_Asset  FOREIGN KEY (AssetId)  REFERENCES dbo.Asset (AssetId)   ON DELETE CASCADE
);
GO

/* ------------------------- Stored procedures ------------------------ */
CREATE PROCEDURE dbo.usp_RegisterPlayer
    @PlayerName NVARCHAR(64),
    @FullName   NVARCHAR(128),
    @Age        NVARCHAR(10),
    @Level      INT,
    @Email      NVARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @PlayerId UNIQUEIDENTIFIER = NEWID();

    INSERT INTO dbo.Player (PlayerId, PlayerName, FullName, Age, [Level], Email)
    VALUES (@PlayerId, @PlayerName, @FullName, @Age, @Level, @Email);

    SELECT PlayerId, PlayerName, FullName, Age, [Level], Email
    FROM dbo.Player
    WHERE PlayerId = @PlayerId;
END
GO

CREATE PROCEDURE dbo.usp_CreateAsset
    @AssetName    NVARCHAR(64),
    @LevelRequire INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @AssetId UNIQUEIDENTIFIER = NEWID();

    INSERT INTO dbo.Asset (AssetId, AssetName, LevelRequire)
    VALUES (@AssetId, @AssetName, @LevelRequire);

    SELECT AssetId, AssetName, LevelRequire
    FROM dbo.Asset
    WHERE AssetId = @AssetId;
END
GO

CREATE PROCEDURE dbo.usp_AddPlayerAsset
    @PlayerId UNIQUEIDENTIFIER,
    @AssetId  UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.PlayerAsset (PlayerId, AssetId)
    VALUES (@PlayerId, @AssetId);
END
GO

/* Report: assets of each player (optionally filtered by player name) */
CREATE PROCEDURE dbo.usp_GetAssetsByPlayer
    @PlayerName NVARCHAR(64) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ROW_NUMBER() OVER (ORDER BY p.PlayerName, a.AssetName) AS [No],
           p.PlayerName,
           p.[Level],
           p.Age,
           a.AssetName
    FROM dbo.PlayerAsset pa
    INNER JOIN dbo.Player p ON p.PlayerId = pa.PlayerId
    INNER JOIN dbo.Asset  a ON a.AssetId  = pa.AssetId
    WHERE @PlayerName IS NULL OR p.PlayerName = @PlayerName
    ORDER BY [No];
END
GO

/* ---------------------------- Sample data --------------------------- */
DECLARE @Hero1 UNIQUEIDENTIFIER = NEWID(),
        @Hero2 UNIQUEIDENTIFIER = NEWID(),
        @Sword UNIQUEIDENTIFIER = NEWID(),
        @P1    UNIQUEIDENTIFIER = NEWID(),
        @P2    UNIQUEIDENTIFIER = NEWID(),
        @P3    UNIQUEIDENTIFIER = NEWID();

INSERT INTO dbo.Asset (AssetId, AssetName, LevelRequire) VALUES
    (@Hero1, N'Hero 1', 1),
    (@Hero2, N'Hero 2', 3),
    (@Sword, N'Dragon Sword', 5);

INSERT INTO dbo.Player (PlayerId, PlayerName, FullName, Age, [Level], Email) VALUES
    (@P1, N'Player 1', N'Nguyen Van A', N'20', 10, N'player1@battlegame.com'),
    (@P2, N'Player 2', N'Tran Thi B',   N'19', 3,  N'player2@battlegame.com'),
    (@P3, N'Player 3', N'Le Van C',     N'23', 10, N'player3@battlegame.com');

INSERT INTO dbo.PlayerAsset (PlayerId, AssetId) VALUES
    (@P1, @Hero1),
    (@P2, @Hero2),
    (@P3, @Hero1);
GO
