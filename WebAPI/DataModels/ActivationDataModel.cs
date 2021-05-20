using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.DataModels
{
    public class ActivationDataModel
    {
        [Required(ErrorMessage = "Введите email")]
        [EmailAddress(ErrorMessage = "Не является email")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Введите код")]
        public string Code { get; set; }
    }
}
