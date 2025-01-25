using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    public class Product
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }

        public Guid CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
