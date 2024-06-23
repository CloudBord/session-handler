using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Session.DataAccess.Models
{
    public class SnapshotsDatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string SnapshotsCollectionName { get; set; } = null!;
    }
}
