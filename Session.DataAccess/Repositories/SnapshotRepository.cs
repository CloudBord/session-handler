using MongoDB.Driver;
using Session.DataAccess.Context;
using Session.DataAccess.Models;

namespace Session.DataAccess.Repositories
{
    public class SnapshotRepository : ISnapshotRepository
    {
        private readonly SnapshotsContext _snapshotsContext;

        public SnapshotRepository(SnapshotsContext snapshotsContext) 
        {
            _snapshotsContext = snapshotsContext;
        }

        public async Task CreateSnapshot(BoardSnapshot snapshot)
        {
            await _snapshotsContext.Snapshots.InsertOneAsync(snapshot);
        }

        public async Task DeleteSnapshotAsync(uint boardId)
        {
            await _snapshotsContext.Snapshots.DeleteOneAsync(x => x.BoardId == boardId);
        }

        public async Task<BoardSnapshot?> GetSnapshotAsync(uint boardId)
        {
            return await _snapshotsContext.Snapshots.Find(x => x.BoardId == boardId).FirstOrDefaultAsync();
        }

        public async Task UpdateSnapshotAsync(string id, BoardSnapshot boardSnapshot)
        {
            await _snapshotsContext.Snapshots.ReplaceOneAsync(x => x.Id == id, boardSnapshot);
        }
    }
}
