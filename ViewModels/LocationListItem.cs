using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DnDesktop.Models;

namespace DnDesktop.ViewModels
{
    public class LocationListItem
    {
        public Location Location { get; set; }

        public Guid Id
        {
            get { return Location.Id; }
        }

        public string Name
        {
            get { return Location.Name; }
        }

        public string LocationType
        {
            get { return Location.LocationType; }
        }

        public string Description
        {
            get { return Location.Description; }
        }

        public string ParentLocationName { get; set; }

        public int ImageCount { get; set; }

        public string PrimaryImagePath { get; set; }
    }
}