using CoreMVCProject.DataAccessLayer.Infrastructure.IRepository;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CoreMVCProjectWeb.ViewComponents
{
    public class CartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;

        public CartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            int sessionCartCount = 0;
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claims != null)
            {
                int.TryParse(HttpContext.Session.GetInt32("SessionCarts").ToString(), out sessionCartCount);
                if (HttpContext.Session.GetInt32("SessionCarts") == null)
                {
                    sessionCartCount = _unitOfWork.Cart.GetAll(x => x.ApplicationUserId == claims.Value).ToList().Count();
                    HttpContext.Session.SetInt32("SessionCarts", sessionCartCount);
                }
            }
            else
            {
                HttpContext.Session.Clear();
            }
            return View(sessionCartCount);

        }
    }
}
