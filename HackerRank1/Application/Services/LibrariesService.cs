using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryService.WebAPI.Application.Interfaces;
using LibraryService.WebAPI.Domain.Entities;

namespace LibraryService.WebAPI.Application.Services
{
    public class LibrariesService : ILibrariesService
    {
        private readonly ILibraryRepository _libraryRepository;

        public LibrariesService(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        public async Task<IEnumerable<Library>> Get(int[] ids)
        {
            return await _libraryRepository.Get(ids);
        }

        public async Task<Library> Add(Library library)
        {
            return await _libraryRepository.Add(library);
        }

        public async Task<Library> Update(Library library)
        {
            return await _libraryRepository.Update(library);
        }

        public async Task<bool> Delete(Library library)
        {
            return await _libraryRepository.Delete(library);
        }
    }
}