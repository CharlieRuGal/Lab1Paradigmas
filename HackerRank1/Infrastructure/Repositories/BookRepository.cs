using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryService.WebAPI.Domain.Entities;
using LibraryService.WebAPI.Domain.Interfaces;
using LibraryService.WebAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryService.WebAPI.Infrastructure.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly LibraryContext _libraryContext;

        public BookRepository(LibraryContext libraryContext)
        {
            _libraryContext = libraryContext;
        }

        public async Task<IEnumerable<Book>> Get(int libraryId, int[] ids)
        {
            var books = _libraryContext.Books.Where(x => x.LibraryId == libraryId);

            if (ids != null && ids.Any())
                books = books.Where(x => ids.Contains(x.Id));

            return await books.ToListAsync();
        }

        public async Task<Book> Add(Book book)
        {
            await _libraryContext.Books.AddAsync(book);
            await _libraryContext.SaveChangesAsync();
            return book;
        }

        public async Task<Book> Update(Book book)
        {
            var bookForChanges = await _libraryContext.Books.SingleAsync(x => x.Id == book.Id);
            bookForChanges.Name = book.Name;
            bookForChanges.Category = book.Category;
            bookForChanges.LibraryId = book.LibraryId;

            _libraryContext.Books.Update(bookForChanges);
            await _libraryContext.SaveChangesAsync();
            return book;
        }

        public async Task<bool> Delete(Book book)
        {
            var bookForDeletion = await _libraryContext.Books.SingleOrDefaultAsync(x => x.Id == book.Id);
            if (bookForDeletion == null)
                return false;

            _libraryContext.Books.Remove(bookForDeletion);
            await _libraryContext.SaveChangesAsync();
            return true;
        }
    }
}
