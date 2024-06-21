using System.Text.Json.Serialization;

namespace Session.Socket.Model
{
    public class BoardSnapshot
    {
        [JsonPropertyName("id")]
        public int BoardId { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("jsonContent")]
        public string JSONContent {  get; set; }
    }
}
