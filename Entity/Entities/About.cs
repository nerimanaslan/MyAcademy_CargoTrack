using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoTrack.Entity.Entities.Common;

namespace CargoTrack.Entity.Entities
{
    public class About:BaseEntity
    {



        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
    }
}
