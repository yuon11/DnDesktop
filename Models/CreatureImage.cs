
using System;

namespace DnDesktop.Models
{
    public class CreatureImage
    {
        public Guid Id { get; set; }

        public Guid CreatureId { get; set; }

        public string ImagePath { get; set; }

        public string Caption { get; set; }

        public string ImageCategory { get; set; }
        public string ImageDescription { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsPrimary { get; set; }

        public DateTime CreatedDate { get; set; }

        public CreatureImage()
        {
            Id = Guid.NewGuid();
            CreatureId = Guid.Empty;

            ImagePath = string.Empty;
            Caption = string.Empty;
            ImageCategory = "Other";

            IsPrimary = false;
            CreatedDate = DateTime.Now;
        }
    }
}
