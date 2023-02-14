using CoreMVCProject.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CoreMVCProject.DataAccessLayer
{
    public class ApplicationDBContext : IdentityDbContext
    {
        //1. Create Model class for Product
        //2. Run this command. add-migration addProduct
        //3. Run this command. update-database
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {

        }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Cart> Carts { get; set; }
    }
}
