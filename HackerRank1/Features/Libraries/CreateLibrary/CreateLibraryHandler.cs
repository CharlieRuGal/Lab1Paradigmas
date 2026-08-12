using System.Threading.Tasks;
using LibraryService.WebAPI.Data;

namespace LibraryService.WebAPI.Features.Libraries.CreateLibrary
{
    public class CreateLibraryHandler
    {
        private readonly LibraryContext _context;

        public CreateLibraryHandler(LibraryContext context)
        {
            _context = context;
        }

        public async Task<LibraryResponse> HandleAsync(CreateLibraryRequest request)
        {
            var library = new Library { Name = request.Name, Location = request.Location };
            _context.Libraries.Add(library);
            await _context.SaveChangesAsync();

            return new LibraryResponse
            {
                Id = library.Id,
                Name = library.Name,
                Location = library.Location
            };
        }
    }
}