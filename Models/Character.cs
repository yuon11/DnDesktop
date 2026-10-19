using System;
using System.Collections.Generic;

namespace DnDesktop.Models
{
    public class Character : CampaignEntity
    {
        // Unique ID
        public Guid Id { get; set; } = Guid.NewGuid();

        // Basic Info
        public string Name { get; set; } = "";
        public string PlayerName { get; set; } = "";
        public string Race { get; set; } = "";
        public string Subrace { get; set; } = "";
        public string CharacterClass { get; set; } = "";
        public string Subclass { get; set; } = "";
        public int Level { get; set; } = 1;

        public string Background { get; set; } = "";
        public string Alignment { get; set; } = "";
        public string Gender { get; set; } = "";
        public int Age { get; set; }

        // Physical
        public string Size { get; set; } = "Medium";
        public double HeightFeet { get; set; }
        public double WeightLbs { get; set; }
        public string Eyes { get; set; } = "";
        public string Hair { get; set; } = "";
        public string Skin { get; set; } = "";

        // Ability Scores
        public AbilityScores AbilityScores { get; set; }
            = new AbilityScores();

        // Combat Statistics
        public CombatStats Combat { get; set; }
            = new CombatStats();

        // Skills
        public List<Skill> Skills { get; set; }
            = new List<Skill>();

        // Saving Throws
        public List<SavingThrow> SavingThrows { get; set; }
            = new List<SavingThrow>();

        // Languages
        public List<string> Languages { get; set; }
            = new List<string>();

        // Proficiencies
        public List<string> ArmorProficiencies { get; set; }
            = new List<string>();

        public List<string> WeaponProficiencies { get; set; }
            = new List<string>();

        public List<string> ToolProficiencies { get; set; }
            = new List<string>();

        // Equipment
        public List<Item> Inventory { get; set; }
            = new List<Item>();

        // Currency
        public Currency Currency { get; set; }
            = new Currency();

        // Features
        public List<Feature> Features { get; set; }
            = new List<Feature>();

        // Feats
        public List<Feat> Feats { get; set; }
            = new List<Feat>();

        // Spells
        public Spellcasting Spellcasting { get; set; }
            = new Spellcasting();

        // Personality
        public Personality Personality { get; set; }
            = new Personality();

        // Notes
        public string Notes { get; set; } = "";

        // Campaign Information
        public string CampaignName { get; set; } = "";
        public string CurrentLocation { get; set; } = "";
        public string Faction { get; set; } = "";

        // Portrait
        public string PortraitPath { get; set; } = "";

        // Metadata
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime LastModified { get; set; } = DateTime.Now;
    }

    public class AbilityScores
    {
        public int Strength { get; set; } = 10;
        public int Dexterity { get; set; } = 10;
        public int Constitution { get; set; } = 10;
        public int Intelligence { get; set; } = 10;
        public int Wisdom { get; set; } = 10;
        public int Charisma { get; set; } = 10;
    }

    public class CombatStats
    {
        public int ArmorClass { get; set; } = 10;
        public int Initiative { get; set; }

        public int MaxHitPoints { get; set; } = 10;
        public int CurrentHitPoints { get; set; } = 10;
        public int TemporaryHitPoints { get; set; }

        public int Speed { get; set; } = 30;

        // Number of hit dice, e.g. 1 for a level-one character.
        public int HitDice { get; set; } = 1;

        public int PassivePerception { get; set; } = 10;
        public int PassiveInvestigation { get; set; } = 10;
        public int PassiveInsight { get; set; } = 10;

        public int DeathSaveSuccesses { get; set; }
        public int DeathSaveFailures { get; set; }
    }

    public class Skill
    {
        public string Name { get; set; } = "";
        public bool Proficient { get; set; }
        public bool Expertise { get; set; }
        public int Modifier { get; set; }
    }

    public class SavingThrow
    {
        public string Ability { get; set; } = "";
        public bool Proficient { get; set; }
        public int Modifier { get; set; }
    }

    public class Item
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public int Quantity { get; set; }
        public double Weight { get; set; }
        public string Description { get; set; } = "";
        public bool Equipped { get; set; }
    }

    public class Currency
    {
        public int Copper { get; set; }
        public int Silver { get; set; }
        public int Electrum { get; set; }
        public int Gold { get; set; }
        public int Platinum { get; set; }
    }

    public class Feature
    {
        public string Name { get; set; } = "";
        public string Source { get; set; } = "";
        public string Description { get; set; } = "";
    }

    public class Feat
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
    }

    public class Spellcasting
    {
        public string SpellcastingAbility { get; set; } = "";
        public int SpellSaveDC { get; set; }
        public int SpellAttackBonus { get; set; }

        public List<Spell> KnownSpells { get; set; }
            = new List<Spell>();
    }

    public class Spell
    {
        public string Name { get; set; } = "";
        public int Level { get; set; }
        public string School { get; set; } = "";
        public bool Prepared { get; set; }
        public bool Ritual { get; set; }
        public string Description { get; set; } = "";
    }

    public class Personality
    {
        public string PersonalityTraits { get; set; } = "";
        public string Ideals { get; set; } = "";
        public string Bonds { get; set; } = "";
        public string Flaws { get; set; } = "";
        public string Backstory { get; set; } = "";
    }
}
