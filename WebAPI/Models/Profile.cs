using System;
using System.Collections.Generic;
namespace WebAPI.Models
{
    public class Profile
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public string Name { get; set; }
        public float TargetTemperature { get; set; }
        public float TargetHumidity { get; set; }

        public User User { get; set; }
    }
}
