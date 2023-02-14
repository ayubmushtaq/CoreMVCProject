var productTable;
$(document).ready(function () {
    productTable = $('#ProductTable').DataTable({
        "ajax": {
            "url": "Product/AllProducts"
        },
        "columns": [
            { "data": "name" },
            { "data": "description" },
            { "data": "price" },
            { "data": "category.name" },
            {
                "data": "id", "render": function (data) {
                    return `<a href="Product/CreateUpdate?id=${data}"><i class="bi bi-pencil-square"></i></a><a onclick=RemoveProduct("Product/Delete/${data}")><i class="bi bi-trash3"></i></a>`
                }
            }
        ]
    });
});

function RemoveProduct(URL) {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: URL,
                type: 'Delete',
                success: function (data) {
                    if (data.success) {
                        productTable.ajax.reload();
                        toastr.success(data.message);
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            })
        }
    })
}