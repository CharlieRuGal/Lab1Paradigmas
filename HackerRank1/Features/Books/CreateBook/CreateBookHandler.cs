using System.Threading.Tasks;
using LibraryService.WebAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryService.WebAPI.Features.Books.CreateBook
{
    public class CreateBookHandler
    {
        private readonly LibraryContext _context;

        public CreateBookHandler(LibraryContext context)
        {
            _context = context;
        }

        public async Task<BookResponse?> HandleAsync(int libraryId, CreateBookRequest request)
        {
            var libraryExists = await _context.Libraries.AnyAsync(l => l.Id == libraryId);
            if (!libraryExists)
                return null;

            var book = new Book
            {
                Name = request.Name,
                Category = request.Category ?? string.Empty,
                LibraryId = libraryId
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return new BookResponse
            {
                Id = book.Id,
                Name = book.Name,
                Category = book.Category,
                LibraryId = book.LibraryId
            };
        }
    }
}