
using System;

namespace DnDesktop.Models
{
    public class CharacterImage
    {
        public Guid Id { get; set; }
        public Guid CharacterId { get; set; }

        // Path to the image file stored on disk.
        public string ImagePath { get; set; }

        public string Caption { get; set; }
        public string ImageCategory { get; set; }
        public bool IsPrimary { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedDate { get; set; }

        public CharacterImage()
        {
            Id = Guid.NewGuid();
            ImagePath = string.Empty;
            Caption = string.Empty;
            ImageCategory = "Other";
            IsPrimary = false;
            DisplayOrder = 0;
            CreatedDate = DateTime.Now;
        }
    }
}
