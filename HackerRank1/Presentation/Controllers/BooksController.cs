using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using LibraryService.WebAPI.Application.DTOs;
using LibraryService.WebAPI.Application.Interfaces;
using LibraryService.WebAPI.Domain.Entities;

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
        public async Task<IActionResult> Get(int libraryId)
        {
            var library = (await _librariesService.Get(new[] { libraryId })).FirstOrDefault();
            if (library == null)
                return NotFound();

            var books = await _booksService.Get(libraryId, null);
            var bookForms = books.Select(b => new BookForm
            {
                Id = b.Id,
                Name = b.Name,
                Category = b.Category,
                LibraryId = b.LibraryId
            });
            return Ok(bookForms);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int libraryId, BookForm bookForm)
        {
            var library = (await _librariesService.Get(new[] { libraryId })).FirstOrDefault();
            if (library == null)
                return NotFound();

            var book = new Book
            {
                Name = bookForm.Name,
                Category = bookForm.Category ?? string.Empty,
                LibraryId = libraryId
            };

            await _booksService.Add(book);

            var createdForm = new BookForm
            {
                Id = book.Id,
                Name = book.Name,
                Category = book.Category,
                LibraryId = book.LibraryId
            };
            return StatusCode(StatusCodes.Status201Created, createdForm);
        }

        [HttpPut("{bookId}")]
        public async Task<IActionResult> Update(int libraryId, int bookId, BookForm bookForm)
        {
            var library = (await _librariesService.Get(new[] { libraryId })).FirstOrDefault();
            if (library == null)
                return NotFound();

            var existingBook = (await _booksService.Get(libraryId, new[] { bookId })).FirstOrDefault();
            if (existingBook == null)
                return NotFound();

            var book = new Book
            {
                Id = existingBook.Id,
                Name = bookForm.Name,
                Category = bookForm.Category ?? string.Empty,
                LibraryId = libraryId
            };

            await _booksService.Update(book);
            return NoContent();
        }

        [HttpDelete("{bookId}")]
        public async Task<IActionResult> Delete(int libraryId, int bookId)
        {
            var library = (await _librariesService.Get(new[] { libraryId })).FirstOrDefault();
            if (library == null)
                return NotFound();

            var existingBook = (await _booksService.Get(libraryId, new[] { bookId })).FirstOrDefault();
            if (existingBook == null)
                return NotFound();

            await _booksService.Delete(existingBook);
            return NoContent();
        }
    }
}
