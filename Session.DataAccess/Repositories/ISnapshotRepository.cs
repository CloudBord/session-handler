using Session.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Session.DataAccess.Repositories
{
    public interface ISnapshotRepository
    {
        Task<BoardSnapshot?> GetSnapshotAsync(uint boardId);
        Task CreateSnapshot(BoardSnapshot snapshot);
        Task UpdateSnapshotAsync(string id, BoardSnapshot boardSnapshot);
        Task DeleteSnapshotAsync(uint boardId);
    }
}
