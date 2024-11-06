﻿var dataTable;
$(document).ready(function () {
    cargarDataTable();
});
function cargarDataTable() {
    dataTable = $("#tblCategoriasCoach").DataTable({
        responsive: {
            details: {
                display: $.fn.dataTable.Responsive.display.childRow
            }
        },
        "ajax": {
            "url": "/Admin/Coach/GetAll",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            {
                "data": "name",
                "responsivePriority": 2
            },
            {
                "data": "lastName",
                "responsivePriority": 3
            },
            {
                "data": "secondLastName",
                "responsivePriority": 4
            },
            {
                "data": "imageUrl",
                "render": function (imagen) {
                    return `<img src="../${imagen}" width="100" class="img-fluid"/>`
                },
                "responsivePriority": 5
            },
            {
                "data": "id",
                "render": function (data) {
                    return `<div class="d-flex justify-content-center gap-2">
                                <a href="/Admin/Coach/Edit/${data}" class="btn btn-success btn-sm text-white">
                                    <i class="far fa-edit"></i> Editar
                                </a>
                                <a onclick=Delete("/Admin/Coach/Delete/${data}") class="btn btn-danger btn-sm text-white">
                                    <i class="far fa-trash-alt"></i> Borrar
                                </a>
                            </div>`;
                },
                "responsivePriority": 1
            }
        ],
        "language": {
            "decimal": "",
            "emptyTable": "No hay registros",
            "info": "Mostrando _START_ a _END_ de _TOTAL_ Entradas",
            "infoEmpty": "Mostrando 0 to 0 of 0 Entradas",
            "infoFiltered": "(Filtrado de _MAX_ total entradas)",
            "infoPostFix": "",
            "thousands": ",",
            "lengthMenu": "Mostrar _MENU_ Entradas",
            "loadingRecords": "Cargando...",
            "processing": "Procesando...",
            "search": "Buscar:",
            "zeroRecords": "Sin resultados encontrados",
            "paginate": {
                "first": "Primero",
                "last": "Ultimo",
                "next": "Siguiente",
                "previous": "Anterior"
            }
        },
        "order": [[0, "desc"]],
        "pageLength": 10,
        "lengthMenu": [[5, 10, 25, 50, -1], [5, 10, 25, 50, "Todos"]],
        "width": "100%"
    });
}
function Delete(url) {
    swal({
        title: "¿Está seguro de borrar?",
        text: "¡Este contenido no se puede recuperar!",
        type: "warning",
        showCancelButton: true,
        confirmButtonColor: "#DD6B55",
        confirmButtonText: "¡Sí, borrar!",
        closeOnconfirm: true
    }, function () {
        $.ajax({
            type: 'DELETE',
            url: url,
            success: function (data) {
                if (data.success) {
                    toastr.success(data.message);
                    dataTable.ajax.reload();
                }
                else {
                    toastr.error(data.message);
                }
            }
        });
    });
}