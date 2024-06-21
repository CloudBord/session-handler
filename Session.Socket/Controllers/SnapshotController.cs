using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Session.DataAccess.Repositories;
using Session.Socket.Requests;

namespace Session.Socket.Controllers
{
    [Route("snapshot")]
    [ApiController]
    public class SnapshotController : ControllerBase
    {
        private readonly ISnapshotRepository _snapshotRepository;

        public SnapshotController(ISnapshotRepository snapshotRepository)
        {
            _snapshotRepository = snapshotRepository;
        }

        [HttpPost]
        public async Task<IActionResult> SaveSnapshot([FromBody] SaveSnapshotRequest request)
        {
            if (request == null)
            {
                return BadRequest("No snapshot was passed along");
            }
            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBoardContents()
        {
            return Ok();
        }
    }
}
