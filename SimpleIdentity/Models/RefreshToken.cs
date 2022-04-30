using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string ClientId { get; set; }
        public string UserValue { get; set; }
        public DateTime IssuedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string RefreshKey { get; set; }
    }
}
