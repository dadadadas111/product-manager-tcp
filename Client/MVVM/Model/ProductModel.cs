using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.MVVM.Model
{
    class ProductModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string CategoryId { get; set; }

        public ProductModel() : this("", "", 0, 0, "") { }

        public ProductModel( string id, string name, decimal price, int stock, string categoryId)
        {
            Id = id;
            Name = name;
            Price = price;
            Stock = stock;
            CategoryId = categoryId;
        }
    }
}
