
using DnDesktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace DnDesktop.Data
{
    public class CombatParticipantRepository
    {
        private readonly CampaignMembershipRepository
            _membershipRepository =
                new CampaignMembershipRepository();

        private readonly CreatureRepository
            _creatureRepository =
                new CreatureRepository();

        public List<EncounterCombatant> GetAvailableParticipants(
            Guid campaignId,
            Guid encounterId)
        {
            if (campaignId == Guid.Empty)
            {
                throw new ArgumentException(
                    "A valid campaign ID is required.",
                    nameof(campaignId));
            }

            if (encounterId == Guid.Empty)
            {
                throw new ArgumentException(
                    "A valid encounter ID is required.",
                    nameof(encounterId));
            }

            var participants = new List<EncounterCombatant>();

            // Load the campaign's player characters.
            var partyMembers =
                _membershipRepository.GetPartyMembers(campaignId);

            foreach (var character in partyMembers)
            {
                var combat = character.Combat;

                participants.Add(new EncounterCombatant
                {
                    EncounterId = encounterId,
                    ParticipantType = "Character",
                    SourceId = character.Id,
                    DisplayName = character.Name,

                    Initiative = combat?.Initiative ?? 0,
                    CurrentHP = Math.Max(
                        0, combat?.CurrentHitPoints ?? 0),
                    MaxHP = Math.Max(
                        0, combat?.MaxHitPoints ?? 0),
                    ArmorClass = combat?.ArmorClass ?? 10
                });
            }

            // Load the campaign's NPCs.
            // Load the campaign's NPCs.
            var npcs = _membershipRepository.GetNPCs(campaignId);

            foreach (var npc in npcs)
            {
                var combat = npc.Combat;

                participants.Add(new EncounterCombatant
                {
                    EncounterId = encounterId,
                    ParticipantType = "NPC",
                    SourceId = npc.Id,
                    DisplayName = npc.Name,

                    Initiative = combat?.Initiative ?? 0,
                    CurrentHP = Math.Max(
                        0, combat?.CurrentHitPoints ?? 0),
                    MaxHP = Math.Max(
                        0, combat?.MaxHitPoints ?? 0),
                    ArmorClass = combat?.ArmorClass ?? 10
                });
            }

            // Load the creature library.
            var creatures = _creatureRepository.GetAll();

            foreach (var creature in creatures)
            {
                participants.Add(new EncounterCombatant
                {
                    EncounterId = encounterId,
                    ParticipantType = "Creature",
                    SourceId = creature.Id,
                    DisplayName = creature.Name,
                    MaxHP = Math.Max(0, creature.HitPoints),
                    CurrentHP = Math.Max(0, creature.HitPoints),
                    ArmorClass = creature.ArmorClass
                });
            }

            //MessageBox.Show(
            //    "Available participants loaded: " 
            //    + participants.Count +
            //    "\nCampaign ID: " + campaignId,
            //    "Combat Participant Debug");

            return participants
                .OrderBy(p => p.ParticipantType)
                .ThenBy(p => p.DisplayName)
                .ToList();
        }
    }
}
