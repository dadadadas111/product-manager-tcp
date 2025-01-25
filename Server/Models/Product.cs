using System;

namespace Server.Models
{
    public class Product
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }

        public Guid CategoryId { get; set; }
        public Category? Category { get; set; }

        public Product(string name, decimal price, int stock, Guid categoryId)
        {
            Id = Guid.NewGuid();
            Name = name;
            Price = price;
            Stock = stock;
            CategoryId = categoryId;
        }
    }
}
