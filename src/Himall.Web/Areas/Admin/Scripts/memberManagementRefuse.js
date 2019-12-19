$(function () {
    GetCompany()
    $("#company").on('change', function () {
        if ($("#company").val()) {
            GetCompanyDep($("#company").val());
        }
    });

    query();
    $("#searchBtn").click(function () { query(); });
    AutoComplete();
})
$(function () {
    $("#inputStartDateLogin,#inputStartDate").datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd',
        autoclose: true,
        weekStart: 1,
        minView: 2
    });
    $("#inputEndDateLogin,#inputEndDate").datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd',
        autoclose: true,
        weekStart: 1,
        minView: 2
    });
    $("#inputStartDate").on('changeDate', function () {
        if ($("#inputEndDate").val()) {
            if ($("#inputStartDate").val() > $("#inputEndDate").val()) {
                $("#inputEndDate").val($("#inputStartDate").val());
            }
        }
        $("#inputEndDate").datetimepicker('setStartDate', $("#inputStartDate").val());
    });
    $("#inputStartDateLogin").on('changeDate', function () {
        if ($("#inputEndDateLogin").val()) {
            if ($("#inputStartDateLogin").val() > $("#inputEndDateLogin").val()) {
                $("#inputEndDateLogin").val($("#inputStartDateLogin").val());
            }
        }

        $("#inputEndDateLogin").datetimepicker('setStartDate', $("#inputStartDateLogin").val());
    });
    $('#btnAdvanceSearch').click(function () {
        $('#divAdvanceSearch').toggle();

        if ($(this).hasClass('menu-shrink')) {
            $(this).removeClass("menu-shrink").addClass("up-shrink")
        } else {
            $(this).addClass("menu-shrink").removeClass("up-shrink")
        }
    });
});

function BatchLock() {
    var selectedRows = $("#list").hiMallDatagrid("getSelections");


    if (selectedRows.length == 0) {
        $.dialog.tips("你没有选择任何选项！");
    }
    else {
        var selectids = new Array();
        for (var i = 0; i < selectedRows.length; i++) {
            selectids.push(selectedRows[i].Id);
        }
        $.dialog.confirm('确定冻结选择的用户吗？', function () {
            var loading = showLoading();
            $.post("./BatchLock", { ids: selectids.join(',') }, function (data) { $.dialog.tips(data.msg); query(); loading.close(); });
        });
    }
}
$(function () {
    $("#divSetLabel .form-group").css({ "width": "150px", "float": "left", "border": "none", "white-space": "nowrap", "overflow": "hidden", "margin": "10px" });
});

function Show(id) {
    var str = '';
    var loading = showLoading();
    $.ajax({
        type: "post",
        async: true,
        dataType: "html",
        url: '/Admin/member/Detail',
        data: { Id: id },
        success: function (data) {
            str = data;
            $.dialog({
                title: '会员信息',
                lock: true,
                id: 'ChangePwd',
                width: '400px',
                content: str,
                padding: '0 40px',
                okVal: '确定',
                ok: function () {
                }
            });
            loading.close();
        }
    });
}

function query() {
    $("#list").hiMallDatagrid({
        url: './RefuseList',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "Id",
        pageSize: 10,
        pageNumber: 1,
        queryParams: getQueryParams(),
        toolbar: /*"#goods-datagrid-toolbar",*/'',
        operationButtons: "#batchOperate",
        columns:
        [[
            { field: "Id", hidden: true },
            { field: "UserName", title: '会员名' },
            { field: "RealName", title: '真实姓名' },
            { field: "Code", title: '工号' },
            { field: "CompanyName", title: '公司' },
            { field: "DepName", title: '部门' },
            { field: "CellPhone", title: '手机' },
            { field: "CreateDateStr", title: '注册日期' },
            { field: "ReviewDateStr", title: '审核日期' },
            //{
            //    field: "operation", operation: true, title: "操作",
            //    formatter: function (value, row, index) {
            //        var id = row.Id.toString();
            //        var html = ["<span class=\"btn-a\">"];
            //        html.push("<a onclick=\"Pass('" + id + "');\">通过</a>");
            //        html.push("<a onclick=\"Refuse('" + id + "');\">拒绝</a>");
            //        html.push("</span>");
            //        return html.join("");
            //    }
            //}
        ]]
    });
}

