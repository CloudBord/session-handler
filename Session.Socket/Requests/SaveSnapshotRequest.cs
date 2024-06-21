using System.Text.Json.Serialization;

namespace Session.Socket.Requests
{
    public class SaveSnapshotRequest
    {
        [JsonPropertyName("boardId")]
        public required uint BoardId { get; set; }
        [JsonPropertyName("document")]
        public required object Document { get; set; }

        public SaveSnapshotRequest() { }
    }
}
