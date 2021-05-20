using System;

namespace WebAPI.Models
{
    public class Change
    {
        public int ID { get; set; }
        public int RoomID { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }

        public Room Room { get; set; }
    }
}
