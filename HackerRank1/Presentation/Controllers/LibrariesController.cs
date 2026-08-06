using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LibraryService.WebAPI.Application.DTOs;
using LibraryService.WebAPI.Application.Interfaces;
using LibraryService.WebAPI.Domain.Entities;

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
            var libraryForms = libraries.Select(l => new LibraryForm
            {
                Id = l.Id,
                Name = l.Name,
                Location = l.Location
            });
            return Ok(libraryForms);
        }

        [HttpGet("{libraryId}")]
        public async Task<IActionResult> Get(int libraryId)
        {
            var library = (await _librariesService.Get(new[] { libraryId })).FirstOrDefault();
            if (library == null)
                return NotFound();

            var libraryForm = new LibraryForm
            {
                Id = library.Id,
                Name = library.Name,
                Location = library.Location
            };
            return Ok(libraryForm);
        }

        [HttpPost]
        public async Task<IActionResult> Add(LibraryForm libraryForm)
        {
            var library = new Library
            {
                Name = libraryForm.Name,
                Location = libraryForm.Location
            };

            await _librariesService.Add(library);

            var createdForm = new LibraryForm
            {
                Id = library.Id,
                Name = library.Name,
                Location = library.Location
            };
            return Ok(createdForm);
        }

        [HttpPut("{libraryId}")]
        public async Task<IActionResult> Update(int libraryId, LibraryForm libraryForm)
        {
            var existingLibrary = (await _librariesService.Get(new[] { libraryId })).FirstOrDefault();
            if (existingLibrary == null)
                return NotFound();

            var library = new Library
            {
                Id = existingLibrary.Id,
                Name = libraryForm.Name,
                Location = libraryForm.Location
            };

            await _librariesService.Update(library);
            return NoContent();
        }

        [HttpDelete("{libraryId}")]
        public async Task<IActionResult> Delete(int libraryId)
        {
            var existingLibrary = (await _librariesService.Get(new[] { libraryId })).FirstOrDefault();
            if (existingLibrary == null)
                return NotFound();

            await _librariesService.Delete(existingLibrary);
            return NoContent();
        }
    }
}
