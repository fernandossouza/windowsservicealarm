using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPI_Service_Alarm.Model
{
    public class ThingGroup
    {
        public int thingGroupId { get; set; }
        public List<int> thingsIds { get; set; }
    }
}
