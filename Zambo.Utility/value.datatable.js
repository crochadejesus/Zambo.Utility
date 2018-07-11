

var datePickerOptionsSearch = {
    format: 'YYYY-MM-DD',
    extraFormats: ['DD.MM.YYYY'],
    showClear: true,
    useCurrent: false,
};

(function (window, document) {
    'use strict';
    
    ValuationController.fn = ValuationController.prototype;

    function ValuationController() { }
    window.valuation = new ValuationController;

    ValuationController.fn.version = '1.04.09.2015';

    $(document).ready(function () {

        ValuationController.LoadDatatables();

        $('.Avdassettype').change(changeAssetSubType);

        $('.minrequestcreatedate').datetimepicker(datePickerOptionsSearch).on("dp.change", function (e) {
            //$('.minrequestcreatedate').val(e.date.format("YYYY-MM-DD"));
            $('.maxrequestcreatedate').data("DateTimePicker").minDate(e.date);

        });
        $('.maxrequestcreatedate').datetimepicker(datePickerOptionsSearch).on("dp.change", function (e) {
            //$('.maxrequestcreatedate').val(e.date.format("YYYY-MM-DD"));
            $('.minrequestcreatedate').data("DateTimePicker").maxDate(e.date);
        });

        $('.minrequestduedate').datetimepicker(datePickerOptionsSearch).on("dp.change", function (e) {
            //$('.minrequestduedate').val(e.date.format("YYYY-MM-DD"));
            $('.maxrequestduedate').data("DateTimePicker").minDate(e.date);
        });
        $('.maxrequestduedate').datetimepicker(datePickerOptionsSearch).on("dp.change", function (e) {
            //$('.maxrequestduedate').val(e.date.format("YYYY-MM-DD"));
            $('.minrequestduedate').data("DateTimePicker").maxDate(e.date);
        });

        $('.minapprduedate').datetimepicker(datePickerOptionsSearch).on("dp.change", function (e) {
            //$('.minapprduedate').val(e.date.format("YYYY-MM-DD"));
            $('.maxapprduedate').data("DateTimePicker").minDate(e.date);
        });
        $('.maxapprduedate').datetimepicker(datePickerOptionsSearch).on("dp.change", function (e) {
            //$('.maxapprduedate').val(e.date.format("YYYY-MM-DD"));
            $('.minapprduedate').data("DateTimePicker").maxDate(e.date);
        });

        $(".chosen-select").chosen({
            width: "100%",
        });

        $(document).on('click', '.btn_AvdSearchClean', function () {
            $('.advsearch').val("");
            $(".chosen-select").trigger("chosen:updated");
            changeAssetSubType();
        });
        
        $(document).on('click', '.add-valuation-request', function () {
            window.open("/ValuationRequest/OpenNewRequest", "_self");
        });

    });


    function changeAssetSubType(selVal) {
        var assettypeid = $('.Avdassettype option:selected').val();
        clearDisableSelect('.Avdassetsubtype');
        if (assettypeid != null && assettypeid != '') {
            populateSelect('.Avdassetsubtype', '/Lookup/GetAssetSubType', selVal, { 'assetId': assettypeid });
        }
    }

    function fieldSelected(selector, selectedValue) {
        $(selector).val(selectedValue);
    }

    function fillAdvSearchFields() {
        var value;
        for (var item in getFilters) {
            value = getFilters[item];
            tableFilters[item] = value;
            fieldSelected("#"+item, value);
        }
    }

    var tableFilters = {};
    if ($.isEmptyObject(getFilters) == false) {
        tableFilters = getFilters;
        $("#advancedsearch").show();
        fillAdvSearchFields();
    }

    ValuationController.LoadDatatables = function () {

        $("#requestduedate").datepicker({
            dateFormat: "yy-mm-dd",
            showAnim: 'slideDown',
            buttonImageOnly: false,
            closeText: "Clear"
        });
        
        $("#appraiserduedate").datepicker({
            dateFormat: "yy-mm-dd",
            showAnim: 'slideDown',
            buttonImageOnly: false,
            closeText: "Clear"
        });

        var dom =   "<'row'<'col-sm-12'tr>>" +
                    "<'row'<'col-sm-5'i><'col-sm-7'p>>" + 
                    "<'row'<'col-sm-12'l>>";

        var language = {
            "decimal": "",
            "emptyTable": "No data available in table",
            "info": "Showing _START_ to _END_ of _TOTAL_ entries",
            "infoEmpty": "Showing 0 to 0 of 0 entries",
            "infoFiltered": "(filtered from _MAX_ total entries)",
            "infoPostFix": "",
            "thousands": ",",
            "lengthMenu": "_MENU_ records per page",
            "loadingRecords": "Loading...",
            "processing": "Processing...",
            "search": "Search:",
            "zeroRecords": "No matching records found",
            "paginate": {
                "first": "First",
                "last": "Last",
                "next": "Next",
                "previous": "Previous"
            },
            "aria": {
                "sortAscending": ": Sort Ascending",
                "sortDescending": ": Sort Descending"
            }
        };

        var table = $('#datagrid').DataTable({
            responsive: true,
            language: language,
            orderCellsTop: true,
            initComplete: function () { $("#datagrid_filter").detach().appendTo('#new-search-area'); },
            dom: dom,
            lengthMenu: [[10, 20, 30, 40, 50], [10, 20, 30, 40, 50]],
            processing: true, // for show progress bar
            serverSide: true, // for process server side
            stateSave: false,
            sort: true,
            ajax: {
                url: "/Valuation/GetRequests",
                type: "POST",
                datatype: "json",
                cache: "false",
                data: function (d) { d.filters = tableFilters }
            },
            columnDefs: [
                { targets: 0, width: "5px", responsivePriority: 1 },
                { targets: -1, width: "5px", responsivePriority: 1 },
                { targets: [2, 3, 4], width: "100px" },
                { targets: 5, width: "120px" },
                { targets: [1, 6, 7, 8, 9, 10], width: "180px" }
            ],
            columns: getColumns(),
            order: [[0, "asc"]]
        });

        
        table.on('responsive-resize.dt', function (e, datatable, columns) {
            columns.forEach(function (is_visible, index) {
                var filterTh = $('th.value-dt-filter[data-index="' + index + '"]');
                console.log(filterTh);
                is_visible == true ? filterTh.show() : filterTh.hide();
            });
        });        
        
        //Pesquisa via Status e Search
        $('#Status').on('change', function () {
            table.search(this.options[this.selectedIndex].text).draw();
        });
        $('#txt_Search').on('blur', function () {
            table.search(this.value).draw();
        });
        $('#txt_Search').keypress(function (e) {
            var keycode = e.keyCode || e.which;
            if (keycode == 13) table.search(this.value).draw();
        });

        //Pesquisa por colunas da Datatable
        $(document).on("keydown", ".value-dt-filter > input", function (event) {
            var keycode = event.keyCode || event.which;
            if (keycode == '13') { $(this).trigger('blur'); }
        });
        $(document).on("change", ".value-dt-filter > input, .value-dt-filter > select", function () {
            setFilters();
            table.ajax.reload();
        });

        function setFilters() {
            $('.value-dt-filter > input, .value-dt-filter > select').each(function () {
                tableFilters[$(this).attr('name')] = $(this).val();
            });
        }

        $(document).on('click', '.btn_AvdSearch', function () {
            setFiltersAdvance();
            table.ajax.reload();
        });

        function setFiltersAdvance() {
            $('.advsearch').each(function () {
                tableFilters[$(this).attr('name')] = $(this).val();
            });
        }


    }


    ValuationController.fn.ShowHideSearchDiv = function() {
        (document.all.advancedsearch.style.display == 'none') ? $('#advancedsearch').fadeIn() : $('#advancedsearch').hide();
    }

    return ValuationController;

})(window, document);


