
using System;

namespace DnDesktop.Models
{
    public class SessionEncounter
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }

        public string Name { get; set; }
        public string EncounterNotes { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime LastModified { get; set; }

        public SessionEncounter()
        {
            Id = Guid.NewGuid();
            Name = string.Empty;
            EncounterNotes = string.Empty;
            CreatedDate = DateTime.Now;
            LastModified = DateTime.Now;
        }
    }
}
