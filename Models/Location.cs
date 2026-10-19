
using System;

namespace DnDesktop.Models
{
    public class Location
    {
        public Guid Id { get; set; }

        // Basic information
        public string Name { get; set; }
        public string LocationType { get; set; }
        public Guid? ParentLocationId { get; set; }
        public string Description { get; set; }

        // World-building details
        public string Population { get; set; }
        public string Government { get; set; }
        public string NotableNPCs { get; set; }
        public string PointsOfInterest { get; set; }
        public string GMNotes { get; set; }

        // Audit information
        public DateTime CreatedDate { get; set; }
        public DateTime LastModified { get; set; }

        public Location()
        {
            Id = Guid.NewGuid();
            Name = string.Empty;
            LocationType = "Other";
            Description = string.Empty;
            Population = string.Empty;
            Government = string.Empty;
            NotableNPCs = string.Empty;
            PointsOfInterest = string.Empty;
            GMNotes = string.Empty;

            CreatedDate = DateTime.Now;
            LastModified = DateTime.Now;
        }
    }
}
