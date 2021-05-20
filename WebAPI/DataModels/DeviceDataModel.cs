using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.DataModels
{
    public class DeviceDataModel
    {
        [Required(ErrorMessage = "Введите название")]
        public string Name { get; set; }
    }
}
