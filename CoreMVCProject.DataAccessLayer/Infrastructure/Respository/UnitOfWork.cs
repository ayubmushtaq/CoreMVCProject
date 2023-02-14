using CoreMVCProject.DataAccessLayer.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreMVCProject.DataAccessLayer.Infrastructure.Respository
{
    public class UnitOfWork : IUnitOfWork
    {
        public ICategoryRepository Category { get; private set; }
        public IProductRepository Product { get; private set; }

        public IApplicationUser ApplicationUser { get; private set; }

        public ICartRepository Cart { get; private set; }

        private readonly ApplicationDBContext _context;
        public UnitOfWork(ApplicationDBContext context) 
        {
            _context = context;
            Category = new CategoryRepository(context);
            Product = new ProductRepository(context);
            ApplicationUser = new ApplicationUserRepository(context);
            Cart = new CartRepository(context);
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
