using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Session.DataAccess.Models;

namespace Session.DataAccess.Context
{
    public class SnapshotsContext : DbContext
    {
        public IMongoCollection<BoardSnapshot> Snapshots;

        public SnapshotsContext(IOptions<SnapshotsDatabaseSettings> settings)
        {
            MongoClient mongoClient = new MongoClient(settings.Value.ConnectionString);
            IMongoDatabase mongoDatabase = mongoClient.GetDatabase(settings.Value.DatabaseName);
            Snapshots = mongoDatabase.GetCollection<BoardSnapshot>(settings.Value.SnapshotsCollectionName);
        }
    }
}
