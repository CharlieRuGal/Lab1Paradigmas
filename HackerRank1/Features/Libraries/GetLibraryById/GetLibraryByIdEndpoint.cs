using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace LibraryService.WebAPI.Features.Libraries.GetLibraryById
{
    [ApiController]
    [Route("api/libraries/{libraryId}")]
    public class GetLibraryByIdEndpoint : ControllerBase
    {
        private readonly GetLibraryByIdHandler _handler;

        public GetLibraryByIdEndpoint(GetLibraryByIdHandler handler)
        {
            _handler = handler;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int libraryId)
        {
            var library = await _handler.HandleAsync(libraryId);
            if (library == null)
                return NotFound();
            return Ok(library);
        }
    }
}