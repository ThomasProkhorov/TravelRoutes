using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TravelRoutes.Services.Infrastructure;
using TravelRoutes.Services.Interfaces;

namespace WebApi.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/search")]
    [Route("api/v{version:apiVersion}/search")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] SearchRequest request)
        {
            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
            CancellationToken token = cancelTokenSource.Token;

            try
            {
                var response = await _searchService.SearchAsync(request, token);
                return Content(JsonConvert.SerializeObject(response, new JsonSerializerSettings() { DateFormatString = DateTimeFormats.DateTimeFormat }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
