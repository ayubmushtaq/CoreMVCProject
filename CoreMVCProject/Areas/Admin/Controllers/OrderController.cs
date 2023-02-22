using CoreMVCProject.CommonHelper;
using CoreMVCProject.DataAccessLayer.Infrastructure.IRepository;
using CoreMVCProject.Models;
using CoreMVCProject.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace CoreMVCProjectWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        #region API
        public IActionResult AllOrders(string status)
        {
            IEnumerable<OrderHeader> orderHeaders;
            if (User.IsInRole("Admin") || User.IsInRole("Employee"))
            {
                orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                orderHeaders = _unitOfWork.OrderHeader.GetAll(x => x.ApplicationUserId == claims.Value);
            }
            switch (status)
            {
                case "pending":
                    orderHeaders = orderHeaders.Where(x => x.PaymentStatus == PaymentStatus.StatusPending);
                    break;
                case "approved":
                    orderHeaders = orderHeaders.Where(x => x.PaymentStatus == PaymentStatus.StatusApproved);
                    break;
                case "underprocess":
                    orderHeaders = orderHeaders.Where(x => x.OrderStatus == OrderStatus.StatusInProgress);
                    break;
                case "shipped":
                    orderHeaders = orderHeaders.Where(x => x.OrderStatus == OrderStatus.StatusShipped);
                    break;
                default:
                    break;
            }
            return Json(new { data = orderHeaders });
        }
        #endregion
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult OrderDetails(int id)
        {
            OrderVM vm = new()
            {
                OrderHeader = _unitOfWork.OrderHeader.GetT(x => x.OrderHeaderId == id, includeProperties: "ApplicationUser"),
                OrderDetails = _unitOfWork.OrderDetail.GetAll(x => x.OrderHeaderId == id, includeProperties: "Product")
            };
            return View(vm);
        }
        [Authorize(Roles = WebsiteRole.Role_Admin + "," + WebsiteRole.Role_Employee)]
        [HttpPost]
        public IActionResult OrderDetails(OrderVM vm)
        {
            var oHeader = _unitOfWork.OrderHeader.GetT(x => x.OrderHeaderId == vm.OrderHeader.OrderHeaderId);
            if (oHeader != null)
            {
                oHeader.Name = vm.OrderHeader.Name;
                oHeader.Phone = vm.OrderHeader.Phone;
                oHeader.Address = vm.OrderHeader.Address;
                oHeader.City = vm.OrderHeader.City;
                oHeader.State = vm.OrderHeader.State;
                oHeader.PostalCode = vm.OrderHeader.PostalCode;
                if (vm.OrderHeader.Carrier != null)
                {
                    oHeader.Carrier = vm.OrderHeader.Carrier;
                }
                if (vm.OrderHeader.TrackingNumber != null)
                {
                    oHeader.TrackingNumber = vm.OrderHeader.TrackingNumber;
                }
                _unitOfWork.OrderHeader.Update(oHeader);
                _unitOfWork.Save();
                TempData["success"] = "Info Updated";
                return RedirectToAction("OrderDetails", "Order", new { id = vm.OrderHeader.OrderHeaderId });
            }
            return View(vm);
        }
        [Authorize(Roles = WebsiteRole.Role_Admin + "," + WebsiteRole.Role_Employee)]
        public IActionResult InProcess(OrderVM vm)
        {
            _unitOfWork.OrderHeader.UpdateStatus(vm.OrderHeader.OrderHeaderId, OrderStatus.StatusInProgress);
            _unitOfWork.Save();
            TempData["success"] = "Order Status Updated";
            return RedirectToAction("OrderDetails", "Order", new { id = vm.OrderHeader.OrderHeaderId });
        }
        [Authorize(Roles = WebsiteRole.Role_Admin + "," + WebsiteRole.Role_Employee)]
        public IActionResult Shipped(OrderVM vm)
        {
            var oHeader = _unitOfWork.OrderHeader.GetT(x => x.OrderHeaderId == vm.OrderHeader.OrderHeaderId);
            if (oHeader != null)
            {
                oHeader.Carrier = vm.OrderHeader.Carrier;
                oHeader.TrackingNumber = vm.OrderHeader.TrackingNumber;
                oHeader.OrderStatus = OrderStatus.StatusShipped;
                oHeader.DateOfShipping = DateTime.Now;
                _unitOfWork.OrderHeader.Update(oHeader);
                _unitOfWork.Save();
                TempData["success"] = "Order Status Updated";
                return RedirectToAction("OrderDetails", "Order", new { id = vm.OrderHeader.OrderHeaderId });
            }
            return View(vm);
        }
        [Authorize(Roles = WebsiteRole.Role_Admin + "," + WebsiteRole.Role_Employee)]
        public IActionResult CancelOrder(OrderVM vm)
        {
            var oHeader = _unitOfWork.OrderHeader.GetT(x => x.OrderHeaderId == vm.OrderHeader.OrderHeaderId);
            if (oHeader != null)
            {
                if (oHeader.PaymentStatus == PaymentStatus.StatusApproved)
                {
                    RefundCreateOptions refundOptions = new()
                    {
                        Reason = RefundReasons.RequestedByCustomer,
                        PaymentIntent = oHeader.PaymentIntentId
                    };
                    RefundService refundService = new();
                    Refund refund = refundService.Create(refundOptions);
                    _unitOfWork.OrderHeader.UpdateStatus(vm.OrderHeader.OrderHeaderId, OrderStatus.StatusCancelled, PaymentStatus.StatusReject);
                }
                else
                {
                    _unitOfWork.OrderHeader.UpdateStatus(vm.OrderHeader.OrderHeaderId, OrderStatus.StatusCancelled, PaymentStatus.StatusReject);
                }
                _unitOfWork.Save();
                TempData["success"] = "Order Cancelled";
                return RedirectToAction("OrderDetails", "Order", new { id = vm.OrderHeader.OrderHeaderId });
            }
            return View(vm);
        }
        public IActionResult PayNow(OrderVM vm)
        {
            var orderHeader = _unitOfWork.OrderHeader.GetT(x => x.OrderHeaderId == vm.OrderHeader.OrderHeaderId, includeProperties: "ApplicationUser");
            var orderDetails = _unitOfWork.OrderDetail.GetAll(x => x.OrderHeaderId == vm.OrderHeader.OrderHeaderId, includeProperties: "Product");
            var domain = "https://localhost:7011/";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"Customer/Cart/OrderSuccess?id={vm.OrderHeader.OrderHeaderId}",
                CancelUrl = domain + "Customer/Cart/Index",
            };
            foreach (var itemCarts in orderDetails)
            {
                var lineItemOptions = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(itemCarts.Product.Price * 100),
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
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }
    }
}
