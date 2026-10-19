
using System;

namespace DnDesktop.Models
{
    public class CreatureAttack
    {
        public Guid Id { get; set; }
        public Guid CreatureId { get; set; }

        public string Name { get; set; }
        public string AttackType { get; set; }

        public int AttackBonus { get; set; }

        public string Reach { get; set; }
        public string Range { get; set; }
        public string Target { get; set; }

        public string DamageDice { get; set; }
        public string DamageType { get; set; }
        public string AdditionalDamage { get; set; }

        public string SavingThrow { get; set; }
        public string Effect { get; set; }
        public string Recharge { get; set; }
        public string Notes { get; set; }

        public int DisplayOrder { get; set; }

        public CreatureAttack()
        {
            Id = Guid.NewGuid();

            Name = string.Empty;
            AttackType = "Melee";
            Reach = "5 ft.";
            Range = string.Empty;
            Target = "One target";

            DamageDice = string.Empty;
            DamageType = string.Empty;
            AdditionalDamage = string.Empty;

            SavingThrow = string.Empty;
            Effect = string.Empty;
            Recharge = string.Empty;
            Notes = string.Empty;

            DisplayOrder = 0;
        }
    }
}
