using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace mvc.framework.Data
{
	public class ApplicationDbContext : IdentityDbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
		{

		}

		public DbSet<NavigationMenu> NavigationMenu { get; set; }
		public DbSet<RoleMenuPermission> RoleMenuPermission { get; set; }
		public DbSet<Areas.Book.Models.Entity.Category> Categories { get; set; }

		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.Entity<RoleMenuPermission>().HasKey(c => new { c.RoleId, c.NavigationMenuId});

			foreach (var relationship in builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
			{
				relationship.DeleteBehavior = DeleteBehavior.Restrict;
			}

			base.OnModelCreating(builder);
		}
	}
}