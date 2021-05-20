using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Models;

namespace WebAPI.ViewModels
{
    public class UserViewModel
    {
        public int ID { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronymic { get; set; }
        public string Role { get; set; }
        public DateTime RegistrationDate { get; set; }

        public UserViewModel(User user)
        {
            ID = user.ID;
            Surname = user.Surname;
            Name = user.Name;
            Patronymic = user.Patronymic;
            Role = user.Role;
            RegistrationDate = user.RegistrationDate;
        }

    }
}
