using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    public class Rule
    {
        public int ID { get; set; }
        public int DeviceID { get; set; }
        public bool Temperature { get; set; }
        public float Offset { get; set; }
        public string Command { get; set; }
        public string Status { get; set; }

        public Device Device { get; set; }
    }
}
