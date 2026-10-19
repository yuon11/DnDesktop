
using System;

namespace DnDesktop.Models
{
    public class Quest
    {
        public Guid Id { get; set; }

        public Guid CampaignId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        // Optional reference to a character who gives the quest.
        public Guid? QuestGiverId { get; set; }

        public string Status { get; set; }
        public string Notes { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime LastModified { get; set; }

        public Quest()
        {
            Id = Guid.NewGuid();

            Name = string.Empty;
            Description = string.Empty;
            Status = "Not Started";
            Notes = string.Empty;

            CreatedDate = DateTime.Now;
            LastModified = DateTime.Now;
        }
    }
}
