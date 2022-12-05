using DocumentStorageAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DocumentStorageAPI.DB
{
    public class DocumentDbContext : DbContext
    {
        public DocumentDbContext(DbContextOptions<DocumentDbContext> options) : base(options)
        {
        }

        public DbSet<Document> Document { get; set; }
        public DbSet<Tag> Tag { get; set; }
    }
}
