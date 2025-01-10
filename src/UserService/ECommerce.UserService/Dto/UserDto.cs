using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.UserService.Dto
{
    public class UserDto
    {
        [Required(ErrorMessage="Kullanıcı Adı Zorunlu")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage="Şifre Zorunlu")]
        public string Password { get; set; } = null;
    }
}