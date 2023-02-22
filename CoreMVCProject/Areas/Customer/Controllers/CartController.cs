using CoreMVCProject.CommonHelper;
using CoreMVCProject.DataAccessLayer.Infrastructure.IRepository;
using CoreMVCProject.Models;
using CoreMVCProject.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
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
                Carts = _unitOfWork.Cart.GetAll(x => x.ApplicationUserId == claims.Value, includeProperties: "Product"),
                OrderHeader = new CoreMVCProject.Models.OrderHeader()
            };

            foreach (var item in vm.Carts)
            {
                vm.OrderHeader.OrderTotal += (item.Product.Price * item.Count);
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
            HttpContext.Session.SetInt32("SessionCarts", _unitOfWork.Cart.GetAll(x => x.ApplicationUserId == cart.ApplicationUserId).Count());
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            CartVM vm = new()
            {
                Carts = _unitOfWork.Cart.GetAll(x => x.ApplicationUserId == claims.Value, includeProperties: "Product"),
                OrderHeader = new CoreMVCProject.Models.OrderHeader()
            };
            vm.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetT(x => x.Id == claims.Value);
            vm.OrderHeader.ApplicationUserId = claims.Value;
            vm.OrderHeader.Name = vm.OrderHeader.ApplicationUser.Name;
            vm.OrderHeader.Phone = vm.OrderHeader.ApplicationUser.PhoneNumber;
            vm.OrderHeader.City = vm.OrderHeader.ApplicationUser.City;
            vm.OrderHeader.State = vm.OrderHeader.ApplicationUser.State;
            vm.OrderHeader.PostalCode = vm.OrderHeader.ApplicationUser.PinCode;
            vm.OrderHeader.Address = vm.OrderHeader.ApplicationUser.Address;
            foreach (var item in vm.Carts)
            {
                vm.OrderHeader.OrderTotal += (item.Product.Price * item.Count);
            }
            return View(vm);
        }
        [HttpPost]
        public IActionResult Summary(CartVM vm)
        {

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            vm.Carts = _unitOfWork.Cart.GetAll(x => x.ApplicationUserId == claims.Value, includeProperties: "Product");
            vm.OrderHeader.OrderStatus = OrderStatus.StatusPending;
            vm.OrderHeader.PaymentStatus = PaymentStatus.StatusPending;
            vm.OrderHeader.DateOfOrder = DateTime.Now;
            vm.OrderHeader.ApplicationUserId = claims.Value;
            //vm.OrderHeader.Name = vm.OrderHeader.ApplicationUser.Name;
            //vm.OrderHeader.Phone = vm.OrderHeader.ApplicationUser.PhoneNumber;
            //vm.OrderHeader.City = vm.OrderHeader.ApplicationUser.City;
            //vm.OrderHeader.State = vm.OrderHeader.ApplicationUser.State;
            //vm.OrderHeader.PostalCode = vm.OrderHeader.ApplicationUser.PinCode;
            //vm.OrderHeader.Address = vm.OrderHeader.ApplicationUser.Address;
            foreach (var item in vm.Carts)
            {
                vm.OrderHeader.OrderTotal += (item.Product.Price * item.Count);
            }
            _unitOfWork.OrderHeader.Add(vm.OrderHeader);
            _unitOfWork.Save();
            foreach (var itemDetail in vm.Carts)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = itemDetail.ProductId,
                    Price = itemDetail.Product.Price,
                    OrderHeaderId = vm.OrderHeader.OrderHeaderId,
                    Count = itemDetail.Count
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
            }

            var domain = "https://localhost:7011/";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"Customer/Cart/OrderSuccess?id={vm.OrderHeader.OrderHeaderId}",
                CancelUrl = domain + "Customer/Cart/Index",
            };
            foreach (var itemCarts in vm.Carts)
            {
                var lineItemOptions = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(itemCarts.Product.Price*100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = itemCarts.Product.Name,
                        },
                    },
                    Quantity = itemCarts.Count,
                };
                options.LineItems.Add(lineItemOptions);
            }
            var service = new SessionService();
            Session session = service.Create(options);
            _unitOfWork.OrderHeader.PaymentStatus(vm.OrderHeader.OrderHeaderId, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();
            _unitOfWork.Cart.DeleteRange(vm.Carts);
            _unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);



           
            return RedirectToAction("Index", "Home");

        }
        public IActionResult OrderSuccess(int id)
        {
            var orderHeader = _unitOfWork.OrderHeader.GetT(x => x.OrderHeaderId == id);
            if (orderHeader != null)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if (session.PaymentStatus.ToLower()=="paid")
                {
                    _unitOfWork.OrderHeader.UpdateStatus(id, OrderStatus.StatusApproved, PaymentStatus.StatusApproved);
                    _unitOfWork.OrderHeader.PaymentStatus(id, session.Id, session.PaymentIntentId);
                }
                List<Cart> _cart = _unitOfWork.Cart.GetAll(x => x.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
                _unitOfWork.Cart.DeleteRange(_cart);
                _unitOfWork.Save();
            }
            return View(id);
        }
    }
}
