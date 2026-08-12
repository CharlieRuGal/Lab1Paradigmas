using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace LibraryService.WebAPI.Features.Libraries.DeleteLibrary
{
    [ApiController]
    [Route("api/libraries/{libraryId}")]
    public class DeleteLibraryEndpoint : ControllerBase
    {
        private readonly DeleteLibraryHandler _handler;

        public DeleteLibraryEndpoint(DeleteLibraryHandler handler)
        {
            _handler = handler;
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int libraryId)
        {
            var deleted = await _handler.HandleAsync(libraryId);
            if (!deleted)
                return NotFound();
            return NoContent();
        }
    }
}