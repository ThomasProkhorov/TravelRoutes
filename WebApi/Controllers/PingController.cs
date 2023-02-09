using Microsoft.AspNetCore.Mvc;
using TravelRoutes.Services.Interfaces;

namespace WebApi.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/ping")]
    [Route("api/v{version:apiVersion}/ping")]
    public class PingController : ControllerBase
    {
        private readonly ISearchService _searchService;

        public PingController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
            CancellationToken token = cancelTokenSource.Token;

            try
            {
                if (await _searchService.IsAvailableAsync(token))
                {
                    return Ok();
                }
                return StatusCode(500);
            }
            catch (Exception ex) 
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
