
using System;

namespace DnDesktop.Models
{
    public class CreatureAbility
    {
        public Guid Id { get; set; }
        public Guid CreatureId { get; set; }

        public string Name { get; set; }
        public string AbilityType { get; set; }
        public string Description { get; set; }

        public string Activation { get; set; }
        public string Recharge { get; set; }
        public string Uses { get; set; }

        public string SavingThrow { get; set; }
        public string Effect { get; set; }

        public int DisplayOrder { get; set; }

        public CreatureAbility()
        {
            Id = Guid.NewGuid();

            Name = string.Empty;
            AbilityType = "Trait";
            Description = string.Empty;

            Activation = string.Empty;
            Recharge = string.Empty;
            Uses = string.Empty;

            SavingThrow = string.Empty;
            Effect = string.Empty;

            DisplayOrder = 0;
        }
    }
}
