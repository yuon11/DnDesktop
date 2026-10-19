using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnDesktop.Models
{
    public class LocationImage
    {
        public Guid Id { get; set; }
        public Guid LocationId { get; set; }

        // Path to the image file stored on disk.
        public string ImagePath { get; set; }

        public string Caption { get; set; }
        public string ImageCategory { get; set; }
        public bool IsPrimary { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedDate { get; set; }

        public LocationImage()
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
