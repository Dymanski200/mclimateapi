using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Models;

namespace WebAPI.ViewModels
{
    public class RoomViewModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public float Temperature { get; set; }
        public float Humidity { get; set; }
        public float TargetTemperature { get; set; }
        public float TargetHumidity { get; set; }
        public DateTime PreviousUpdate { get; set; }

        public RoomViewModel(Room room)
        {
            ID = room.ID;
            Name = room.Name;
            Temperature = room.Temperature;
            Humidity = room.Humidity;
            TargetTemperature = room.TargetTemperature;
            TargetHumidity = room.TargetHumidity;
            PreviousUpdate = room.PreviousUpdate;
        }
    }
}
