using System.Threading.Tasks;
using LibraryService.WebAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryService.WebAPI.Features.Libraries.GetLibraryById
{
    public class GetLibraryByIdHandler
    {
        private readonly LibraryContext _context;

        public GetLibraryByIdHandler(LibraryContext context)
        {
            _context = context;
        }

        public async Task<LibraryResponse?> HandleAsync(int libraryId)
        {
            var library = await _context.Libraries.AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == libraryId);

            if (library == null)
                return null;

            return new LibraryResponse
            {
                Id = library.Id,
                Name = library.Name,
                Location = library.Location
            };
        }
    }
}