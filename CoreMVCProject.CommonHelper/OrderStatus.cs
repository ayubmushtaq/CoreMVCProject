using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreMVCProject.CommonHelper
{
    public class OrderStatus
    {
        public const string StatusPending = "Pending";
        public const string StatusRefund = "Refund";
        public const string StatusApproved = "Approved";
        public const string StatusCancelled = "Cancelled";
        public const string StatusInProgress = "UnderProgress";
        public const string StatusShipped = "Shipped";
    }
}
