using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryService.WebAPI.Domain.Entities;

namespace LibraryService.WebAPI.Application.Interfaces
{
    public interface ILibrariesService
    {
        Task<IEnumerable<Library>> Get(int[] ids);

        Task<Library> Add(Library library);

        Task<Library> Update(Library library);

        Task<bool> Delete(Library library);
    }
}