using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryService.WebAPI.Domain.Entities;

namespace LibraryService.WebAPI.Application.Interfaces
{
    public interface IBooksService
    {
        Task<IEnumerable<Book>> Get(int libraryId, int[] ids);

        Task<Book> Add(Book book);

        Task<Book> Update(Book book);

        Task<bool> Delete(Book book);
    }
}