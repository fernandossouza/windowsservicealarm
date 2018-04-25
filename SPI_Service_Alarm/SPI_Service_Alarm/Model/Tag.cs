using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPI_Service_Alarm.Model
{
    public class Tag
    {
        public int tagId { get; set; }
        public string tagName { get; set; }
        public string physicalTag { get; set; }
        public string tagType { get; set; }
        public int thingGroupId { get; set; }
    }
}
