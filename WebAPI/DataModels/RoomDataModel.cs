using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.DataModels
{
    public class RoomDataModel
    {
        [Required(ErrorMessage = "Введите название")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Введите код доступа")]
        public string Code { get; set; }
    }
}
