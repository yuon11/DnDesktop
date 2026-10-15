using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnDesktop
{
    public abstract class CampaignEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = "";

        public string Notes { get; set; } = "";

        public DateTime Created { get; set; } = DateTime.Now;

        public DateTime LastModified { get; set; } = DateTime.Now;
    }

}
