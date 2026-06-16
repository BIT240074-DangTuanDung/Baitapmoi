using BaithuchanhORM.Models;
using Microsoft.EntityFrameworkCore;

namespace BaithuchanhORM.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Book> Books => Set<Book>();

    public DbSet<BookImage> BookImages => Set<BookImage>();
}

