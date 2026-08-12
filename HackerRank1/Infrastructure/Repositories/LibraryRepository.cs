using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryService.WebAPI.Application.Interfaces;
using LibraryService.WebAPI.Domain.Entities;
using LibraryService.WebAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryService.WebAPI.Infrastructure.Repositories
{
    public class LibraryRepository : ILibraryRepository
    {
        private readonly LibraryContext _libraryContext;

        public LibraryRepository(LibraryContext libraryContext)
        {
            _libraryContext = libraryContext;
        }

        public async Task<IEnumerable<Library>> Get(int[] ids)
        {
            var libraries = _libraryContext.Libraries.AsQueryable();

            if (ids != null && ids.Any())
                libraries = libraries.Where(x => ids.Contains(x.Id));

            return await libraries.ToListAsync();
        }

        public async Task<Library> Add(Library library)
        {
            await _libraryContext.Libraries.AddAsync(library);
            await _libraryContext.SaveChangesAsync();
            return library;
        }

        public async Task<IEnumerable<Library>> AddRange(IEnumerable<Library> libraries)
        {
            await _libraryContext.Libraries.AddRangeAsync(libraries);
            await _libraryContext.SaveChangesAsync();
            return libraries;
        }

        public async Task<Library> Update(Library library)
        {
            var existing = await _libraryContext.Libraries.SingleAsync(x => x.Id == library.Id);
            existing.Name = library.Name;
            existing.Location = library.Location;

            _libraryContext.Libraries.Update(existing);
            await _libraryContext.SaveChangesAsync();
            return library;
        }

        public async Task<bool> Delete(Library library)
        {
            var books = _libraryContext.Books.Where(x => x.LibraryId == library.Id);
            _libraryContext.Books.RemoveRange(books);

            var existing = await _libraryContext.Libraries.SingleAsync(x => x.Id == library.Id);
            _libraryContext.Libraries.Remove(existing);
            await _libraryContext.SaveChangesAsync();
            return true;
        }
    }
}