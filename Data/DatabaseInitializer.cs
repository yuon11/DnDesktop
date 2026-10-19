
using System;
using System.IO;
using System.Data.SQLite;

namespace DnDesktop.Data
{
    public static class DatabaseInitializer
    {
        private static readonly string AppDataFolder =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "DnDesktop");

        public static string DatabasePath
        {
            get
            {
                return Path.Combine(
                    AppDataFolder,
                    "DnDesktop.db");
            }
        }
        private static void MigrateCharactersTable(
    SQLiteConnection connection)
        {
            string[,] columns =
            {
        { "Strength", "INTEGER NOT NULL DEFAULT 10" },
        { "Dexterity", "INTEGER NOT NULL DEFAULT 10" },
        { "Constitution", "INTEGER NOT NULL DEFAULT 10" },
        { "Intelligence", "INTEGER NOT NULL DEFAULT 10" },
        { "Wisdom", "INTEGER NOT NULL DEFAULT 10" },
        { "Charisma", "INTEGER NOT NULL DEFAULT 10" },
        { "ArmorClass", "INTEGER NOT NULL DEFAULT 10" },
        { "Initiative", "INTEGER NOT NULL DEFAULT 0" },
        { "MaxHP", "INTEGER NOT NULL DEFAULT 10" },
        { "CurrentHP", "INTEGER NOT NULL DEFAULT 10" },
        { "TemporaryHP", "INTEGER NOT NULL DEFAULT 0" },
        { "Speed", "INTEGER NOT NULL DEFAULT 30" },
        { "HitDice", "TEXT" }
    };

            for (int i = 0; i < columns.GetLength(0); i++)
            {
                string columnName = columns[i, 0];
                string columnDefinition = columns[i, 1];

                bool columnExists = false;

                using (SQLiteCommand command = new SQLiteCommand(
                    "PRAGMA table_info(Characters);",
                    connection))
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (string.Equals(
                            reader["name"].ToString(),
                            columnName,
                            StringComparison.OrdinalIgnoreCase))
                        {
                            columnExists = true;
                            break;
                        }
                    }
                }

