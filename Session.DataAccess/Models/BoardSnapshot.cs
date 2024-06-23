using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Session.DataAccess.Models
{
    public class BoardSnapshot
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        [BsonElement("BoardId")]
        public uint BoardId {  get; set; }
        [BsonElement("MemberIds")]
        public IEnumerable<uint> MemberIds { get; set; } = [];
        [BsonElement("Snapshot")]
        public object? Snapshot { get; set; }
    }
}
