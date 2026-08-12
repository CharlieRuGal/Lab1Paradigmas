using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace LibraryService.WebAPI.Features.Books.DeleteBook
{
    [ApiController]
    [Route("api/libraries/{libraryId}/books/{bookId}")]
    public class DeleteBookEndpoint : ControllerBase
    {
        private readonly DeleteBookHandler _handler;

        public DeleteBookEndpoint(DeleteBookHandler handler)
        {
            _handler = handler;
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int libraryId, int bookId)
        {
            var deleted = await _handler.HandleAsync(libraryId, bookId);
            if (!deleted)
                return NotFound();
            return NoContent();
        }
    }
}