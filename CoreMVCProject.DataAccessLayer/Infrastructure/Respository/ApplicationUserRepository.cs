using CoreMVCProject.DataAccessLayer.Infrastructure.IRepository;
using CoreMVCProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreMVCProject.DataAccessLayer.Infrastructure.Respository
{
    internal class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUser
    {
        private readonly ApplicationDBContext _context;
        public ApplicationUserRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }
    }
}
