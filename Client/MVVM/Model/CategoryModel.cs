using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.MVVM.Model
{
    class CategoryModel
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public CategoryModel(string id, string name)
        {
            Name = name;
            Id = id;
        }
    }
}
