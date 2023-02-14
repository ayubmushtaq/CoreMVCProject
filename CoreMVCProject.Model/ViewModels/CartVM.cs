using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreMVCProject.Models.ViewModels
{
    public class CartVM
    {
        public IEnumerable<Cart> Carts { get; set; }
    }
}
