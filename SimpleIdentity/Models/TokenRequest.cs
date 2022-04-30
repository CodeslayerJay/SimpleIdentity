using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.Models
{
    public class TokenRequest
    {
        public string Identifier { get; set; }
        public string UserValue { get; set; }
        public string ClientId { get; set; }
        public string RefreshToken { get; set; }
        public string AccessToken { get; set; }
    }
}
