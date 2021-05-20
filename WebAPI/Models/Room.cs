using System;
using System.Collections.Generic;
using WebAPI.ViewModels;

namespace WebAPI.Models
{
    public class Room
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public float Temperature { get; set; }
        public float Humidity { get; set; }
        public float TargetTemperature { get; set; }
        public float TargetHumidity { get; set; }
        public DateTime PreviousUpdate { get; set; }

        public List<Change> Changes { get; set; }
        public List<Device> Devices { get; set; }
    }
}
