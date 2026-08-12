using System.Threading.Tasks;
using LibraryService.WebAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryService.WebAPI.Features.Books.DeleteBook
{
    public class DeleteBookHandler
    {
        private readonly LibraryContext _context;

        public DeleteBookHandler(LibraryContext context)
        {
            _context = context;
        }

        public async Task<bool> HandleAsync(int libraryId, int bookId)
        {
            var book = await _context.Books
                .FirstOrDefaultAsync(b => b.Id == bookId && b.LibraryId == libraryId);
            if (book == null)
                return false;

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}