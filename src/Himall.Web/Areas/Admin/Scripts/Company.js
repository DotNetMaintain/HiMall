$(function () {
    GetData();
    $('#searchButton').click(function () {
        GetData();
    });

    $(".start_datetime").datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd',
        autoclose: true,
        weekStart: 1,
        minView: 2
    });
    $(".end_datetime").datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd',
        autoclose: true,
        weekStart: 1,
        minView: 2
    });

    $('.start_datetime').on('changeDate', function () {
        if ($(".end_datetime").val()) {
            if ($(".start_datetime").val() > $(".end_datetime").val()) {
                $('.end_datetime').val($(".start_datetime").val());
            }
        }

        $('.end_datetime').datetimepicker('setStartDate', $(".start_datetime").val());
    });

    $('.start_datetime,.end_datetime').keydown(function (e) {
        e = e || window.event;
        var k = e.keyCode || e.which;
        if (k != 8 && k != 46) {
            return false;
        }
    });
    $("#area-selector").RegionSelector({
        selectClass: "form-control input-sm select-sort",
        valueHidden: "#AddressId"
    });
})

function GetData() {

    var dataColumn = [];

    dataColumn.push({ field: "Code", title: '公司编码', width: 50 });
    dataColumn.push({ field: "Name", title: '公司名称', width: 120 });
    dataColumn.push({ field: "Contacts", title: '联系人(电话)', width: 120 });
    dataColumn.push({ field: "Address", title: '公司地址', width: 120 });
    dataColumn.push({
        field: "DepNum", title: '部门/员工', width: 100,
        formatter: function (value, row, index) {
            var id = row['Id'].toString();
            var DepNum = row['DepNum'].toString();
            var MemberNum = row['MemberNum'].toString();
            var MemberNumAuditing = row['MemberNumAuditing'].toString();
            return "<a href='/admin/company/dep/" + id + "' title='点击查看详情'>" + DepNum + "</a>/" + "<a href='/Admin/member/management?companyId=" + id + "' title='点击查看详情&#13;待审核：" + MemberNumAuditing+"人'>" + MemberNum + "</a>";
        } });
    dataColumn.push({ field: "CreateDate", title: '创建时间', width: 100 });

    dataColumn.push({
        field: "operate", title: "操作", width: 100, align: "center",
        formatter: function (value, row, index) {
            var id = row['Id'].toString();
            var MemberNum = row['MemberNum'].toString();
            var MemberNumAuditing = row['MemberNumAuditing'].toString();
            var isCan = (parseInt(MemberNum) + parseInt(MemberNumAuditing)) > 0 ? "0" : "1";
            var html = ["<span class=\"btn-a\">"];
            html.push("<a href='/admin/company/Edit/" + id + "'>编辑</a>");
            html.push("<a href='javascript:Delete(this," + id + "," + isCan + ");'>删除</a>");
            html.push("</span>");
            return html.join("");
        }
    });

    var url = '/admin/Company/GetCompanyInfos';
    var code = $.trim($('#txtCode').val());
    var name = $.trim($('#txtName').val());
    var contacts = $.trim($('#txtContacts').val());
    var phone = $.trim($('#txtPhone').val());
    var addressId = $.trim($('#AddressId').val());
    var startDate = $.trim($("#inputStartDate").val());
    var endDate = $.trim($("#inputEndDate").val());


    $("#list").empty();
    $("#list").hiMallDatagrid({
        url: url,
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "id",
        pageSize: 15,
        pagePosition: 'bottom',
        pageNumber: 1,
        queryParams: { name: name, code: code, contacts: contacts, phone: phone, addressId: addressId, startDate: startDate, endDate: endDate },
        columns: [dataColumn]
    });
}

function Delete(t, id, iscan) {
    if (iscan == 1) {
        $.dialog.confirm('确认删除该公司吗？', function () {
            var loading = showLoading();
            $.post("/admin/Company/Delete/" + id, function (data) {
                $.dialog.succeedTips(data.msg);
                if (data.success) {
                    GetData();
                }
                loading.close();
            });
        })
    } else {
        $.dialog.errorTips("已有人员注册审核通过或审核中的公司不可删除！");
    }
}