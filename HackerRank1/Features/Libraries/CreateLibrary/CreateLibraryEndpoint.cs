using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace LibraryService.WebAPI.Features.Libraries.CreateLibrary
{
    [ApiController]
    [Route("api/libraries")]
    public class CreateLibraryEndpoint : ControllerBase
    {
        private readonly CreateLibraryHandler _handler;

        public CreateLibraryEndpoint(CreateLibraryHandler handler)
        {
            _handler = handler;
        }

        [HttpPost]
        public async Task<IActionResult> Add(CreateLibraryRequest request)
        {
            var created = await _handler.HandleAsync(request);
            return Ok(created);
        }
    }
}