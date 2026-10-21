
using System;

namespace DnDesktop.Models
{
    public class Creature
    {
        public Guid Id { get; set; }

        // Display-only path for the creature card's primary image.
        public string PrimaryImagePath { get; set; } = "";

        // Identity
        public string Name { get; set; }
        public string CreatureType { get; set; }
        public string Subtype { get; set; }
        public string Size { get; set; }
        public string Alignment { get; set; }
        public string Description { get; set; }

        // Combat statistics
        public int ArmorClass { get; set; }
        public string ArmorClassNotes { get; set; }
        public int HitPoints { get; set; }
        public string HitDice { get; set; }
        public int InitiativeModifier { get; set; }
        public string ChallengeRating { get; set; }
        public int ProficiencyBonus { get; set; }
        public int ExperiencePoints { get; set; }

        // Ability scores
        public int Strength { get; set; }
        public int Dexterity { get; set; }
        public int Constitution { get; set; }
        public int Intelligence { get; set; }
        public int Wisdom { get; set; }
        public int Charisma { get; set; }

        // Movement speeds, in feet
        public int WalkSpeed { get; set; }
        public int FlySpeed { get; set; }
        public int SwimSpeed { get; set; }
        public int ClimbSpeed { get; set; }
        public int BurrowSpeed { get; set; }
        public string SpecialMovement { get; set; }

        // Saving throws, skills, and defenses
        public string SavingThrows { get; set; }
        public string Skills { get; set; }
        public string DamageVulnerabilities { get; set; }
        public string DamageResistances { get; set; }
        public string DamageImmunities { get; set; }
        public string ConditionImmunities { get; set; }

        // Senses and communication
        public string Senses { get; set; }
        public int PassivePerception { get; set; }
        public string Languages { get; set; }

        // Additional creature information
        public string Traits { get; set; }
        public string LegendaryActions { get; set; }
        public string LairActions { get; set; }
        public string Reactions { get; set; }
        public string Spellcasting { get; set; }
        public string Habitat { get; set; }
        public string Behavior { get; set; }
        public string Tactics { get; set; }
        public string GMNotes { get; set; }

        // Record timestamps
        public DateTime CreatedDate { get; set; }
        public DateTime LastModified { get; set; }

        public Creature()
        {
            Id = Guid.NewGuid();

            Name = string.Empty;
            CreatureType = "Other";
            Subtype = string.Empty;
            Size = "Medium";
            Alignment = "Unaligned";
            Description = string.Empty;

            ArmorClass = 10;
            ArmorClassNotes = string.Empty;
            HitPoints = 1;
            HitDice = string.Empty;
            InitiativeModifier = 0;
            ChallengeRating = "0";
            ProficiencyBonus = 2;
            ExperiencePoints = 0;

            Strength = 10;
            Dexterity = 10;
            Constitution = 10;
            Intelligence = 10;
            Wisdom = 10;
            Charisma = 10;

            WalkSpeed = 30;
            FlySpeed = 0;
            SwimSpeed = 0;
            ClimbSpeed = 0;
            BurrowSpeed = 0;
            SpecialMovement = string.Empty;

            SavingThrows = string.Empty;
            Skills = string.Empty;
            DamageVulnerabilities = string.Empty;
            DamageResistances = string.Empty;
            DamageImmunities = string.Empty;
            ConditionImmunities = string.Empty;

            Senses = string.Empty;
            PassivePerception = 10;
            Languages = string.Empty;

            Traits = string.Empty;
            LegendaryActions = string.Empty;
            LairActions = string.Empty;
            Reactions = string.Empty;
            Spellcasting = string.Empty;
            Habitat = string.Empty;
            Behavior = string.Empty;
            Tactics = string.Empty;
            GMNotes = string.Empty;

            CreatedDate = DateTime.Now;
            LastModified = DateTime.Now;
        }

        // Standard D&D-style ability modifiers.
        // Custom rules can override these calculations later if needed.
        public int GetAbilityModifier(int score)
        {
            return (int)Math.Floor((score - 10) / 2.0);
        }

        public int StrengthModifier
        {
            get { return GetAbilityModifier(Strength); }
        }

        public int DexterityModifier
        {
            get { return GetAbilityModifier(Dexterity); }
        }

        public int ConstitutionModifier
        {
            get { return GetAbilityModifier(Constitution); }
        }

        public int IntelligenceModifier
        {
            get { return GetAbilityModifier(Intelligence); }
        }

        public int WisdomModifier
        {
            get { return GetAbilityModifier(Wisdom); }
        }

        public int CharismaModifier
        {
            get { return GetAbilityModifier(Charisma); }
        }
    }
}
