namespace LibraryService.WebAPI.Features.Books.CreateBook
{
    public class CreateBookRequest
    {
        public string Name { get; set; }
        public string? Category { get; set; }
    }
}