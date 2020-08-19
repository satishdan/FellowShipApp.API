using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FellowShipApp.API.Dtos
{
    public class UserForLogin
    {
       // [Required]
        public string Username { get; set; }

        //[Required]
        //[StringLength(8, MinimumLength = 4, ErrorMessage = "You must specify password between 4 and 8 chars")]
        public string Password { get; set; }
    }
}