function getColumns() {
    if (ShowDefaultGrid) {
        return [
            { "data": "RequestId", "name": "RequestId", "className": "dt-id", "orderable": true },
            { "data": "SourceReference", "name": "SourceReference", "orderable": true },
            { "data": "AssetType", "name": "AssetType", "orderable": true },
            { "data": "ValuationMethod", "name": "ValuationMethod", "orderable": true },
            { "data": "WorkflowStatusName", "name": "WorkflowStatusName", "orderable": true },
            { "data": "RequestDueDateString", "name": "RequestDueDateString", "orderable": true },
            { "data": "TechnicianUserName", "name": "TechnicianUserName", "orderable": true },
            { "data": "AppraiserUserName", "name": "AppraiserUserName", "orderable": true },
            { "data": "AppraiserCompanyName", "name": "AppraiserCompanyName", "orderable": true },
            { "data": "AppraiserDueDateString", "name": "AppraiserDueDateString", "orderable": true, "visible": false },
            { "data": "CountyName", "name": "CountyName", "orderable": true, "visible": false },
            {
                "orderable": false,
                "render": function (data, type, full, meta) {
                    return renderActions(full);
                }
            }];
    }
    else {
        if (ShowAppraiserGrid) {
            return [
                { "data": "RequestId", "name": "RequestId", "className": "dt-id", "orderable": true },
                { "data": "SourceReference", "name": "SourceReference", "orderable": true },
                { "data": "AssetType", "name": "AssetType", "orderable": true },
                { "data": "ValuationMethod", "name": "ValuationMethod", "orderable": true },
                { "data": "WorkflowStatusName", "name": "WorkflowStatusName", "orderable": true },
                { "data": "RequestDueDateString", "name": "RequestDueDateString", "orderable": true },
                { "data": "TechnicianUserName", "name": "TechnicianUserName", "orderable": true },
                { "data": "AppraiserUserName", "name": "AppraiserUserName", "orderable": true, "visible": false },
                { "data": "AppraiserCompanyName", "name": "AppraiserCompanyName", "orderable": true, "visible": false },
                { "data": "AppraiserDueDateString", "name": "AppraiserDueDateString", "orderable": true },
                { "data": "CountyName", "name": "CountyName", "orderable": true },
                {
                    "orderable": false,
                    "render": function (data, type, full, meta) {
                        return renderActions(full);
                    }
                }];
        }
    }
}

function renderActions(full) {
    var button = "";
    switch (full.WorkflowStatusID) {
        case 1:
        case 2:
        case 112: button = '<a href="/ValuationRequest/OpenUpdateRequest?id=' + full.RequestId + '" class="btn btn-dt wsi-red" data-toggle="tooltip" data-placement="top" title="Edit Request" ><i class="fa fa-edit"></i>'; break;
        default: button = '<a href="/ValuationRequest/Index?reqID=' + full.RequestId + '" class="btn btn-dt wsi-red" data-toggle="tooltip" data-placement="top" title="Edit Valuation" ><i class="fa fa-folder-open"></i>'; break;
    }
    return button;
}
