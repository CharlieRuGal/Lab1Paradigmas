using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace LibraryService.WebAPI.Features.Libraries.UpdateLibrary
{
    [ApiController]
    [Route("api/libraries/{libraryId}")]
    public class UpdateLibraryEndpoint : ControllerBase
    {
        private readonly UpdateLibraryHandler _handler;

        public UpdateLibraryEndpoint(UpdateLibraryHandler handler)
        {
            _handler = handler;
        }

        [HttpPut]
        public async Task<IActionResult> Update(int libraryId, UpdateLibraryRequest request)
        {
            var updated = await _handler.HandleAsync(libraryId, request);
            if (!updated)
                return NotFound();
            return NoContent();
        }
    }
}