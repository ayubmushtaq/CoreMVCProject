var orderTable;
$(document).ready(function () {
    var url = window.location.search;
    if (url.includes("pending")) {
        orderTable = OrderTable("pending");
    }
    else if (url.includes("approved")) {
        orderTable = OrderTable("approved");
    }
    else if (url.includes("underprocess")) {
        orderTable = OrderTable("underprocess");
    }
    else if (url.includes("shipped")) {
        orderTable = OrderTable("shipped");
    }
    else {
        orderTable = OrderTable();
    }
});
function OrderTable(status) {
    var url = "Order/AllOrders";
    if (status) {
        url = url + "?status=" + status;
    }
    $('#OrderTable').DataTable({
        "ajax": {
            "url": url
        },
        "columns": [
            { "data": "name" },
            { "data": "phone" },
            { "data": "orderStatus" },
            { "data": "orderTotal" },
            {
                "data": "orderHeaderId", "render": function (data) {
                    return `<a href="Order/OrderDetails?id=${data}"><i class="bi bi-pencil-square"></i></a>`
                }
            }
        ]
    });
}