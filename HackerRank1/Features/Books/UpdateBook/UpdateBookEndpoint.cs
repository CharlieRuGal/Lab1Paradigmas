using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace LibraryService.WebAPI.Features.Books.UpdateBook
{
    [ApiController]
    [Route("api/libraries/{libraryId}/books/{bookId}")]
    public class UpdateBookEndpoint : ControllerBase
    {
        private readonly UpdateBookHandler _handler;

        public UpdateBookEndpoint(UpdateBookHandler handler)
        {
            _handler = handler;
        }

        [HttpPut]
        public async Task<IActionResult> Update(int libraryId, int bookId, UpdateBookRequest request)
        {
            var updated = await _handler.HandleAsync(libraryId, bookId, request);
            if (!updated)
                return NotFound();
            return NoContent();
        }
    }
}