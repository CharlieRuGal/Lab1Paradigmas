namespace LibraryService.WebAPI.Features.Books.UpdateBook
{
    public class UpdateBookRequest
    {
        public string Name { get; set; }
        public string? Category { get; set; }
    }
}