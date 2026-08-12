using System.Threading.Tasks;
using LibraryService.WebAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryService.WebAPI.Features.Libraries.UpdateLibrary
{
    public class UpdateLibraryHandler
    {
        private readonly LibraryContext _context;

        public UpdateLibraryHandler(LibraryContext context)
        {
            _context = context;
        }

        public async Task<bool> HandleAsync(int libraryId, UpdateLibraryRequest request)
        {
            var library = await _context.Libraries.FirstOrDefaultAsync(l => l.Id == libraryId);
            if (library == null)
                return false;

            library.Name = request.Name;
            library.Location = request.Location;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}