﻿
$(function () {
    var stae1, stae2, stae3;
    var pwdErrMsg = '密码不能为空！';
    $('#firstPwd').blur(function () {
        var d = $(this).val();
        if (d.length < 6) {
            $('#firstPwd').css({ borderColor: '#f60' });
            pwdErrMsg = '密码长度不能少于6位';
            stae2 = '';
        } else {
            $('#firstPwd').css({ borderColor: '#ccc' });
            stae2 = d;
            if ($('#secondPwd').val() != '' && $('#secondPwd').val() == $('#firstPwd').val()) {
                $('#secondPwd').css({ borderColor: '#ccc' });
                stae3 = d;
            }
            else {
                stae3 = '';
                pwdErrMsg = '两次密码不一致！';
            }
        }
    });
    $('#secondPwd').blur(function () {
        var d = $(this).val();
        if (d == $('#firstPwd').val()) {
            $('#secondPwd').css({ borderColor: '#ccc' });
            stae3 = d;
        } else {
            $('#secondPwd').css({ borderColor: '#f60' });
            pwdErrMsg = '两次密码不一致！';
            stae3 = '';
        }
    });
    $('#submitPwd').bind('click', function () {
        //console.log(stae1)
        if (!stae2) {
            $('#firstPwd').css({ borderColor: '#f60' });
            $.dialog.alert(pwdErrMsg);
        }
        if (!stae3) {
            $('#secondPwd').css({ borderColor: '#f60' });
            $.dialog.alert(pwdErrMsg);
        }
        if (stae2 && stae3) {
            var loading = showLoading();
            $.ajax({
                type: 'post',
                //url: 'SetPayPwd',
                url: "/" + areaName + "/Capital/SetPayPwd",
                data: { "pwd": stae3 },
                dataType: "json",
                success: function (data) {
                    loading.close();
                    if (data.success) {
                        $.dialog.succeedTips('设置成功！');
                        pwdflag = 'true';
                        $('#stepone').hide();
                        $('#steptwo').show();
                    }
                }
            });
        }
    });
    $('#btnWithDraw').click(function () {
        var userBalance = parseFloat($('#balanceValue').text());
        if (userBalance <= 0) {
            $.dialog.alert('可用金额为零，不能提现！');
            return;
        }
        var loading = showLoading();
        $.post("/" + areaName + "/Capital/CanWithDraw", null,
            function (result) {
                loading.close();
                if (result.success) {
                    if (!result.canWeiXin) {
                        $("#withDrawWeixinBox").hide();
                        $("#withDrawWeixin").prop("checked", false);
                    }
                    if (!result.canAlipay) {
                        $("#withDrawALipayBox").hide();
                        $("#withDrawALipay").prop("checked", false);
                    }
                    $("#J_assets_layer").addClass("cover");
                    //$(".steponeee").height($(".steponeee").width() * 120 / 141);
                    if (pwdflag.toLowerCase() == 'true') {
                        $('#steptwo').show();
                    }
                    else {
                        $('#stepone').show();
                    }
                }
                else {
                    $.dialog.alert('请在平台微信公众号内进行提现，或登录平台PC端进行提现');
                }
            }
        );
    });
    $("input[name='userVo.applyType']").on("click", function () {
        var _t = $(this);
        var _v = _t.val();
        var alipayitem = $(".alipayitem");
        alipayitem.hide();
        if (_v == 3) {
            alipayitem.show();
        }
    });
    $(".steponeee .close").click(function () {
        $(this).parent().hide();
        $("#J_assets_layer").removeClass("cover");
    });
    $('#submitApply').click(function () {
        var reg = /^[0-9]+([.]{1}[0-9]{1,2})?$/;
        if (!reg.test($('#inputMoney').val())) {
            $.dialog.alert("提现金额不能为非数字字符");
            return;
        }
        var applyWithDrawAmount = parseFloat($('#inputMoney').val());
        var userBalance = parseFloat($('#balanceValue').text());
        var inputWithDrawMinimum = parseFloat($('#inputWithDrawMinimum').val()) || 0;
        var inputWithDrawMaximum = parseFloat($('#inputWithDrawMaximum').val()) || 0;
        var openid = $('#wdopenId').val();
        var nickname = $('#wdnickName').val();
        var applyType = 1;
        var _d = $("input[name='userVo.applyType']:checked");
        if (_d) {
            applyType = _d.val();
        } else {
            $.dialog.alert("请选择提现到账方式");
            return;
        }

        if (parseFloat(applyWithDrawAmount) > userBalance) {
            $.dialog.alert("提现金额不能超出可用金额");
            return;
        }
        if (parseFloat(applyWithDrawAmount) < 1) {
            $.dialog.alert("提现金额不能小于1元");
            return;
        }
        if (!(parseFloat(applyWithDrawAmount) <= inputWithDrawMaximum && parseFloat(applyWithDrawAmount) >= inputWithDrawMinimum)) {
            $.dialog.alert("提现金额不能小于：" + inputWithDrawMinimum + " 元,不能大于：" + inputWithDrawMaximum + " 元");
            return;
        }
        if (applyType == 3) {
            if (openid.length < 1) {
                $.dialog.alert("请填写收款账号");
                return;
            }
            if (nickname.length < 1) {
                $.dialog.alert("请填写收款账号真实姓名");
                return;
            }
        }
        var loading = showLoading();
        $.post("/" + areaName + "/Capital/ApplyWithDrawSubmit", { nickname: '', amount: parseFloat($('#inputMoney').val()), pwd: $('#payPwd').val(), openid: openid, nickname: nickname, applyType: applyType },
            function (result) {
                loading.close();
                if (result.success) {
                    $.dialog.succeedTips('提现申请成功!', function () {
                        $('#steptwo').hide();
                        $('#stepone').hide();
                        window.location.reload();
                    });
                }
                else {
                    $.dialog.errorTips(result.msg);
                }
            }
        );
    });
    //充值
    $('#btnCharge').click(function () {
        //if (areaName.toLowerCase() != 'm-weixin') {
        //	$.dialog.alert('请在微信端进行充值');
        //	return;
        //}
        var ua = navigator.userAgent.toLowerCase();
        if (ua.match(/MicroMessenger/i) != "micromessenger") {
            $.dialog.alert('请在微信端进行充值');
            return;
        }
        $('#rechargePay').show();
        $("#J_assets_layer").addClass("cover");


        /*$.dialog({
    		title: '请输入充值金额',
    		lock: true,
    		width: 430,
    		padding: '0 40px',
    		id: 'advertisement',
    		content: ['<div class="dialog-form">',
						'<div class="form-group clearfix">',
							'<input class="form-control input-sm" type="text" id="chargeAmount">',
						'</div>',
					'</div>'].join(''),
    		okVal: '确定',
    		ok: function () {
    			
    		}
    	});*/
    });

    $('#submitPayBtn').click(function () {
        var amount = $('#chargeAmount').val();
        if (/^\d+(\.\d{1,2})?$/.test(amount) == false) {
            $.dialog.errorTips('请输入正确的金额');
            return false;
        }

        var loading = showLoading();
        $.post('/' + areaName + '/Capital/Charge', { pluginId: 'Himall.Plugin.Payment.WeiXinPay', amount: $('#chargeAmount').val() },
			function (data) {
			    loading.close();
			    if (data.success == true) {
			        $('#rechargePay').hide();
			        $("#J_assets_layer").removeClass();
			        //通过A链接模拟跳转
			        //jumpUrl在weixin中为一段js脚本，直接调用jsapi
			        if ($('#payJumpUrl').length > 0) {
			            $('#payJumpUrl').attr('href', data.href);
			        }
			        else {
			            $('body').append('<a id="payJumpUrl" style="display:none;" href="' + data.href + '"></a>');
                    }
                    alert(data.href);
			        document.getElementById("payJumpUrl").click();
			    } else
			        $.dialog.errorTips(data.msg);
			}).error(function (data) {
			    loading.close();
			    $.dialog.errorTips('操作失败,请稍候尝试.');
			});
    });

    var page = 1;

    $(window).scroll(function () {
        var scrollTop = $(this).scrollTop();
        var scrollHeight = $(document).height();
        var windowHeight = $(this).height();
        $('#autoLoad').show();
        if (scrollTop + windowHeight >= scrollHeight - 30) {

            loadRecords(++page);
        }
    });
});
var lodeEnd = false, total = 0;
function loadRecords(page) {
    if (lodeEnd)
        return;
    var url = "/" + areaName + '/Capital/List';
    $.post(url, { page: page, rows: 15 }, function (result) {
        var str = '';
        if (result.model.length > 0) {
            total = result.Total;
            $.each(result.model, function (i, model) {
            	str +='<li class="item">'+
                    '<div class="price '+(model.Amount<0?'out':'')+'">' + model.Amount+'</div>'+
		            '<div class="desc">' + model.SourceTypeName;
            	if (model.SourceType == 6 && model.Remark != "" && model.Remark != null && model.Remark != "null") {
            	    str += '<span>（' + model.Remark + '）</span>';
            	} else {
            	    str += '<span>（单号 ' + ((model.SourceData == "" || model.SourceData == null || model.SourceData == "null") ? model.Id : model.SourceData) + '）</span>';
            	}
                    str +='</div>'+
		            '<div class="time">' + model.CreateTime + '</div>'+
		        '</li>';
            });
            $('#ulList').append(str);
            if (total == result.model.length)
                lodeEnd = true;
        }
        else {
            $('#autoLoad').html('没有更多记录了');
            lodeEnd = true;
        }
    });

}