                if (!columnExists)
                {
                    string sql =
                        "ALTER TABLE Characters ADD COLUMN " +
                        columnName + " " + columnDefinition + ";";

                    using (SQLiteCommand command =
                        new SQLiteCommand(sql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }
        public static void Initialize()
        {
            // Create the application data directory if needed.
            Directory.CreateDirectory(AppDataFolder);

            // Create the database file automatically if missing.
            if (!File.Exists(DatabasePath))
            {
                SQLiteConnection.CreateFile(DatabasePath);
            }

            string connectionString =
                "Data Source=" + DatabasePath + ";Version=3;";

            using (SQLiteConnection connection =
                   new SQLiteConnection(connectionString))
            {
                connection.Open();

                // SQLite foreign-key enforcement is enabled per connection.
                using (SQLiteCommand pragmaCommand =
                       new SQLiteCommand(
                           "PRAGMA foreign_keys = ON;",
                           connection))
                {
                    pragmaCommand.ExecuteNonQuery();
                }

                string createTables = @"
                    CREATE TABLE IF NOT EXISTS Characters
                    (
                        Id TEXT PRIMARY KEY,
                        Name TEXT NOT NULL,
                        PlayerName TEXT,
                        Race TEXT,
                        Subrace TEXT,
                        CharacterClass TEXT,
                        Subclass TEXT,
                        Level INTEGER NOT NULL DEFAULT 1,
                        Background TEXT,
                        CreatedDate TEXT NOT NULL,
                        Strength INTEGER NOT NULL DEFAULT 10,
                        Dexterity INTEGER NOT NULL DEFAULT 10,
                        Constitution INTEGER NOT NULL DEFAULT 10,
                        Intelligence INTEGER NOT NULL DEFAULT 10,
                        Wisdom INTEGER NOT NULL DEFAULT 10,
                        Charisma INTEGER NOT NULL DEFAULT 10,
                        ArmorClass INTEGER NOT NULL DEFAULT 10,
                        Initiative INTEGER NOT NULL DEFAULT 0,
                        MaxHP INTEGER NOT NULL DEFAULT 10,
                        CurrentHP INTEGER NOT NULL DEFAULT 10,
                        TemporaryHP INTEGER NOT NULL DEFAULT 0,
                        Speed INTEGER NOT NULL DEFAULT 30,
                        HitDice TEXT,
                        LastModified TEXT NOT NULL
                    );

                    CREATE TABLE IF NOT EXISTS Locations
                    (
                        Id TEXT PRIMARY KEY,
                        Name TEXT NOT NULL,
                        LocationType TEXT NOT NULL DEFAULT 'Other',
                        ParentLocationId TEXT NULL,
                        Description TEXT,
                        Population TEXT,
                        Government TEXT,
                        NotableNPCs TEXT,
                        PointsOfInterest TEXT,
                        GMNotes TEXT,
                        CreatedDate TEXT NOT NULL,
                        LastModified TEXT NOT NULL,

                        FOREIGN KEY (ParentLocationId)
                            REFERENCES Locations(Id)
                            ON DELETE SET NULL
                    );

                    CREATE TABLE IF NOT EXISTS LocationImages
                    (
                        Id TEXT PRIMARY KEY,
                        LocationId TEXT NOT NULL,
                        ImagePath TEXT NOT NULL,
                        Caption TEXT,
                        ImageCategory TEXT NOT NULL DEFAULT 'Other',
                        IsPrimary INTEGER NOT NULL DEFAULT 0,
                        DisplayOrder INTEGER NOT NULL DEFAULT 0,
                        CreatedDate TEXT NOT NULL,

                        FOREIGN KEY (LocationId)
                            REFERENCES Locations(Id)
                            ON DELETE CASCADE
                    );

                    CREATE TABLE IF NOT EXISTS Creatures
                    (
                        Id TEXT PRIMARY KEY,
                        Name TEXT NOT NULL,
                        CreatureType TEXT NOT NULL DEFAULT 'Other',
                        Subtype TEXT,
                        Size TEXT NOT NULL DEFAULT 'Medium',
                        Alignment TEXT,
                        Description TEXT,

                        ArmorClass INTEGER NOT NULL DEFAULT 10,
                        ArmorClassNotes TEXT,
                        HitPoints INTEGER NOT NULL DEFAULT 1,
                        HitDice TEXT,
                        InitiativeModifier INTEGER NOT NULL DEFAULT 0,
                        ChallengeRating TEXT NOT NULL DEFAULT '0',
                        ProficiencyBonus INTEGER NOT NULL DEFAULT 2,
                        ExperiencePoints INTEGER NOT NULL DEFAULT 0,

                        Strength INTEGER NOT NULL DEFAULT 10,
                        Dexterity INTEGER NOT NULL DEFAULT 10,
                        Constitution INTEGER NOT NULL DEFAULT 10,
                        Intelligence INTEGER NOT NULL DEFAULT 10,
                        Wisdom INTEGER NOT NULL DEFAULT 10,
                        Charisma INTEGER NOT NULL DEFAULT 10,

                        WalkSpeed INTEGER NOT NULL DEFAULT 30,
                        FlySpeed INTEGER NOT NULL DEFAULT 0,
                        SwimSpeed INTEGER NOT NULL DEFAULT 0,
                        ClimbSpeed INTEGER NOT NULL DEFAULT 0,
                        BurrowSpeed INTEGER NOT NULL DEFAULT 0,
                        SpecialMovement TEXT,

                        SavingThrows TEXT,
                        Skills TEXT,
                        DamageVulnerabilities TEXT,
                        DamageResistances TEXT,
                        DamageImmunities TEXT,
                        ConditionImmunities TEXT,

                        Senses TEXT,
                        PassivePerception INTEGER NOT NULL DEFAULT 10,
                        Languages TEXT,

                        Traits TEXT,
                        LegendaryActions TEXT,
                        LairActions TEXT,
                        Reactions TEXT,
                        Spellcasting TEXT,
                        Habitat TEXT,
                        Behavior TEXT,
                        Tactics TEXT,
                        GMNotes TEXT,

                        CreatedDate TEXT NOT NULL,
                        LastModified TEXT NOT NULL
                    );

                    CREATE TABLE IF NOT EXISTS CreatureAttacks
                    (
                        Id TEXT PRIMARY KEY,
                        CreatureId TEXT NOT NULL,

                        Name TEXT NOT NULL,
                        AttackType TEXT,
                        AttackBonus INTEGER NOT NULL DEFAULT 0,

                        Reach TEXT,
                        Range TEXT,
                        Target TEXT,

                        DamageDice TEXT,
                        DamageType TEXT,
                        AdditionalDamage TEXT,

                        SavingThrow TEXT,
                        Effect TEXT,
                        Recharge TEXT,
                        Notes TEXT,

                        DisplayOrder INTEGER NOT NULL DEFAULT 0,

                        FOREIGN KEY (CreatureId)
                            REFERENCES Creatures(Id)
                            ON DELETE CASCADE
                    );

                    CREATE TABLE IF NOT EXISTS CreatureAbilities
                    (
                        Id TEXT PRIMARY KEY,
                        CreatureId TEXT NOT NULL,

                        Name TEXT NOT NULL,
                        AbilityType TEXT,
                        Description TEXT,

                        Activation TEXT,
                        Recharge TEXT,
                        Uses TEXT,

                        SavingThrow TEXT,
                        Effect TEXT,

                        DisplayOrder INTEGER NOT NULL DEFAULT 0,

                        FOREIGN KEY (CreatureId)
                            REFERENCES Creatures(Id)
                            ON DELETE CASCADE
                    );

                    CREATE INDEX IF NOT EXISTS
                        IX_CreatureAttacks_CreatureId
                    ON CreatureAttacks(CreatureId);

                    CREATE INDEX IF NOT EXISTS
                        IX_CreatureAbilities_CreatureId
                    ON CreatureAbilities(CreatureId);

                    CREATE TABLE IF NOT EXISTS CharacterFeats
                    (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        CharacterId TEXT NOT NULL,
                        Name TEXT NOT NULL,
                        Description TEXT,

                        FOREIGN KEY (CharacterId)
                            REFERENCES Characters(Id)
                            ON DELETE CASCADE
                    );

                    CREATE INDEX IF NOT EXISTS IX_CharacterFeats_CharacterId
                        ON CharacterFeats(CharacterId);


                    CREATE TABLE IF NOT EXISTS Campaigns
                    (
                        Id TEXT PRIMARY KEY,
                        Name TEXT NOT NULL,
                        Description TEXT,
                        CampaignNotes TEXT,
                        CreatedDate TEXT NOT NULL,
                        LastModified TEXT NOT NULL
                    );

                    CREATE TABLE IF NOT EXISTS CampaignPartyMembers
                    (
                        CampaignId TEXT NOT NULL,
                        CharacterId TEXT NOT NULL,

                        PRIMARY KEY (CampaignId, CharacterId),

                        FOREIGN KEY (CampaignId)
                            REFERENCES Campaigns(Id)
                            ON DELETE CASCADE,

                        FOREIGN KEY (CharacterId)
                            REFERENCES Characters(Id)
                            ON DELETE CASCADE
                    );

                    CREATE INDEX IF NOT EXISTS IX_CampaignPartyMembers_CharacterId
                        ON CampaignPartyMembers(CharacterId);


                    CREATE TABLE IF NOT EXISTS CampaignNPCs
                    (
                        CampaignId TEXT NOT NULL,
                        CharacterId TEXT NOT NULL,

                        PRIMARY KEY (CampaignId, CharacterId),

                        FOREIGN KEY (CampaignId)
                            REFERENCES Campaigns(Id)
                            ON DELETE CASCADE,

                        FOREIGN KEY (CharacterId)
                            REFERENCES Characters(Id)
                            ON DELETE CASCADE
                    );

                    CREATE INDEX IF NOT EXISTS IX_CampaignNPCs_CharacterId
                        ON CampaignNPCs(CharacterId);
                
                    CREATE TABLE IF NOT EXISTS Quests
                    (
                        Id TEXT PRIMARY KEY,
                        CampaignId TEXT NOT NULL,
                        Name TEXT NOT NULL,
                        Description TEXT,
                        QuestGiverId TEXT,
                        Status TEXT NOT NULL DEFAULT 'Not Started',
                        Notes TEXT,
                        CreatedDate TEXT NOT NULL,
                        LastModified TEXT NOT NULL,

                        FOREIGN KEY (CampaignId)
                            REFERENCES Campaigns(Id)
                            ON DELETE CASCADE,

                        FOREIGN KEY (QuestGiverId)
                            REFERENCES Characters(Id)
                            ON DELETE SET NULL
                    );

                    CREATE INDEX IF NOT EXISTS IX_Quests_CampaignId
                        ON Quests(CampaignId);

                    CREATE INDEX IF NOT EXISTS IX_Quests_QuestGiverId
                        ON Quests(QuestGiverId);

                    CREATE TABLE IF NOT EXISTS CampaignLocations
                    (
                        CampaignId TEXT NOT NULL,
                        LocationId TEXT NOT NULL,

                        PRIMARY KEY (CampaignId, LocationId),

                        FOREIGN KEY (CampaignId)
                            REFERENCES Campaigns(Id)
                            ON DELETE CASCADE,

                        FOREIGN KEY (LocationId)
                            REFERENCES Locations(Id)
                            ON DELETE CASCADE
                    );

                    CREATE INDEX IF NOT EXISTS IX_CampaignLocations_LocationId
                        ON CampaignLocations(LocationId);

                    CREATE TABLE IF NOT EXISTS CampaignSessions
                    (
                        Id TEXT PRIMARY KEY,
                        CampaignId TEXT NOT NULL,
                        Title TEXT NOT NULL,
                        SessionDate TEXT,
                        Status TEXT NOT NULL DEFAULT 'Planned',
                        Summary TEXT,
                        SessionNotes TEXT,
                        CreatedDate TEXT NOT NULL,
                        LastModified TEXT NOT NULL,

                        FOREIGN KEY (CampaignId)
                            REFERENCES Campaigns(Id)
                            ON DELETE CASCADE
                    );

                    CREATE INDEX IF NOT EXISTS IX_CampaignSessions_CampaignId
                        ON CampaignSessions(CampaignId);

                    CREATE INDEX IF NOT EXISTS IX_CampaignSessions_SessionDate
                        ON CampaignSessions(SessionDate);

                    CREATE TABLE IF NOT EXISTS SessionEncounters
                    (
                        Id TEXT PRIMARY KEY,
                        SessionId TEXT NOT NULL,
                        Name TEXT NOT NULL,
                        EncounterNotes TEXT,
                        CreatedDate TEXT NOT NULL,
                        LastModified TEXT NOT NULL,
                        FOREIGN KEY (SessionId)
                            REFERENCES CampaignSessions(Id)
                            ON DELETE CASCADE
                    );

                    CREATE INDEX IF NOT EXISTS IX_SessionEncounters_SessionId
                        ON SessionEncounters(SessionId);

                    CREATE TABLE IF NOT EXISTS SessionEncounterCreatures
                    (
                        EncounterId TEXT NOT NULL,
                        CreatureId TEXT NOT NULL,
                        Quantity INTEGER NOT NULL DEFAULT 1
                            CHECK (Quantity > 0),
                        PRIMARY KEY (EncounterId, CreatureId),
                        FOREIGN KEY (EncounterId)
                            REFERENCES SessionEncounters(Id)
                            ON DELETE CASCADE,
                        FOREIGN KEY (CreatureId)
                            REFERENCES Creatures(Id)
                            ON DELETE CASCADE
                    );

                    CREATE INDEX IF NOT EXISTS IX_SessionEncounterCreatures_CreatureId
                        ON SessionEncounterCreatures(CreatureId);
                ";

                using (SQLiteCommand command =
                       new SQLiteCommand(createTables, connection))
                {
                    command.ExecuteNonQuery();
                }
                MigrateCharactersTable(connection);
            }
        }
    }

    internal static class SQLiteFileExtensions
    {
        public static void CreateFileIfNotExists(
            string databasePath)
        {
            if (!File.Exists(databasePath))
            {
                SQLiteConnection.CreateFile(databasePath);
            }
        }
    }

}
