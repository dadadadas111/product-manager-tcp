using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    public class Category
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }

        public ICollection<Product> Products { get; set; }

        public Category(string name)
        {
            Name = name;
            Products = new List<Product>();
        }
    }
}
