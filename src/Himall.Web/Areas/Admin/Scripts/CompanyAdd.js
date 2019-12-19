$(function () {

    $("#area-selector").RegionSelector({
        selectClass: "form-control input-sm select-sort",
        valueHidden: "#RegionId"
    });
})

var a = v({
    form: 'v-form',// 表单id 必须
    beforeSubmit: function () {
        if ($("div.tip-error span").length === 0) {
            loadingobj = showLoading();
        }
    },// 表单提交之前的回调 不是必须
    afterSubmit: function (data) {
        if (data.success) {
            // a.reset();
            $.dialog.succeedTips(data.msg, function () { window.location.href = Management; });

        } else {
            $.dialog.errorTips(data.msg);
        }
        loadingobj.close();
    },// 表单提交之后的回调 不是必须
    ajaxSubmit: true// 是否ajax提交 如果没有这个参数那么就是默认提交方式 如果没有特殊情况建议默认提交方式
});
a.add(
    {
        target: "Name",
        ruleType: "required",
        error: '公司名称必须填写!'
    },
    {
        target: "Code",
        ruleType: "required",
        error: '公司编号必须填写!'
    },
    {
        target: "Contacts",
        ruleType: "required",
        error: '联系人必须填写!'
    },
    {
        target: "Phone",
        ruleType: "mobile||phone",
        error: '请输入正确的电话号码!'
    },
    {
        target: "RegionId",
        ruleType: "required",
        error: '公司地址必须选择!'
    },
    {
        target: "AddressDetail",
        ruleType: "required",
        error: '地址详情必须填写!'
    },

);
