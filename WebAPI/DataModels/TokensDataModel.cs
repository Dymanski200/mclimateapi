using System.ComponentModel.DataAnnotations;

namespace WebAPI.DataModels
{
    public class TokensDataModel
    {
        [Required(ErrorMessage = "Отсутствует токен")]
        public string AccessToken { get; set; }
        [Required(ErrorMessage = "Отсутствует токен")]
        public string RefreshToken { get; set; }
    }
}
