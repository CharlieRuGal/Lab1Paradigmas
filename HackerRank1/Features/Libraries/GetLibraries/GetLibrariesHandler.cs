using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryService.WebAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryService.WebAPI.Features.Libraries.GetLibraries
{
    public class GetLibrariesHandler
    {
        private readonly LibraryContext _context;

        public GetLibrariesHandler(LibraryContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<LibraryResponse>> HandleAsync()
        {
            var libraries = await _context.Libraries.AsNoTracking().ToListAsync();
            return libraries
                .Select(l => new LibraryResponse
                {
                    Id = l.Id,
                    Name = l.Name,
                    Location = l.Location
                })
                .ToList();
        }
    }
}