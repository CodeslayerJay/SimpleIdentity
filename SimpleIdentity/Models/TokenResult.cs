using Identity.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.Models
{
    public class TokenResult
    {
        public TokenResult()
        {
            Errors = new List<string>();
        }

        public JsonWebToken AccessToken { get; set; }
        public RefreshToken RefreshToken { get; set; }
        public List<string> Errors { get; set; }
        public bool Success => Errors.Any() == false;
        public bool NotAuthorized { get; set; }
    }
}
