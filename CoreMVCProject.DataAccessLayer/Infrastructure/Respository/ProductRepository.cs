using CoreMVCProject.DataAccessLayer.Infrastructure.IRepository;
using CoreMVCProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreMVCProject.DataAccessLayer.Infrastructure.Respository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDBContext _context;
        public ProductRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }

        public void Update(Product product)
        {
            var updatedProduct = _context.Products.FirstOrDefault(x => x.Id == product.Id);
            if (updatedProduct != null)
            {
                updatedProduct.Name = product.Name;
                updatedProduct.Description = product.Description;
                updatedProduct.Price = product.Price;
                if (product.ImageURL != null)
                {
                    updatedProduct.ImageURL = product.ImageURL;
                }
            }
        }
    }
}
