using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace LibraryService.WebAPI.Features.Books.GetBooks
{
    [ApiController]
    [Route("api/libraries/{libraryId}/books")]
    public class GetBooksEndpoint : ControllerBase
    {
        private readonly GetBooksHandler _handler;

        public GetBooksEndpoint(GetBooksHandler handler)
        {
            _handler = handler;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int libraryId)
        {
            var books = await _handler.HandleAsync(libraryId);
            if (books == null)
                return NotFound();
            return Ok(books);
        }
    }
}