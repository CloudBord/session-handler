using Microsoft.AspNetCore.Mvc;
using Session.Socket.Services;

namespace Session.Socket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RedisController(IRedisCacheService redisCacheService) : ControllerBase
    {
        private readonly IRedisCacheService _redisCacheService = redisCacheService;

        [HttpGet]
        public async Task<IActionResult> GetData()
        {
            var value = await _redisCacheService.GetCacheValueAsync<string>("test");
            if (value == null)
            {
                value = "empty string";
            }
            return Ok(value);
        }
    }
}
