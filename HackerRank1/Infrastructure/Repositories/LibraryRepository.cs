using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryService.WebAPI.Domain.Entities;
using LibraryService.WebAPI.Domain.Interfaces;
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
            var libraryForChanges = await _libraryContext.Libraries.SingleAsync(x => x.Id == library.Id);
            libraryForChanges.Name = library.Name;
            libraryForChanges.Location = library.Location;

            _libraryContext.Libraries.Update(libraryForChanges);
            await _libraryContext.SaveChangesAsync();
            return library;
        }

        public async Task<bool> Delete(Library library)
        {
            var libraryForDeletion = await _libraryContext.Libraries.SingleOrDefaultAsync(x => x.Id == library.Id);
            if (libraryForDeletion == null)
                return false;

            _libraryContext.Libraries.Remove(libraryForDeletion);
            await _libraryContext.SaveChangesAsync();
            return true;
        }
    }
}
