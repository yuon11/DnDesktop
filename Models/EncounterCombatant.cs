
using System;

namespace DnDesktop.Models
{
    public class EncounterCombatant
    {
        public Guid Id { get; set; }

        public Guid EncounterId { get; set; }

        // "Character", "NPC", or "Creature"
        public string ParticipantType { get; set; }

        // ID of the existing Character or Creature record.
        public Guid SourceId { get; set; }

        // Name displayed in this encounter.
        public string DisplayName { get; set; }

        public int Initiative { get; set; }

        public int CurrentHP { get; set; }

        public int MaxHP { get; set; }

        public int ArmorClass { get; set; }

        // Conditions such as Poisoned, Prone, or Stunned.
        public string Conditions { get; set; }

        public bool IsDefeated { get; set; }

        // Used to maintain turn order when initiative ties.
        public int TurnOrder { get; set; }

        public EncounterCombatant()
        {
            Id = Guid.NewGuid();

            ParticipantType = "Creature";
            DisplayName = string.Empty;
            Conditions = string.Empty;

            Initiative = 0;
            CurrentHP = 0;
            MaxHP = 0;
            ArmorClass = 10;

            IsDefeated = false;
            TurnOrder = 0;
        }
    }
}
