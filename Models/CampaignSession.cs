
using System;

namespace DnDesktop.Models
{
    public class CampaignSession
    {
        public Guid Id { get; set; }

        public Guid CampaignId { get; set; }

        public string Title { get; set; }

        public DateTime? SessionDate { get; set; }

        public string Status { get; set; }

        public string Summary { get; set; }

        public string SessionNotes { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime LastModified { get; set; }

        public CampaignSession()
        {
            Id = Guid.NewGuid();

            Title = string.Empty;
            Status = "Planned";
            Summary = string.Empty;
            SessionNotes = string.Empty;

            CreatedDate = DateTime.Now;
            LastModified = DateTime.Now;
        }
    }
}
