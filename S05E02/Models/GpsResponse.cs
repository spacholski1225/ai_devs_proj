using Newtonsoft.Json;

namespace ai_devs_proj.S05E02.Models
{
    public class GpsResponse
    {
        public int Code { get; set; }
        public Coordinates Message { get; set; }
    }

    public class Coordinates
    {
        [JsonProperty(PropertyName = "lat")]
        public double Lat { get; set; }

        [JsonProperty(PropertyName = "lon")]
        public double Lon { get; set; }
    }
}
