using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Models;

namespace WebAPI.ViewModels
{
    public class DeviceViewModel
    {
        public int ID { get; set; }
        public int RoomID { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }

        public DeviceViewModel(Device device)
        {
            ID = device.ID;
            RoomID = device.RoomID;
            Name = device.Name;
            Status = device.Status;
        }
    }
}
