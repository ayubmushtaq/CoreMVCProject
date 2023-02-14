using CoreMVCProject.Models;

namespace CoreMVCProject.DataAccessLayer.Infrastructure.IRepository
{
    public interface IProductRepository : IRepository<Product>
    {
        void Update(Product product);
    }
}