using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.DataModels
{
    public class RegistrationDataModel
    {
        [Required(ErrorMessage = "Введите email")]
        [EmailAddress(ErrorMessage = "Не является email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        [MinLength(8,ErrorMessage = "Пароль должен содержать минимум 8 символов")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Повторите пароль")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Введите фамилию")]
        public string Surname { get; set; }

        [Required(ErrorMessage = "Введите имя")]
        public string Name { get; set; }

        public string Patronymic { get; set; }
    }
}
