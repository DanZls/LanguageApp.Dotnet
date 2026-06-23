using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext {
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

	public DbSet<Client> Clients { get; set; }
	public DbSet<Dictionary> Dictionaries { get; set; }
	public DbSet<Word> Words { get; set; }
}