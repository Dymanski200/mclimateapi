using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.DataModels
{
    public class ClimateDataModel
    {
        [Required(ErrorMessage = "Укажите температуру")]
        public float Temperature { get; set; }
        [Required(ErrorMessage = "Укажите влажность")]
        public float Humidity { get; set; }
    }
}
