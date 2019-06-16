using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Spa.Models
{
    public class UserAccessModel
    {
        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string ServerUrl { get; set; }

        public string RedirectUrl { get; set; }
    }
}
