using System.Linq;
using System.Threading.Tasks;
using LibraryService.WebAPI.Application.DTOs;
using LibraryService.WebAPI.Application.Interfaces;
using LibraryService.WebAPI.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace LibraryService.WebAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LibrariesController : ControllerBase
    {
        private readonly ILibrariesService _librariesService;

        public LibrariesController(ILibrariesService librariesService)
        {
            _librariesService = librariesService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var libraries = await _librariesService.Get(null);
            return Ok(libraries.Select(ToDto));
        }

        [HttpGet("{libraryId}")]
        public async Task<IActionResult> Get(int libraryId)
        {
            var library = (await _librariesService.Get(new[] { libraryId })).FirstOrDefault();
            if (library == null)
                return NotFound();
            return Ok(ToDto(library));
        }

        [HttpPost]
        public async Task<IActionResult> Add(LibraryForm form)
        {
            var library = new Library
            {
                Id = form.Id,
                Name = form.Name,
                Location = form.Location
            };

            await _librariesService.Add(library);
            return Ok(ToDto(library));
        }

        [HttpPut("{libraryId}")]
        public async Task<IActionResult> Update(int libraryId, LibraryForm form)
        {
            var existingLibrary = (await _librariesService.Get(new[] { libraryId })).FirstOrDefault();
            if (existingLibrary == null)
                return NotFound();

            var library = new Library
            {
                Id = libraryId,
                Name = form.Name,
                Location = form.Location
            };

            await _librariesService.Update(library);
            return NoContent();
        }

        [HttpDelete("{libraryId}")]
        public async Task<IActionResult> Delete(int libraryId)
        {
            var library = (await _librariesService.Get(new[] { libraryId })).FirstOrDefault();
            if (library == null)
                return NotFound();

            await _librariesService.Delete(library);
            return NoContent();
        }

        private static LibraryForm ToDto(Library library)
        {
            return new LibraryForm
            {
                Id = library.Id,
                Name = library.Name,
                Location = library.Location
            };
        }
    }
}