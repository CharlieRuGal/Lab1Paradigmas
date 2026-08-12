using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace LibraryService.WebAPI.Features.Libraries.GetLibraries
{
    [ApiController]
    [Route("api/libraries")]
    public class GetLibrariesEndpoint : ControllerBase
    {
        private readonly GetLibrariesHandler _handler;

        public GetLibrariesEndpoint(GetLibrariesHandler handler)
        {
            _handler = handler;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var libraries = await _handler.HandleAsync();
            return Ok(libraries);
        }
    }
}