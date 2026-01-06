using Varnex.AuthService.Abstractions.Models;
using Microsoft.EntityFrameworkCore;

namespace Varnex.AuthService.Core.Data;

/// <summary>
/// Database context for the Auth service.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppDbContext"/> class.
    /// </summary>
    /// <param name="options">The database context options.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the Users table.
    /// </summary>
    public DbSet<User> Users { get; set; }
}




