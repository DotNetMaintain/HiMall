$(function () {
    GetData();
    $('#searchButton').click(function () {
        GetData();
    });

    $("body").on("change", "input[name='ridOperateMoney']", function () {
    	if(this.checked){
    		var _t = $(this);
    		_t.parent().parent().find('input').focus();
    		var val = _t.val();
    		if (val == 1) {
    		    $("#txtSubMoney").val("");
    		} else {
    		    $("#txtAddMoney").val("");
    		}
    	}
    });
    $("body").on("click", "input.txtOpMoney", function () {
    	var radioEl=$(this).siblings().find('input');
    	if(!radioEl[0].checked){
    	    radioEl.click();
    	    var val = radioEl.val();
    	    if (val == 1) {
    	        $("#txtSubMoney").val("");
    	    } else {
    	        $("#txtAddMoney").val("");
    	    }
    	}
    	
    });
})

function GetData() {

    var dataColumn = [];

    dataColumn.push({ field: "UserCode", title: '会员帐号', width: 120 });
    dataColumn.push({ field: "UserName", title: '会员姓名', width: 120 });
    dataColumn.push({
        field: "Balance", title: '账户可用金额', width: 100, align: 'center'
    });
    dataColumn.push({
        field: "FreezeAmount", title: '冻结金额', width: 100, align: 'center'
    });
    dataColumn.push({
        field: "ChargeAmount", title: "累计充值金额", width: 100, align: "center"
    });
    dataColumn.push({
        field: "operate", title: "操作", width: 140, align: "center",
        formatter: function (value, row, index) {
            var id = row['UserId'].toString();
            var html = ["<span class=\"btn-a\">"];
            html.push("<a href='./Detail/" + id + "'>查看明细</a>");
            html.push("<a href='javascript:ShowCapitalOperateDlg(" + id + ");'>加减款</a>");
            html.push("</span>");
            return html.join("");
        }
    });

    var url = 'GetMemberCapitals';
    var username = $.trim($('#txtMemName').val());
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
        queryParams: { user: username },
        columns: [dataColumn]
    });
}

function ShowCapitalOperateDlg(id) {
    var datas = $("#list").hiMallDatagrid('getRows');
    var data = null;
    for (var i = 0; i < datas.length; i++) {
        if (datas[i].UserId == id) {
            data = datas[i];
            break;
        }
    }

    $.dialog({
        title: '加减款',
        width: 466,
        lock: true,
        id: 'operatecapital',
        content: ['<div class="dialog-form">',
            '<div class="form-group">',
            '<label class="label-inline fl">会员账号：</label>',
            '<p>' + data.UserName + '</p>',
            '</div>',
            '<div class="form-group">',
            '<label class="label-inline fl">可用余额：</label>',
            '<p >' + data.Balance + '元</p>',
            '</div>',
            '<div class="form-group">',
            '<label class="label-inline fl">金额变动：</label>',
            '<div ><label><input type="radio" name="ridOperateMoney" value="1">加款 </label> <input type="text" class="form-control input-ssm txtOpMoney" id="txtAddMoney" name="txtAddMoney"> 元</div> ',
            '</div>',
            '<div class="form-group">',
            '<label class="label-inline fl">　　　　　</label>',
            '<div ><label><input type="radio" name="ridOperateMoney" value="0">减款 </label> <input type="text" class="form-control input-ssm txtOpMoney" id="txtSubMoney" name="txtSubMoney"> 元</div>',
            '</div>',
            '<div class="form-group">',
            '<label class="label-inline fl">备注：</label>',
            '<p><textarea class="form-control" type="text" name="txtRemark" id="txtRemark" rows="3"/></textarea></p>',
            '</div>',
            '<div class="form-group">',
            '<span class="field-validation-error" id="txtErrorTip"></span> ',
            '</div>',
            '</div>'].join(''),
        padding: '0 40px',
        button: [
            {
                name: '确定',
                callback: function () {
                    var remark = $("#txtRemark").val();
                    if (remark.length < 1 || remark.length > 200) {
                        $("#txtErrorTip").text("备注信息在1-200之间");
                        $("#txtRemark").css({ border: '1px solid #f60' });
                        return false;
                    }
                    var isadd = $("input[name='ridOperateMoney']:checked").val() == "1";
                    var _opd = $("input[name='ridOperateMoney']:checked").parent().siblings(".txtOpMoney");
                    $(".txtOpMoney").css({ border: '' });
                    var opmoney = parseFloat(_opd.val());
                    if (isNaN(opmoney) || opmoney <= 0) {
                        $("#txtErrorTip").text("请输入正确的金额！");
                        _opd.css({ border: '1px solid #f60' });
                        return false;
                    }
                    if (!isadd) {
                        opmoney = -opmoney;
                    }
                    var loading = showLoading();
                    $.ajax({
                        type: "post",
                        url: "/Admin/Capital/ChageCapital",
                        data: { userId: id, amount: opmoney, remark: remark },
                        async: false,
                        success: function (data) {
                            loading.close();
                            if (!data.success) {
                                $.dialog.errorTips(data.msg);
                            }
                            else {
                                $.dialog({ id: 'operatecapital' }).close();
                                $.dialog.succeedTips('操作成功！');
                                var pageNo = $("#list").hiMallDatagrid('options').pageNumber;
                                $("#list").hiMallDatagrid('reload', { pageNumber: pageNo });
                            }
                        }
                    });
                    return false;
                },
                focus: true
            }, { name: "取消" }]
    });
}