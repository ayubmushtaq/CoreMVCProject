using CoreMVCProject.DataAccessLayer.Infrastructure.IRepository;
using CoreMVCProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreMVCProject.DataAccessLayer.Infrastructure.Respository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly ApplicationDBContext _context;
        public CategoryRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }

        public void Update(Category category)
        {
            var updatedCategory = _context.Categories.FirstOrDefault(x => x.Id == category.Id);
            if (updatedCategory != null)
            {
                updatedCategory.Name = category.Name;
                updatedCategory.DisplayOrder = category.DisplayOrder;
            }
        }
    }
}
