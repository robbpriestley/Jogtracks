using Microsoft.EntityFrameworkCore;

namespace DigitalWizardry.SPA_Template
{
	public class MainContext : DbContext
	{
		public DbSet<Account> Account { get; set; }
		public DbSet<Item> Item { get; set; }
		
		public MainContext(DbContextOptions<MainContext> options) : base(options){}
	}

	// A separate context for ServiceLog ensures that if the main context gets invalidated due to a bug 
	// (something like a PK exception), that a log entry can still be written to record the event.
	public class ServiceLogContext : DbContext
	{
		public DbSet<ServiceLog> ServiceLog { get; set; }
	
		public ServiceLogContext(DbContextOptions<ServiceLogContext> options) : base(options){}
	}
}