using CoreMVCProject.DataAccessLayer.Infrastructure.IRepository;
using CoreMVCProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreMVCProject.DataAccessLayer.Infrastructure.Respository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private readonly ApplicationDBContext _context;
        public OrderHeaderRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }

        public void PaymentStatus(int id, string sessionId, string paymentIntentId)
        {
            var order = _context.OrderHeaders.FirstOrDefault(x => x.OrderHeaderId == id);
            if (order != null)
            {
                order.DateOfPayment = DateTime.Now;
                order.PaymentIntentId = paymentIntentId;
                order.SessionId = sessionId;
            }
        }

        public void Update(OrderHeader orderHeader)
        {
            _context.OrderHeaders.Update(orderHeader);
        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = "")
        {
            var order = _context.OrderHeaders.FirstOrDefault(x => x.OrderHeaderId == id);
            if (order != null)
            {
                order.OrderStatus = orderStatus;
                if (paymentStatus !="")
                {
                    order.PaymentStatus = paymentStatus;
                }
            }
        }
    }
}
