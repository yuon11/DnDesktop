
using DnDesktop.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;

namespace DnDesktop.Data
{
    public class CreatureRepository
    {
        private readonly string _connectionString;

        public CreatureRepository()
        {
            _connectionString =
                "Data Source=" + DatabaseInitializer.DatabasePath +
                ";Version=3;Foreign Keys=True;";
        }

        private SQLiteConnection OpenConnection()
        {
            SQLiteConnection connection =
                new SQLiteConnection(_connectionString);

            connection.Open();

            using (SQLiteCommand command =
                new SQLiteCommand("PRAGMA foreign_keys = ON;", connection))
            {
                command.ExecuteNonQuery();
            }

            return connection;
        }

        // Retrieve all creatures.
        public List<Creature> GetAll()
        {
            List<Creature> creatures = new List<Creature>();

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command = new SQLiteCommand(
                "SELECT * FROM Creatures ORDER BY Name;", connection))
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    creatures.Add(ReadCreature(reader));
                }
            }

            return creatures;
        }

        // Retrieve a creature by ID.
        public Creature GetById(Guid id)
        {
            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command = new SQLiteCommand(
                "SELECT * FROM Creatures WHERE Id = @Id;", connection))
            {
                command.Parameters.AddWithValue("@Id", id.ToString());

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return ReadCreature(reader);
                    }
                }
            }

            return null;
        }

        public Creature GetWithDetails(Guid id)
        {
            Creature creature = GetById(id);

            if (creature == null)
                return null;

            // Load related records to verify they can be retrieved.
            // The current Creature model stores these separately,
            // so the lists are returned through the repository methods.
            return creature;
        }

        // Insert a new creature.
        public void Add(Creature creature)
        {
            ValidateCreature(creature);

            if (creature.Id == Guid.Empty)
                creature.Id = Guid.NewGuid();

            if (creature.CreatedDate == default(DateTime))
                creature.CreatedDate = DateTime.Now;

            creature.LastModified = DateTime.Now;

            const string sql = @"
                INSERT INTO Creatures
                (
                    Id, Name, CreatureType, Subtype, Size, Alignment,
                    Description,
                    ArmorClass, ArmorClassNotes, HitPoints, HitDice,
                    InitiativeModifier, ChallengeRating, ProficiencyBonus,
                    ExperiencePoints,
                    Strength, Dexterity, Constitution, Intelligence,
                    Wisdom, Charisma,
                    WalkSpeed, FlySpeed, SwimSpeed, ClimbSpeed,
                    BurrowSpeed, SpecialMovement,
                    SavingThrows, Skills, DamageVulnerabilities,
                    DamageResistances, DamageImmunities, ConditionImmunities,
                    Senses, PassivePerception, Languages,
                    Traits, LegendaryActions, LairActions, Reactions,
                    Spellcasting, Habitat, Behavior, Tactics, GMNotes,
                    CreatedDate, LastModified
                )
                VALUES
                (
                    @Id, @Name, @CreatureType, @Subtype, @Size, @Alignment,
                    @Description,
                    @ArmorClass, @ArmorClassNotes, @HitPoints, @HitDice,
                    @InitiativeModifier, @ChallengeRating, @ProficiencyBonus,
                    @ExperiencePoints,
                    @Strength, @Dexterity, @Constitution, @Intelligence,
                    @Wisdom, @Charisma,
                    @WalkSpeed, @FlySpeed, @SwimSpeed, @ClimbSpeed,
                    @BurrowSpeed, @SpecialMovement,
                    @SavingThrows, @Skills, @DamageVulnerabilities,
                    @DamageResistances, @DamageImmunities, @ConditionImmunities,
                    @Senses, @PassivePerception, @Languages,
                    @Traits, @LegendaryActions, @LairActions, @Reactions,
                    @Spellcasting, @Habitat, @Behavior, @Tactics, @GMNotes,
                    @CreatedDate, @LastModified
                );";

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command =
                new SQLiteCommand(sql, connection))
            {
                AddCreatureParameters(command, creature);
                command.ExecuteNonQuery();
            }
        }

        // Update an existing creature.
        public void Update(Creature creature)
        {
            ValidateCreature(creature);

            creature.LastModified = DateTime.Now;

            const string sql = @"
                UPDATE Creatures SET
                    Name = @Name,
                    CreatureType = @CreatureType,
                    Subtype = @Subtype,
                    Size = @Size,
                    Alignment = @Alignment,
                    Description = @Description,

                    ArmorClass = @ArmorClass,
                    ArmorClassNotes = @ArmorClassNotes,
                    HitPoints = @HitPoints,
                    HitDice = @HitDice,
                    InitiativeModifier = @InitiativeModifier,
                    ChallengeRating = @ChallengeRating,
                    ProficiencyBonus = @ProficiencyBonus,
                    ExperiencePoints = @ExperiencePoints,

                    Strength = @Strength,
                    Dexterity = @Dexterity,
                    Constitution = @Constitution,
                    Intelligence = @Intelligence,
                    Wisdom = @Wisdom,
                    Charisma = @Charisma,

                    WalkSpeed = @WalkSpeed,
                    FlySpeed = @FlySpeed,
                    SwimSpeed = @SwimSpeed,
                    ClimbSpeed = @ClimbSpeed,
                    BurrowSpeed = @BurrowSpeed,
                    SpecialMovement = @SpecialMovement,

                    SavingThrows = @SavingThrows,
                    Skills = @Skills,
                    DamageVulnerabilities = @DamageVulnerabilities,
                    DamageResistances = @DamageResistances,
                    DamageImmunities = @DamageImmunities,
                    ConditionImmunities = @ConditionImmunities,

                    Senses = @Senses,
                    PassivePerception = @PassivePerception,
                    Languages = @Languages,

                    Traits = @Traits,
                    LegendaryActions = @LegendaryActions,
                    LairActions = @LairActions,
                    Reactions = @Reactions,
                    Spellcasting = @Spellcasting,
                    Habitat = @Habitat,
                    Behavior = @Behavior,
                    Tactics = @Tactics,
                    GMNotes = @GMNotes,

                    LastModified = @LastModified
                WHERE Id = @Id;";

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command =
                new SQLiteCommand(sql, connection))
            {
                AddCreatureParameters(command, creature);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    throw new InvalidOperationException(
                        "The creature could not be found.");
                }
            }
        }

        // Delete a creature. Its attacks and abilities are deleted
        // automatically by the database's ON DELETE CASCADE rules.
        public void Delete(Guid id)
        {
            const string sql =
                "DELETE FROM Creatures WHERE Id = @Id;";

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command =
                new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Id", id.ToString());

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    throw new InvalidOperationException(
                        "The creature could not be found.");
                }
            }
        }

        // Retrieve a creature's attacks.
        public List<CreatureAttack> GetAttacks(Guid creatureId)
        {
            List<CreatureAttack> attacks =
                new List<CreatureAttack>();

            const string sql = @"
                SELECT * FROM CreatureAttacks
                WHERE CreatureId = @CreatureId
                ORDER BY DisplayOrder, Name;";

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command =
                new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddWithValue(
                    "@CreatureId", creatureId.ToString());

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        attacks.Add(ReadAttack(reader));
                    }
                }
            }

            return attacks;
        }

        // Add an attack to a creature.
        public void AddAttack(CreatureAttack attack)
        {
            if (attack == null)
                throw new ArgumentNullException("attack");

            if (string.IsNullOrWhiteSpace(attack.Name))
                throw new ArgumentException("Attack name is required.");

            if (attack.Id == Guid.Empty)
                attack.Id = Guid.NewGuid();

            const string sql = @"
                INSERT INTO CreatureAttacks
                (
                    Id, CreatureId, Name, AttackType, AttackBonus,
                    Reach, Range, Target, DamageDice, DamageType,
                    AdditionalDamage, SavingThrow, Effect, Recharge,
                    Notes, DisplayOrder
                )
                VALUES
                (
                    @Id, @CreatureId, @Name, @AttackType, @AttackBonus,
                    @Reach, @Range, @Target, @DamageDice, @DamageType,
                    @AdditionalDamage, @SavingThrow, @Effect, @Recharge,
                    @Notes, @DisplayOrder
                );";

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command =
                new SQLiteCommand(sql, connection))
            {
                AddAttackParameters(command, attack);
                command.ExecuteNonQuery();
            }
        }

        // Update an existing attack.
        public void UpdateAttack(CreatureAttack attack)
        {
            if (attack == null)
                throw new ArgumentNullException("attack");

            if (string.IsNullOrWhiteSpace(attack.Name))
                throw new ArgumentException("Attack name is required.");

            const string sql = @"
                UPDATE CreatureAttacks SET
                    CreatureId = @CreatureId,
                    Name = @Name,
                    AttackType = @AttackType,
                    AttackBonus = @AttackBonus,
                    Reach = @Reach,
                    Range = @Range,
                    Target = @Target,
                    DamageDice = @DamageDice,
                    DamageType = @DamageType,
                    AdditionalDamage = @AdditionalDamage,
                    SavingThrow = @SavingThrow,
                    Effect = @Effect,
                    Recharge = @Recharge,
                    Notes = @Notes,
                    DisplayOrder = @DisplayOrder
                WHERE Id = @Id;";

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command =
                new SQLiteCommand(sql, connection))
            {
                AddAttackParameters(command, attack);

                if (command.ExecuteNonQuery() == 0)
                {
                    throw new InvalidOperationException(
                        "The attack could not be found.");
                }
            }
        }

        // Delete one attack.
        public void DeleteAttack(Guid id)
        {
            DeleteChildRecord("CreatureAttacks", id, "attack");
        }

        // Retrieve a creature's abilities.
        public List<CreatureAbility> GetAbilities(Guid creatureId)
        {
            List<CreatureAbility> abilities =
                new List<CreatureAbility>();

            const string sql = @"
                SELECT * FROM CreatureAbilities
                WHERE CreatureId = @CreatureId
                ORDER BY DisplayOrder, Name;";

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command =
                new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddWithValue(
                    "@CreatureId", creatureId.ToString());

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        abilities.Add(ReadAbility(reader));
                    }
                }
            }

            return abilities;
        }

        // Add an ability to a creature.
        public void AddAbility(CreatureAbility ability)
        {
            if (ability == null)
                throw new ArgumentNullException("ability");

            if (string.IsNullOrWhiteSpace(ability.Name))
                throw new ArgumentException("Ability name is required.");

            if (ability.Id == Guid.Empty)
                ability.Id = Guid.NewGuid();

            const string sql = @"
                INSERT INTO CreatureAbilities
                (
                    Id, CreatureId, Name, AbilityType, Description,
                    Activation, Recharge, Uses, SavingThrow, Effect,
                    DisplayOrder
                )
                VALUES
                (
                    @Id, @CreatureId, @Name, @AbilityType, @Description,
                    @Activation, @Recharge, @Uses, @SavingThrow, @Effect,
                    @DisplayOrder
                );";

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command =
                new SQLiteCommand(sql, connection))
            {
                AddAbilityParameters(command, ability);
                command.ExecuteNonQuery();
            }
        }

        // Update an existing ability.
        public void UpdateAbility(CreatureAbility ability)
        {
            if (ability == null)
                throw new ArgumentNullException("ability");

            if (string.IsNullOrWhiteSpace(ability.Name))
                throw new ArgumentException("Ability name is required.");

            const string sql = @"
                UPDATE CreatureAbilities SET
                    CreatureId = @CreatureId,
                    Name = @Name,
                    AbilityType = @AbilityType,
                    Description = @Description,
                    Activation = @Activation,
                    Recharge = @Recharge,
                    Uses = @Uses,
                    SavingThrow = @SavingThrow,
                    Effect = @Effect,
                    DisplayOrder = @DisplayOrder
                WHERE Id = @Id;";

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command =
                new SQLiteCommand(sql, connection))
            {
                AddAbilityParameters(command, ability);

                if (command.ExecuteNonQuery() == 0)
                {
                    throw new InvalidOperationException(
                        "The ability could not be found.");
                }
            }
        }

        // Delete one ability.
        public void DeleteAbility(Guid id)
        {
            DeleteChildRecord("CreatureAbilities", id, "ability");
        }

        private void DeleteChildRecord(
            string tableName, Guid id, string recordName)
        {
            // Only repository-controlled table names are passed here.
            string sql = "DELETE FROM " + tableName + " WHERE Id = @Id;";

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command =
                new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Id", id.ToString());

                if (command.ExecuteNonQuery() == 0)
                {
                    throw new InvalidOperationException(
                        "The " + recordName + " could not be found.");
                }
            }
        }

        private static void ValidateCreature(Creature creature)
        {
            if (creature == null)
                throw new ArgumentNullException("creature");

            if (string.IsNullOrWhiteSpace(creature.Name))
                throw new ArgumentException("Creature name is required.");
        }

        private static Creature ReadCreature(SQLiteDataReader reader)
        {
            Creature creature = new Creature();

            creature.Id = Guid.Parse(reader["Id"].ToString());
            creature.Name = reader["Name"].ToString();
            creature.CreatureType = reader["CreatureType"].ToString();
            creature.Subtype = reader["Subtype"].ToString();
            creature.Size = reader["Size"].ToString();
            creature.Alignment = reader["Alignment"].ToString();
            creature.Description = reader["Description"].ToString();

            creature.ArmorClass = Convert.ToInt32(reader["ArmorClass"]);
            creature.ArmorClassNotes = reader["ArmorClassNotes"].ToString();
            creature.HitPoints = Convert.ToInt32(reader["HitPoints"]);
            creature.HitDice = reader["HitDice"].ToString();
            creature.InitiativeModifier =
                Convert.ToInt32(reader["InitiativeModifier"]);
            creature.ChallengeRating = reader["ChallengeRating"].ToString();
            creature.ProficiencyBonus =
                Convert.ToInt32(reader["ProficiencyBonus"]);
            creature.ExperiencePoints =
                Convert.ToInt32(reader["ExperiencePoints"]);

            creature.Strength = Convert.ToInt32(reader["Strength"]);
            creature.Dexterity = Convert.ToInt32(reader["Dexterity"]);
            creature.Constitution = Convert.ToInt32(reader["Constitution"]);
            creature.Intelligence = Convert.ToInt32(reader["Intelligence"]);
            creature.Wisdom = Convert.ToInt32(reader["Wisdom"]);
            creature.Charisma = Convert.ToInt32(reader["Charisma"]);

            creature.WalkSpeed = Convert.ToInt32(reader["WalkSpeed"]);
            creature.FlySpeed = Convert.ToInt32(reader["FlySpeed"]);
            creature.SwimSpeed = Convert.ToInt32(reader["SwimSpeed"]);
            creature.ClimbSpeed = Convert.ToInt32(reader["ClimbSpeed"]);
            creature.BurrowSpeed = Convert.ToInt32(reader["BurrowSpeed"]);
            creature.SpecialMovement = reader["SpecialMovement"].ToString();

            creature.SavingThrows = reader["SavingThrows"].ToString();
            creature.Skills = reader["Skills"].ToString();
            creature.DamageVulnerabilities =
                reader["DamageVulnerabilities"].ToString();
            creature.DamageResistances =
                reader["DamageResistances"].ToString();
            creature.DamageImmunities =
                reader["DamageImmunities"].ToString();
            creature.ConditionImmunities =
                reader["ConditionImmunities"].ToString();

            creature.Senses = reader["Senses"].ToString();
            creature.PassivePerception =
                Convert.ToInt32(reader["PassivePerception"]);
            creature.Languages = reader["Languages"].ToString();

            creature.Traits = reader["Traits"].ToString();
            creature.LegendaryActions =
                reader["LegendaryActions"].ToString();
            creature.LairActions = reader["LairActions"].ToString();
            creature.Reactions = reader["Reactions"].ToString();
            creature.Spellcasting = reader["Spellcasting"].ToString();
            creature.Habitat = reader["Habitat"].ToString();
            creature.Behavior = reader["Behavior"].ToString();
            creature.Tactics = reader["Tactics"].ToString();
            creature.GMNotes = reader["GMNotes"].ToString();

            creature.CreatedDate = DateTime.Parse(
                reader["CreatedDate"].ToString(),
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind);

            creature.LastModified = DateTime.Parse(
                reader["LastModified"].ToString(),
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind);

            return creature;
        }

        private static CreatureAttack ReadAttack(
            SQLiteDataReader reader)
        {
            CreatureAttack attack = new CreatureAttack();

            attack.Id = Guid.Parse(reader["Id"].ToString());
            attack.CreatureId = Guid.Parse(reader["CreatureId"].ToString());
            attack.Name = reader["Name"].ToString();
            attack.AttackType = reader["AttackType"].ToString();
            attack.AttackBonus = Convert.ToInt32(reader["AttackBonus"]);
            attack.Reach = reader["Reach"].ToString();
            attack.Range = reader["Range"].ToString();
            attack.Target = reader["Target"].ToString();
            attack.DamageDice = reader["DamageDice"].ToString();
            attack.DamageType = reader["DamageType"].ToString();
            attack.AdditionalDamage = reader["AdditionalDamage"].ToString();
            attack.SavingThrow = reader["SavingThrow"].ToString();
            attack.Effect = reader["Effect"].ToString();
            attack.Recharge = reader["Recharge"].ToString();
            attack.Notes = reader["Notes"].ToString();
            attack.DisplayOrder = Convert.ToInt32(reader["DisplayOrder"]);

            return attack;
        }

        private static CreatureAbility ReadAbility(
            SQLiteDataReader reader)
        {
            CreatureAbility ability = new CreatureAbility();

            ability.Id = Guid.Parse(reader["Id"].ToString());
            ability.CreatureId = Guid.Parse(reader["CreatureId"].ToString());
            ability.Name = reader["Name"].ToString();
            ability.AbilityType = reader["AbilityType"].ToString();
            ability.Description = reader["Description"].ToString();
            ability.Activation = reader["Activation"].ToString();
            ability.Recharge = reader["Recharge"].ToString();
            ability.Uses = reader["Uses"].ToString();
            ability.SavingThrow = reader["SavingThrow"].ToString();
            ability.Effect = reader["Effect"].ToString();
            ability.DisplayOrder = Convert.ToInt32(reader["DisplayOrder"]);

            return ability;
        }

        private static void AddCreatureParameters(
            SQLiteCommand command, Creature creature)
        {
            command.Parameters.AddWithValue("@Id", creature.Id.ToString());
            command.Parameters.AddWithValue("@Name", creature.Name.Trim());
            command.Parameters.AddWithValue(
                "@CreatureType", creature.CreatureType ?? "Other");
            command.Parameters.AddWithValue("@Subtype", creature.Subtype ?? "");
            command.Parameters.AddWithValue("@Size", creature.Size ?? "Medium");
            command.Parameters.AddWithValue("@Alignment", creature.Alignment ?? "");
            command.Parameters.AddWithValue("@Description", creature.Description ?? "");

            command.Parameters.AddWithValue("@ArmorClass", creature.ArmorClass);
            command.Parameters.AddWithValue(
                "@ArmorClassNotes", creature.ArmorClassNotes ?? "");
            command.Parameters.AddWithValue("@HitPoints", creature.HitPoints);
            command.Parameters.AddWithValue("@HitDice", creature.HitDice ?? "");
            command.Parameters.AddWithValue(
                "@InitiativeModifier", creature.InitiativeModifier);
            command.Parameters.AddWithValue(
                "@ChallengeRating", creature.ChallengeRating ?? "0");
            command.Parameters.AddWithValue(
                "@ProficiencyBonus", creature.ProficiencyBonus);
            command.Parameters.AddWithValue(
                "@ExperiencePoints", creature.ExperiencePoints);

            command.Parameters.AddWithValue("@Strength", creature.Strength);
            command.Parameters.AddWithValue("@Dexterity", creature.Dexterity);
            command.Parameters.AddWithValue("@Constitution", creature.Constitution);
            command.Parameters.AddWithValue("@Intelligence", creature.Intelligence);
            command.Parameters.AddWithValue("@Wisdom", creature.Wisdom);
            command.Parameters.AddWithValue("@Charisma", creature.Charisma);

            command.Parameters.AddWithValue("@WalkSpeed", creature.WalkSpeed);
            command.Parameters.AddWithValue("@FlySpeed", creature.FlySpeed);
            command.Parameters.AddWithValue("@SwimSpeed", creature.SwimSpeed);
            command.Parameters.AddWithValue("@ClimbSpeed", creature.ClimbSpeed);
            command.Parameters.AddWithValue("@BurrowSpeed", creature.BurrowSpeed);
            command.Parameters.AddWithValue(
                "@SpecialMovement", creature.SpecialMovement ?? "");

            command.Parameters.AddWithValue(
                "@SavingThrows", creature.SavingThrows ?? "");
            command.Parameters.AddWithValue("@Skills", creature.Skills ?? "");
            command.Parameters.AddWithValue(
                "@DamageVulnerabilities", creature.DamageVulnerabilities ?? "");
            command.Parameters.AddWithValue(
                "@DamageResistances", creature.DamageResistances ?? "");
            command.Parameters.AddWithValue(
                "@DamageImmunities", creature.DamageImmunities ?? "");
            command.Parameters.AddWithValue(
                "@ConditionImmunities", creature.ConditionImmunities ?? "");

            command.Parameters.AddWithValue("@Senses", creature.Senses ?? "");
            command.Parameters.AddWithValue(
                "@PassivePerception", creature.PassivePerception);
            command.Parameters.AddWithValue("@Languages", creature.Languages ?? "");

            command.Parameters.AddWithValue("@Traits", creature.Traits ?? "");
            command.Parameters.AddWithValue(
                "@LegendaryActions", creature.LegendaryActions ?? "");
            command.Parameters.AddWithValue(
                "@LairActions", creature.LairActions ?? "");
            command.Parameters.AddWithValue("@Reactions", creature.Reactions ?? "");
            command.Parameters.AddWithValue(
                "@Spellcasting", creature.Spellcasting ?? "");
            command.Parameters.AddWithValue("@Habitat", creature.Habitat ?? "");
            command.Parameters.AddWithValue("@Behavior", creature.Behavior ?? "");
            command.Parameters.AddWithValue("@Tactics", creature.Tactics ?? "");
            command.Parameters.AddWithValue("@GMNotes", creature.GMNotes ?? "");

            command.Parameters.AddWithValue(
                "@CreatedDate",
                creature.CreatedDate.ToString(
                    "o", CultureInfo.InvariantCulture));

            command.Parameters.AddWithValue(
                "@LastModified",
                creature.LastModified.ToString(
                    "o", CultureInfo.InvariantCulture));
        }

        private static void AddAttackParameters(
            SQLiteCommand command, CreatureAttack attack)
        {
            command.Parameters.AddWithValue("@Id", attack.Id.ToString());
            command.Parameters.AddWithValue(
                "@CreatureId", attack.CreatureId.ToString());
            command.Parameters.AddWithValue("@Name", attack.Name.Trim());
            command.Parameters.AddWithValue("@AttackType", attack.AttackType ?? "");
            command.Parameters.AddWithValue("@AttackBonus", attack.AttackBonus);
            command.Parameters.AddWithValue("@Reach", attack.Reach ?? "");
            command.Parameters.AddWithValue("@Range", attack.Range ?? "");
            command.Parameters.AddWithValue("@Target", attack.Target ?? "");
            command.Parameters.AddWithValue("@DamageDice", attack.DamageDice ?? "");
            command.Parameters.AddWithValue("@DamageType", attack.DamageType ?? "");
            command.Parameters.AddWithValue(
                "@AdditionalDamage", attack.AdditionalDamage ?? "");
            command.Parameters.AddWithValue(
                "@SavingThrow", attack.SavingThrow ?? "");
            command.Parameters.AddWithValue("@Effect", attack.Effect ?? "");
            command.Parameters.AddWithValue("@Recharge", attack.Recharge ?? "");
            command.Parameters.AddWithValue("@Notes", attack.Notes ?? "");
            command.Parameters.AddWithValue(
                "@DisplayOrder", attack.DisplayOrder);
        }

        private static void AddAbilityParameters(
            SQLiteCommand command, CreatureAbility ability)
        {
            command.Parameters.AddWithValue("@Id", ability.Id.ToString());
            command.Parameters.AddWithValue(
                "@CreatureId", ability.CreatureId.ToString());
            command.Parameters.AddWithValue("@Name", ability.Name.Trim());
            command.Parameters.AddWithValue(
                "@AbilityType", ability.AbilityType ?? "");
            command.Parameters.AddWithValue(
                "@Description", ability.Description ?? "");
            command.Parameters.AddWithValue(
                "@Activation", ability.Activation ?? "");
            command.Parameters.AddWithValue(
                "@Recharge", ability.Recharge ?? "");
            command.Parameters.AddWithValue("@Uses", ability.Uses ?? "");
            command.Parameters.AddWithValue(
                "@SavingThrow", ability.SavingThrow ?? "");
            command.Parameters.AddWithValue("@Effect", ability.Effect ?? "");
            command.Parameters.AddWithValue(
                "@DisplayOrder", ability.DisplayOrder);
        }
    }
}
