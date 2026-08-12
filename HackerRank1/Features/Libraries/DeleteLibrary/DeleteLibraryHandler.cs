using System.Threading.Tasks;
using LibraryService.WebAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryService.WebAPI.Features.Libraries.DeleteLibrary
{
    public class DeleteLibraryHandler
    {
        private readonly LibraryContext _context;

        public DeleteLibraryHandler(LibraryContext context)
        {
            _context = context;
        }

        public async Task<bool> HandleAsync(int libraryId)
        {
            var library = await _context.Libraries.FirstOrDefaultAsync(l => l.Id == libraryId);
            if (library == null)
                return false;

            _context.Libraries.Remove(library);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}