using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext {
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

	public DbSet<Translation> DictionaryRuDe { get; set; }


	protected override void OnModelCreating(ModelBuilder modelBuilder) {
		modelBuilder.Entity<Translation>()
			.Property(t => t.Id)
			.ValueGeneratedNever();
	}
}