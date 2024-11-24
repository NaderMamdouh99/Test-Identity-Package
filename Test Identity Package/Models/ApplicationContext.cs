using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Test_Identity_Package.Models
{
    public class ApplicationContext:IdentityDbContext<ApplicationUser,IdentityRole,string,ApplicationUserClaims, IdentityUserRole<string>, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>
    {
        public ApplicationContext()
        {
            
        }
        public ApplicationContext(DbContextOptions<ApplicationContext> options):base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            #region Rename The Tables 
            var SchemaName = "Secret";
            builder.Entity<ApplicationUser>().ToTable("Users", SchemaName);
            builder.Entity<IdentityRole>().ToTable("Roles", SchemaName);
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles", SchemaName);
            builder.Entity<ApplicationUserClaims>().ToTable("UserClaims", SchemaName);
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins", SchemaName);
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims", SchemaName);
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens", SchemaName);
            #endregion
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
    }
}
