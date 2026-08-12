namespace LibraryService.WebAPI.Features.Books.UpdateBook
{
    public class BookResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public int LibraryId { get; set; }
    }
}