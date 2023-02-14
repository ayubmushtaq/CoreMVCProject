using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreMVCProject.Models.ViewModels
{
    public class CategoryVM
    {
        public Category Category { get; set; } = new();
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
    }
}
