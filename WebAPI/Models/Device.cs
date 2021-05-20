using System.Collections.Generic;

namespace WebAPI.Models
{
    public class Device
    {
        public int ID { get; set; }
        public int RoomID { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }

        public Room Room { get; set; }
        public List<Rule> Rules { get; set; }
    }
}
