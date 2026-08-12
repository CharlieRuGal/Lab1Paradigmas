using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryService.WebAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryService.WebAPI.Features.Books.GetBooks
{
    public class GetBooksHandler
    {
        private readonly LibraryContext _context;

        public GetBooksHandler(LibraryContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<BookResponse>?> HandleAsync(int libraryId)
        {
            var libraryExists = await _context.Libraries.AnyAsync(l => l.Id == libraryId);
            if (!libraryExists)
                return null;

            var books = await _context.Books
                .AsNoTracking()
                .Where(b => b.LibraryId == libraryId)
                .ToListAsync();

            return books
                .Select(b => new BookResponse
                {
                    Id = b.Id,
                    Name = b.Name,
                    Category = b.Category,
                    LibraryId = b.LibraryId
                })
                .ToList();
        }
    }
}