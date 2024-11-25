using System.Text.Json.Serialization;

namespace ai_devs_proj.S04E01.Models
{
    internal class UrlsResponseModel
    {
        [JsonPropertyName("result")]
        public List<ListOfPhotosModel> Result { get; set; }
    }
}
