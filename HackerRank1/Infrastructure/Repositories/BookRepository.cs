using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryService.WebAPI.Application.Interfaces;
using LibraryService.WebAPI.Domain.Entities;
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
            var books = _libraryContext.Books
                .Where(x => x.LibraryId == libraryId)
                .AsQueryable();

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
            var existing = await _libraryContext.Books
                .SingleAsync(x => x.Id == book.Id && x.LibraryId == book.LibraryId);
            existing.Name = book.Name;
            existing.Category = book.Category;

            _libraryContext.Books.Update(existing);
            await _libraryContext.SaveChangesAsync();
            return book;
        }

        public async Task<bool> Delete(Book book)
        {
            var existing = await _libraryContext.Books
                .SingleAsync(x => x.Id == book.Id && x.LibraryId == book.LibraryId);
            _libraryContext.Books.Remove(existing);
            await _libraryContext.SaveChangesAsync();
            return true;
        }
    }
}