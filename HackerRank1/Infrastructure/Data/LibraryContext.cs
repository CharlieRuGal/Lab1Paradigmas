using LibraryService.WebAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryService.WebAPI.Infrastructure.Data
{
    public class LibraryContext : DbContext
    {
        public LibraryContext(DbContextOptions<LibraryContext> options)
            : base(options)
        { }

        public DbSet<Library> Libraries { get; set; }
        public DbSet<Book> Books { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>()
                .HasOne(b => b.Library)
                .WithMany()
                .HasForeignKey(b => b.LibraryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}