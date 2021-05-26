using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Models;

namespace WebAPI.ViewModels
{
    public class ChangeViewModel
    {
        public int ID { get; set; }
        public int RoomID { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }

        public ChangeViewModel(Change change)
        {
            ID = change.ID;
            RoomID = change.RoomID;
            Message = change.Message;
            Date = change.Date;
        }
    }
}
