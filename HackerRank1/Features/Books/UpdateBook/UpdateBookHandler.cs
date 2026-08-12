using System.Threading.Tasks;
using LibraryService.WebAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryService.WebAPI.Features.Books.UpdateBook
{
    public class UpdateBookHandler
    {
        private readonly LibraryContext _context;

        public UpdateBookHandler(LibraryContext context)
        {
            _context = context;
        }

        public async Task<bool> HandleAsync(int libraryId, int bookId, UpdateBookRequest request)
        {
            var book = await _context.Books
                .FirstOrDefaultAsync(b => b.Id == bookId && b.LibraryId == libraryId);
            if (book == null)
                return false;

            book.Name = request.Name;
            book.Category = request.Category ?? string.Empty;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}