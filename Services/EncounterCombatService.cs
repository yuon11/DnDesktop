
using DnDesktop.Models;
using DnDesktop.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DnDesktop.Services
{
    public class EncounterCombatService
    {
        private readonly CombatParticipantRepository
            _participantRepository;

        private readonly EncounterCombatantRepository
            _combatantRepository;

        public EncounterCombatService()
        {
            _participantRepository =
                new CombatParticipantRepository();

            _combatantRepository =
                new EncounterCombatantRepository();
        }

        public List<EncounterCombatant> GetAvailableParticipants(
            Guid campaignId,
            Guid encounterId)
        {
            var available = _participantRepository
                .GetAvailableParticipants(campaignId, encounterId);

            var existing = _combatantRepository
                .GetByEncounterId(encounterId);

            // Characters and NPCs can only be added once.
            // Creature definitions can be added multiple times.
            var existingUniqueParticipants = new HashSet<string>(
                existing
                    .Where(c => c.ParticipantType != "Creature")
                    .Select(c => GetParticipantKey(
                        c.ParticipantType,
                        c.SourceId)));

            return available
                .Where(p =>
                    p.ParticipantType == "Creature" ||
                    !existingUniqueParticipants.Contains(
                        GetParticipantKey(
                            p.ParticipantType,
                            p.SourceId)))
                .OrderBy(p => p.ParticipantType)
                .ThenBy(p => p.DisplayName)
                .ToList();
        }

        public List<EncounterCombatant> GetCombatants(
            Guid encounterId)
        {
            if (encounterId == Guid.Empty)
            {
                throw new ArgumentException(
                    "A valid encounter ID is required.",
                    nameof(encounterId));
            }

            return _combatantRepository.GetByEncounterId(
                encounterId);
        }

        public void AddParticipant(
            EncounterCombatant participant)
        {
            if (participant == null)
            {
                throw new ArgumentNullException(
                    nameof(participant));
            }

            var existing =
                _combatantRepository.GetByEncounterId(
                    participant.EncounterId);

            bool alreadyAdded =
                participant.ParticipantType != "Creature" &&
                existing.Any(c =>
                    c.ParticipantType == participant.ParticipantType &&
                    c.SourceId == participant.SourceId);

            if (alreadyAdded)
            {
                throw new InvalidOperationException(
                    "This character or NPC is already in the encounter.");
            }

            participant.Id = Guid.NewGuid();

            participant.TurnOrder = existing.Count == 0
                ? 0
                : existing.Max(c => c.TurnOrder) + 1;

            _combatantRepository.Add(participant);
        }

        public void AddParticipants(
            Guid encounterId,
            IEnumerable<EncounterCombatant> participants)
        {
            if (encounterId == Guid.Empty)
            {
                throw new ArgumentException(
                    "A valid encounter ID is required.",
                    nameof(encounterId));
            }

            if (participants == null)
            {
                throw new ArgumentNullException(
                    nameof(participants));
            }

            foreach (var participant in participants)
            {
                if (participant == null)
                {
                    throw new ArgumentException(
                        "The participant list cannot contain null entries.",
                        nameof(participants));
                }

                participant.EncounterId = encounterId;
                AddParticipant(participant);
            }
        }

        public void RemoveCombatant(Guid combatantId)
        {
            if (combatantId == Guid.Empty)
            {
                throw new ArgumentException(
                    "A valid combatant ID is required.",
                    nameof(combatantId));
            }

            _combatantRepository.Delete(combatantId);
        }

        private static string GetParticipantKey(
            string participantType,
            Guid sourceId)
        {
            return participantType + ":" + sourceId.ToString("D");
        }
    }
}
