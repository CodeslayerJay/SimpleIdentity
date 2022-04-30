using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.Models
{
    public class TokenRequestApiModel
    {
        public DateTime ExpiresAt { get; set; }
        [Required]
        public string RefreshKey { get; set; }
        [Required]
        public string AccessKey { get; set; }

        public string ClientId { get; set; }
    }
}