function AutoComplete() {
    //autocomplete
    $('#autoTextBox').autocomplete({
        source: function (query, process) {
            var matchCount = this.options.items;//返回结果集最大数量
            $.post("./getMembers", { "keyWords": $('#autoTextBox').val() }, function (respData) {
                return process(respData);
            });
        },
        formatItem: function (item) {
            return item.value;
        },
        setValue: function (item) {
            return { 'data-value': item.value, 'real-value': item.key };
        }
    });
}
function getQueryParams() {
    var rtstart, rtend, ltstart, ltend, isseller, isfocus;
    if ($('#divAdvanceSearch').css('display') != 'none') {
        rtstart = $("#inputStartDate").val();
        rtend = $("#inputEndDate").val();
        ltstart = $("#inputStartDateLogin").val();
        ltend = $("#inputEndDateLogin").val();

        isseller = $("#isrzseller").val();
        isfocus = $("#iswxfocus").val();
    }
    var mobile = $("#mobile").val();
    var weChatNick = $("#weChatNick").val();
    var companyId = $("#company").val();
    var depId = $("#dep").val();


    var result = {
        keyWords: $("#autoTextBox").val()
    };
    if (isseller && isseller.length > 0) {
        result.isSeller = isseller;
    }
    if (isfocus && isfocus.length > 0) {
        result.isFocus = isfocus;
    }
    if (rtstart && rtstart.length > 0) {
        result.regtimeStart = rtstart;
    }
    if (rtend && rtend.length > 0) {
        result.regtimeEnd = rtend;
    }
    if (mobile && mobile.length > 0) {
        result.mobile = mobile;
    }
    if (weChatNick && weChatNick.length > 0) {
        result.weChatNick = weChatNick;
    }
    result.companyId = companyId;
    result.depId = depId;
    return result;
}
function ExportExecl() {
    var url = "/Admin/Member/ExportToExcelRefuse?";
    var urldata = getQueryParams();
    var ltstart, ltend;
    if ($('#divAdvanceSearch').css('display') != 'none') {
        ltstart = $("#inputStartDateLogin").val();
        ltend = $("#inputEndDateLogin").val();
        if (ltstart && ltstart.length > 0) {
            urldata.logintimeStart = ltstart;
        }
        if (ltstart && ltstart.length > 0) {
            urldata.logintimeEnd = ltend;
        }
    }

    for (var item in urldata) {
        var itemdata = urldata[item];
        if (itemdata) {
            url += item + "=" + itemdata + "&";
        }
    }
    $("#aExport").attr("href", url);
}
function GetCompany(id) {

    $.post('/admin/Company/GetCompanySelector', {}, function (data) {
        if (!data || data.length == 0) {
            return;
        }
        var selector = '';
        $.each(data, function (i, item) {
            var selstr = "";
            if (id && item.Id == id)
                selstr = "selected=selected";
            selector += '<option value="' + item.Id + '" ' + selstr + ' >' + item.Name + '</option>';
        });
        $("#company").append(selector);
    });
}
function GetCompanyDep(companyId, id) {

    $.post('/admin/Company/GetCompanyDepSelector', { companyId: companyId }, function (data) {
        if (!data || data.length == 0) {
            return;
        }
        var selector = '';
        $.each(data, function (i, item) {
            var selstr = "";
            if (id && item.Id == id)
                selstr = "selected=selected";
            selector += '<option value="' + item.Id + '" ' + selstr + ' >' + item.Name + '</option>';
        });
        $("#dep").append(selector);
    });
}