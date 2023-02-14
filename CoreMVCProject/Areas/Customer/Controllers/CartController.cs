using CoreMVCProject.DataAccessLayer.Infrastructure.IRepository;
using CoreMVCProject.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CoreMVCProjectWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            CartVM vm = new()
            {
                Carts = _unitOfWork.Cart.GetAll(x => x.ApplicationUserId == claims.Value, includeProperties: "Product")
            };
            foreach (var item in vm.Carts)
            {
                vm.Total += (item.Product.Price * item.Count);
            }
            return View(vm);
        }
        public IActionResult AddItemInCart(int id)
        {
            var cart = _unitOfWork.Cart.GetT(x => x.CartId == id);
            _unitOfWork.Cart.IncrementCartItem(cart, 1);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult RemoveItemFromCart(int id)
        {
            var cart = _unitOfWork.Cart.GetT(x => x.CartId == id);
            if (cart.Count > 1)
            {
                _unitOfWork.Cart.DecrementCartItem(cart, 1);
            }
            else
            {
                _unitOfWork.Cart.Delete(cart);
            }
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult DeleteCart(int id)
        {
            var cart = _unitOfWork.Cart.GetT(x => x.CartId == id);
            _unitOfWork.Cart.Delete(cart);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Summary()
        {
            return View();
        }
    }
}
