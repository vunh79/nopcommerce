using Newtonsoft.Json;
using System.Collections.Generic;

namespace Nop.Spa.DTOs
{
    public class CustomersRootObject
    {
        [JsonProperty("customers")]
        public List<CustomerApi> Customers { get; set; }
    }
}
