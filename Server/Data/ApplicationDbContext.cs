using Microsoft.EntityFrameworkCore;
using Server.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure relationships if needed
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Products)
                .WithOne(p => p.Category)
                .HasForeignKey(p => p.CategoryId);

            base.OnModelCreating(modelBuilder);
        }

        // CRUD Methods for Category

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await Categories.ToListAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(Guid id)
        {
            return await Categories.FindAsync(id);
        }

        public async Task AddCategoryAsync(Category category)
        {
            await Categories.AddAsync(category);
            await SaveChangesAsync();
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            Categories.Update(category);
            await SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(Guid id)
        {
            var category = await GetCategoryByIdAsync(id);
            if (category != null)
            {
                Categories.Remove(category);
                await SaveChangesAsync();
            }
        }

        // CRUD Methods for Product

        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await Products.Include(p => p.Category).ToListAsync();
        }

        public async Task<List<Product>> GetProductsByCategoryIdAsync(Guid categoryId)
        {
            return await Products
                .Where(p => p.CategoryId == categoryId)
                .Include(p => p.Category) // Optional: include category details if needed
                .ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(Guid id)
        {
            return await Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product?> AddProductAsync(Product product)
        {
            await Products.AddAsync(product);
            await SaveChangesAsync();
            return product;
        }

        public async Task<Product?> UpdateProductAsync(Product product)
        {
            Products.Update(product);
            await SaveChangesAsync();
            return product;
        }

        public async Task<Product?> DeleteProductAsync(Guid id)
        {
            var product = await GetProductByIdAsync(id);
            if (product != null)
            {
                Products.Remove(product);
                await SaveChangesAsync();
            }
            return product;
        }
    }
}
