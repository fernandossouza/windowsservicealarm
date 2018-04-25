using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPI_Service_Alarm.Model
{
    public class Alarm
    {
        public int alarmId { get; set; }
        public int thingId { get; set; }
        public string alarmDescription { get; set; }
        public string alarmName { get; set; }
        public string alarmColor { get; set; }
        public int priority { get; set; }
        public long datetime { get; set; }
        public string tagIL { get; set; }
    }
}
