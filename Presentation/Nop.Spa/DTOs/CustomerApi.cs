using Newtonsoft.Json;

namespace Nop.Spa.DTOs
{
    // Simplified Customer dto object with only the first and last name
    public class CustomerApi
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }
    }
}
