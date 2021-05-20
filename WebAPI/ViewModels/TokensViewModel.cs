using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.ViewModels
{
    public class TokensViewModel
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
