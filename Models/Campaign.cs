
using System;

namespace DnDesktop.Models
{
    public class Campaign
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string CampaignNotes { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime LastModified { get; set; }

        public Campaign()
        {
            Id = Guid.NewGuid();

            Name = string.Empty;
            Description = string.Empty;
            CampaignNotes = string.Empty;

            CreatedDate = DateTime.Now;
            LastModified = DateTime.Now;
        }
    }
}
