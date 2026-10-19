
using System;

namespace DnDesktop.Models
{
    public class SessionEncounterCreature
    {
        public Guid EncounterId { get; set; }
        public Guid CreatureId { get; set; }

        public int Quantity { get; set; }

        // Display-only property populated from the existing Creature record.
        public string CreatureName { get; set; }

        public SessionEncounterCreature()
        {
            Quantity = 1;
            CreatureName = string.Empty;
        }
    }
}
