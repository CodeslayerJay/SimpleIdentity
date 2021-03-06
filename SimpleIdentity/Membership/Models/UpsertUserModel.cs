using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.Membership.Models
{
    public class UpsertUserModel
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }

        public string PasswordConfirm { get; set; }

        public string Email { get; set; }
    }
}
