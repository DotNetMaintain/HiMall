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

})

function GetData() {

    var dataColumn = [];

    dataColumn.push({ field: "CompanyCode", title: '公司编码', width: 50 });
    dataColumn.push({ field: "CompanyName", title: '公司名称', width: 120 });
    dataColumn.push({
        field: "Code", title: '部门编码', width: 50,
        formatter: function (value, row, index) {
            var Code = row['Code'].toString();
            var id = row['Id'].toString();
            return "<span id='code_" + id + "'>" + Code + "</span>";
        } });
    dataColumn.push({
        field: "Name", title: '部门名称', width: 120,
        formatter: function (value, row, index) {
            var Name = row['Name'].toString();
            var id = row['Id'].toString();
            return "<span id='name_" + id + "'>" + Name + "</span>";
        }});
    dataColumn.push({
        field: "MemberNum", title: '员工', width: 100,
        formatter: function (value, row, index) {
            var id = row['Id'].toString();
            var CompanyId = row['CompanyId'].toString();
            var MemberNum = row['MemberNum'].toString();
            var MemberNumAuditing = row['MemberNumAuditing'].toString();
            return "<a href='/Admin/member/management?companyId=" + CompanyId + "&depId=" + id + "' title='点击查看详情&#13;待审核：" + MemberNumAuditing + "人'>" + MemberNum + "</a>";
        }
    });
    dataColumn.push({ field: "CreateDate", title: '创建时间', width: 100 });

    dataColumn.push({
        field: "operate", title: "操作", width: 100, align: "center",
        formatter: function (value, row, index) {
            var id = row['Id'].toString();
            var MemberNum = row['MemberNum'].toString();
            var MemberNumAuditing = row['MemberNumAuditing'].toString();
            var isCan = (parseInt(MemberNum) + parseInt(MemberNumAuditing)) > 0 ? "0" : "1";
            var html = ["<span class=\"btn-a\">"];
            html.push("<a href='javascript:EditDep(" + id + ")'>编辑</a>");
            html.push("<a href='javascript:Delete(" + id + "," + isCan + ");'>删除</a>");
            html.push("</span>");
            return html.join("");
        }
    });

    var url = '/admin/Company/GetCompanyDepInfos';
    var companyCode = $.trim($('#txtCompanyCode').val());
    var companyName = $.trim($('#txtCompanyName').val());
    var code = $.trim($('#txtCode').val());
    var name = $.trim($('#txtName').val());

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
        operationButtons: "#orderOperate",
        queryParams: { companyName: companyName, companyCode: companyCode, name: name, code: code, startDate: startDate, endDate: endDate },
        columns: [dataColumn]
    });
}

function Delete(id, iscan) {
    if (iscan == 1) {
        $.dialog.confirm('确认删除该部门吗？', function () {
            var loading = showLoading();
            $.post("/admin/Company/DeleteDep/" + id, function (data) {
                $.dialog.succeedTips(data.msg);
                if (data.success) {
                    GetData();
                }
                loading.close();
            });
        })
    } else {
        $.dialog.errorTips("已有人员注册审核通过或审核中的部门不可删除！");
    }
}

function AddDep() {
    $.post('/admin/Company/GetCompanySelector', {}, function (data) {
        if (!data || data.length == 0) {
            $.dialog.errorTips("您还未添加任何公司，请先添加公司后再新增部门！");
            return;
        }

        var selector = '<p><select id="CompanyCategory" class="form-control input-sm"><option value="0" >请选择公司</option>';

        $.each(data, function (i, item) {
            var selstr = "";
            if (item.Id == parseInt($("#companyId").val()))
                selstr = "selected=selected";
            selector += '<option value="' + item.Id + '" ' + selstr + ' >' + item.Name + '</option>';
        });
        selector += '</select></p>';
        $.dialog({
            title: '新增部门',
            lock: true,
            id: 'AddDep',
            content: ['<div class="dialog-form">',
                '<div class="form-group" >',
                '<label class="label-inline fl" for="">所属公司</label>',
                selector,
                '</div>',
                '<div class="form-group">',
                '<label class="label-inline fl" for="">部门名称</label><input class="form-control input-sm" maxlength="20" type="text" id="DepName" ><p class="help-block">不能多于20个字</span></p>',
                '</div>',
                '<div class="form-group">',
                '<label class="label-inline fl" for="">部门编码</label><input class="form-control input-sm" maxlength="20" type="text" id="DepCode" ><p class="help-block">不能多于20个字符</span></p>',
                '</div>',
                '</div>'].join(''),
            padding: '0 40px',
            init: function () { $("#DepName").focus(); },
            okVal: '保存',
            ok: function () {
                var DepName = $.trim($('#DepName').val());
                var DepCode = $.trim($('#DepCode').val());
                var CompanyId = $('#CompanyCategory').val();

                if (!DepName) {
                    $.dialog.tips('请输入部门名称');
                    return false;
                } else if (!DepCode) {
                    $.dialog.tips('请输入部门编码');
                    return false;
                } else if (!CompanyId || CompanyId == "0") {
                    $.dialog.tips('请选择所属公司');
                    return false;
                }
                else {
                    var loading = showLoading();
                    var params = {};
                    params.DepName = DepName;
                    params.DepCode = DepCode;
                    params.CompanyId = CompanyId;

                    $.post('/admin/Company/AddDep', params, function (result) {
                        loading.close();
                        if (result.success) {
                            $.dialog.succeedTips('添加成功', function () {
                                GetData();
                            });
                        }
                        else
                            $.dialog.errorTips('添加失败！' + result.msg);
                    });
                }
            }
        });
    });
}

function EditDep(id) {
    $.post('/admin/Company/GetDepInfo', { Id: id }, function (data) {
        if (!data) {
            $.dialog.errorTips("获取部门信息失败，请刷新后重试！");
            return;
        }
        $.dialog({
            title: id ? '新增部门' : '编辑部门',
            lock: true,
            id: 'editDep',
            content: ['<div class="dialog-form">',
                '<div class="form-group">',
                '<label class="label-inline fl" for="">部门名称</label><input class="form-control input-sm" maxlength="20" type="text" id="DepName" value="' + data.Name + '" ><p class="help-block">不能多于20个字</span></p>',
                '</div>',
                '<div class="form-group">',
                '<label class="label-inline fl" for="">部门编码</label><input class="form-control input-sm" maxlength="20" type="text" id="DepCode" value="' + data.Code + '" ><p class="help-block">不能多于20个字符</span></p>',
                '</div>',
                '</div>'].join(''),
            padding: '0 40px',
            init: function () { $("#DepName").focus(); },
            okVal: '保存',
            ok: function () {
                var DepName = $.trim($('#DepName').val());
                var DepCode = $.trim($('#DepCode').val());

                if (!DepName) {
                    $.dialog.tips('请输入部门名称');
                    return false;
                } else if (!DepCode) {
                    $.dialog.tips('请输入部门编码');
                    return false;
                }
                else {
                    var loading = showLoading();
                    var params = {};
                    params.Id = id;
                    params.DepName = DepName;
                    params.DepCode = DepCode;

                    $.post('/admin/Company/EditDep', params, function (result) {
                        loading.close();
                        if (result.success) {
                            $.dialog.succeedTips('保存成功');
                            $("#code_" + id).html(DepCode);
                            $("#name_" + id).html(DepName);
                        }
                        else
                            $.dialog.errorTips('保存失败！' + result.msg);

                    });
                }
            }
        });
    });
}
