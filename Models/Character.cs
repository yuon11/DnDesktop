using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public int Level { get; set; }

        public string Background { get; set; } = "";
        public string Alignment { get; set; } = "";
        public string Gender { get; set; } = "";
        public int Age { get; set; }

        // Physical
        public string Size { get; set; } = "";
        public double HeightFeet { get; set; }
        public double WeightLbs { get; set; }
        public string Eyes { get; set; } = "";
        public string Hair { get; set; } = "";
        public string Skin { get; set; } = "";

        // Character Stats
        public AbilityScores AbilityScores { get; set; }

        // Combat
        public CombatStats Combat { get; set; }

        // Skills
        public List<Skill> Skills { get; set; }

        // Saving Throws
        public List<SavingThrow> SavingThrows { get; set; }

        // Languages
        public List<string> Languages { get; set; }

        // Proficiencies
        public List<string> ArmorProficiencies { get; set; }
        public List<string> WeaponProficiencies { get; set; }
        public List<string> ToolProficiencies { get; set; }

        // Equipment
        public List<Item> Inventory { get; set; }

        // Currency
        public Currency Currency { get; set; }

        // Features
        public List<Feature> Features { get; set; }

        // Feats
        public List<Feat> Feats { get; set; }

        // Spells
        public Spellcasting Spellcasting { get; set; }

        // Personality
        public Personality Personality { get; set; }

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
        public int Strength { get; set; }
        public int Dexterity { get; set; }
        public int Constitution { get; set; }
        public int Intelligence { get; set; }
        public int Wisdom { get; set; }
        public int Charisma { get; set; }
    }

    public class CombatStats
    {
        public int ArmorClass { get; set; }
        public int Initiative { get; set; }

        public int MaxHitPoints { get; set; }
        public int CurrentHitPoints { get; set; }
        public int TemporaryHitPoints { get; set; }

        public int Speed { get; set; }
        public int HitDice { get; set; }

        public int PassivePerception { get; set; }
        public int PassiveInvestigation { get; set; }
        public int PassiveInsight { get; set; }

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