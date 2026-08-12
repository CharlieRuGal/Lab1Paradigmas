using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LibraryService.WebAPI.Features.Books.CreateBook
{
    [ApiController]
    [Route("api/libraries/{libraryId}/books")]
    public class CreateBookEndpoint : ControllerBase
    {
        private readonly CreateBookHandler _handler;

        public CreateBookEndpoint(CreateBookHandler handler)
        {
            _handler = handler;
        }

        [HttpPost]
        public async Task<IActionResult> Add(int libraryId, CreateBookRequest request)
        {
            var created = await _handler.HandleAsync(libraryId, request);
            if (created == null)
                return NotFound();
            return StatusCode(StatusCodes.Status201Created, created);
        }
    }
}