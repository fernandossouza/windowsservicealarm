using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPI_Service_Alarm.Model
{
    public class ThingAlarm
    {
        public int thingId { get; set; }
        public List<Alarm> alarms { get; set; }        

    }
}
