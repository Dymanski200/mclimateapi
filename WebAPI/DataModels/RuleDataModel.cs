using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.DataModels
{
    public class RuleDataModel
    {
        [Required(ErrorMessage = "Укажите устройство")]
        public int DeviceID { get; set; }
        [Required(ErrorMessage = "Укажите параметр")]
        public bool Temperature { get; set; }
        [Required(ErrorMessage = "Укажите отклонение")]
        public float Offset { get; set; }
        [Required(ErrorMessage = "Укажите команду")]
        public string Command { get; set; }
        [Required(ErrorMessage = "Укажите статус")]
        public string Status { get; set; }
    }
}
