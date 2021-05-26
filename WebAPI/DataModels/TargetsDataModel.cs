using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.DataModels
{
    public class TargetsDataModel
    {
        [Required(ErrorMessage = "Укажите код доступа")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Укажите температуру")]
        public float TargetTemperature { get; set; }

        [Required(ErrorMessage = "Укажите влажность")]
        public float TargetHumidity { get; set; }
    }
}
