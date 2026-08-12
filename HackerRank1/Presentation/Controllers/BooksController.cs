using System.Linq;
using System.Threading.Tasks;
using LibraryService.WebAPI.Application.DTOs;
using LibraryService.WebAPI.Application.Interfaces;
using LibraryService.WebAPI.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace LibraryService.WebAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/libraries/{libraryId}/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly ILibrariesService _librariesService;
        private readonly IBooksService _booksService;

        public BooksController(IBooksService booksService, ILibrariesService librariesService)
        {
            _librariesService = librariesService;
            _booksService = booksService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int libraryId)
        {
            if (!await LibraryExists(libraryId))
                return NotFound();

            var books = await _booksService.Get(libraryId, null);
            return Ok(books.Select(ToDto));
        }

        [HttpPost]
        public async Task<IActionResult> Add(int libraryId, BookForm form)
        {
            if (!await LibraryExists(libraryId))
                return NotFound();

            var book = new Book
            {
                Name = form.Name,
                Category = form.Category ?? string.Empty,
                LibraryId = libraryId
            };

            await _booksService.Add(book);
            return StatusCode(201, ToDto(book));
        }

        [HttpPut("{bookId}")]
        public async Task<IActionResult> Update(int libraryId, int bookId, BookForm form)
        {
            if (!await LibraryExists(libraryId))
                return NotFound();

            var book = (await _booksService.Get(libraryId, new[] { bookId })).FirstOrDefault();
            if (book == null)
                return NotFound();

            book.Name = form.Name;
            book.Category = form.Category ?? string.Empty;

            await _booksService.Update(book);
            return NoContent();
        }

        [HttpDelete("{bookId}")]
        public async Task<IActionResult> Delete(int libraryId, int bookId)
        {
            if (!await LibraryExists(libraryId))
                return NotFound();

            var book = (await _booksService.Get(libraryId, new[] { bookId })).FirstOrDefault();
            if (book == null)
                return NotFound();

            await _booksService.Delete(book);
            return NoContent();
        }

        private async Task<bool> LibraryExists(int libraryId)
        {
            return (await _librariesService.Get(new[] { libraryId })).Any();
        }

        private static BookForm ToDto(Book book)
        {
            return new BookForm
            {
                Id = book.Id,
                Name = book.Name,
                Category = book.Category,
                LibraryId = book.LibraryId
            };
        }
    }
}