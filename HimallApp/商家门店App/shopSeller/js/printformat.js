/**
 * 复位打印机
 */
var RESET = "\x1b\x40";

/**
 * 左对齐
 */
var ALIGN_LEFT = "\x1b\x61\x00";

/**
 * 中间对齐
 */
var ALIGN_CENTER = "\x1b\x61\x01";

/**
 * 右对齐
 */
var ALIGN_RIGHT = "\x1b\x61\x02";

/**
 * 选择加粗模式
 */
var BOLD = [0x1b, 0x45, 0x01];

/**
 * 取消加粗模式
 */
var BOLD_CANCEL = [0x1b, 0x45, 0x00];
/**
 * 普通高度
 */
var FONT_NORMAL = "\x1d\x21\x00";

/**
 * 宽高加倍
 */
var DOUBLE_HEIGHT_WIDTH = "\x1d\x21\x11";

/**
 * 宽加倍
 */
var DOUBLE_WIDTH = [0x1d, 0x21, 0x10];

/**
 * 高加倍
 */
var DOUBLE_HEIGHT = "\x1d\x21\x01";

/**
 * 字体不放大
 */
var NORMAL = [0x1d, 0x21, 0x00];

/**
 * 设置默认行间距
 */
var LINE_SPACING_DEFAULT = [0x1b, 0x32];

/**
 * 打印纸一行最大的字节
 */
var LINE_BYTE_SIZE = 32;

/**
 * 打印三列时，中间一列的中心线距离打印纸左侧的距离
 */
var LEFT_LENGTH = 16;

/**
 * 打印三列时，中间一列的中心线距离打印纸右侧的距离
 */
var RIGHT_LENGTH = 16;

/**
 * 打印三列时，第一列汉字最多显示几个文字
 */
var LEFT_TEXT_MAX_LENGTH = 5;

function getBytesLength(str) {　
	var byteLen = 0,
		len = str.length;　　
	if(str) {　　　　
		for(var i = 0; i < len; i++) {　　　　　　
			if(str.charCodeAt(i) > 255) {　　　　　　　　
				byteLen += 2;　　　　　　
			}　　　　　　
			else {　　　　　　　　
				byteLen++;　　　　　　
			}　　　　
		}　　　　
		return byteLen;　　
	}　　
	else {　　　　
		return 0;　　
	}
}

/**
 * 设置打印格式,command 格式指令
 */
function selectCommand(command) {
	return command;
}

//打印二列
function printTwoData(leftText, rightText) {
	//StringBuilder sb = new StringBuilder();
	var sb = '';
	var leftTextLength = getBytesLength(leftText);
	var rightTextLength = getBytesLength(rightText);
	sb = leftText;

	// 计算两侧文字中间的空格
	var marginBetweenMiddleAndRight = LINE_BYTE_SIZE - leftTextLength - rightTextLength;

	for(var i = 0; i < marginBetweenMiddleAndRight; i++) {
		sb += " ";
	}
	sb += rightText;
	return sb + '\n';
}

//打印一行线
function printLine(FormatText) {
	var printext = "";
	for(var x = 0; x < LINE_BYTE_SIZE; x++) {
		printext += FormatText;
	}
	return ALIGN_LEFT + printext + "\n";
}

function printInit() {
	return RESET;
}
//打印三列
function printThreeData(leftText, middleText, rightText) {
	var sb = '';
	var leftslice = leftText; //左边余下字数长度
	//左边最多显示 LEFT_TEXT_MAX_LENGTH 个汉字 + 两个点
	sb += selectCommand(ALIGN_LEFT);
	if(leftText.length < LEFT_TEXT_MAX_LENGTH) {
		sb = printThree(leftText, middleText, rightText);
	} else {
		var slength = leftText.length % LEFT_LENGTH; //最后一行字符长度
		var line = parseInt(leftText.length / LEFT_LENGTH); //获取行数
		for(var x = 0; x < line; x++) { //输出左边距离
			sb += leftText.substr(LEFT_LENGTH * x, LEFT_LENGTH) + "\n";
		}
		leftslice = leftText.substr(LEFT_LENGTH * line);
		if(slength > LEFT_TEXT_MAX_LENGTH) { //超出左边最大值,打印到下一行
			sb += leftslice + "\n"
			leftslice = "";
			for(var k = 0; k < LEFT_TEXT_MAX_LENGTH; k++) {
				leftslice += " ";
			}
		}
		sb += printThree(leftslice, middleText, rightText);
	}
	return sb;
}

function printThree(leftText, middleText, rightText) {
	var sb = '';

	//左边最多显示 LEFT_TEXT_MAX_LENGTH 个汉字 + 两个点
	if(leftText.length > LEFT_TEXT_MAX_LENGTH) {
		leftText = leftText.substr(0, LEFT_TEXT_MAX_LENGTH) + "..";
	}

	var leftTextLength = getBytesLength(leftText);
	var middleTextLength = getBytesLength(middleText);
	var rightTextLength = getBytesLength(rightText);
	sb = leftText;
	// 计算左侧文字和中间文字的空格长度
	var marginBetweenLeftAndMiddle = LEFT_LENGTH - leftTextLength - middleTextLength / 2;

	for(var i = 0; i < marginBetweenLeftAndMiddle; i++) {
		sb += " ";
	}
	sb += middleText;

	// 计算右侧文字和中间文字的空格长度
	var marginBetweenMiddleAndRight = RIGHT_LENGTH - middleTextLength / 2 - rightTextLength;

	for(var i = 0; i < marginBetweenMiddleAndRight; i++) {
		sb += " ";
	}

	// 打印的时候发现，最右边的文字总是偏右一个字符，所以需要删除一个空格
	sb = sb.substr(0, sb.length - 1) + rightText;
	return sb + '\n';
}

//打印中间g
function printCenterLine(format, txt) {
	var str = "";
	if(txt) {
		var lenbyte = getBytesLength(txt);
		var marginwidth = LEFT_LENGTH - lenbyte / 2;
		var lineformat = "";
		for(var x = 0; x < marginwidth; x++) {
			lineformat += format;
		}
		str = lineformat + txt + lineformat + "\n";

	} else {
		str = printLine(format);
	}
	return str;
}

function PrintTest(bleObj) {
	var res = {
		"OrderId": "201708239623219",
		"TakeCodeIsUsed": false,
		"TakeCodeUsedTime": "",
		"TakeCode": "",
		"StatusText": "待确认",
		"Status": 1,
		"ItemStatus": 0,
		"ItemStatusText": "正常状态",
		"OrderDate": "2017-08-23 18:24:42",
		"ShipTo": "张三",
		"ShipToDate": "任意时间",
		"Cellphone": "13986589657",
		"Address": "湖南文化大厦1905",
		"OrderTotal": "417.28",
		"FreightFreePromotionName": null,
		"ReducedPromotionName": "满300减10",
		"ReducedPromotionAmount": "-10.00",
		"SentTimesPointPromotionName": "满300双倍积分",
		"CanBackReturn": false,
		"CanCashierReturn": false,
		"PaymentType": "到店支付",
		"DeductionPoints": null,
		"CouponAmount": "0.00",
		"CouponName": null,
		"DeductionMoney": "0.00",
		"RefundAmount": "0.00",
		"Remark": "",
		"InvoiceTitle": "",
		"Tax": "0.00",
		"AdjustedFreight": "0.00",
		"Freight": "0.00",
		"ShippingModeId": -2,
		"OrderType": 0,
		"LineItems": [{
			"Status": 0,
			"StatusText": "正常状态",
			"Id": "174_286_284",
			"Name": "【演示数据】宝贝年代良品水洗棉四件套无印全棉纯色日式简约床品床笠床单款",
			"Price": "427.28",
			"Amount": 1,
			"Image": "https://imageysc.kuaidiantong.cn/Storage/master/product/thumbs180/180_712eb5deb1944ddead3d4ea7d6129aab.jpg",
			"SkuText": "时间：2P; 功率：年票; ",
			"ProductId": 174,
			"PromotionName": null
		}],
		"Gifts": [{
			"GiftId": 1,
			"GiftName": "数据线",
			"PromoteType": 15,
			"Quantity": 1,
			"ImageUrl": "https://yscsslshop.kuaidiantong.cn/Storage/master/gift/thumbs180/180_201708211120411086660.png"
		}]
	};

	var printText = printInit(); //初始化
	var oid = res.OrderId.substr(res.OrderId.length - 4);
	printText += printCenterLine('*', oid);
	printText += selectCommand(DOUBLE_HEIGHT_WIDTH);
	printText += selectCommand(ALIGN_CENTER); //居中
	var ordername = "服务订单";
	if(res.ShippingModeId == 0 || res.ShippingModeId == -1) {
		ordername = "配送订单";

	} else if(res.ShippingModeId == -2) {
		ordername = "自提订单";
	}
	printText += ordername + "\n";
	printText += selectCommand(DOUBLE_HEIGHT);
	printText += "测试门店" + "\n\n";

	printText += selectCommand(ALIGN_LEFT);
	printText += selectCommand(FONT_NORMAL); //普通高度
	printText += printTwoData('下单时间：', res.OrderDate);
	printText += printTwoData('订单号：', res.OrderId);
	printText += selectCommand(DOUBLE_HEIGHT);
	printText += selectCommand(ALIGN_CENTER);
	var payresult = "未支付";
	if(res.Status == 2) {
		payresult = "已支付";
	}
	printText += res.PaymentType + "(" + payresult + ")";
	printText += selectCommand(FONT_NORMAL) + "\n\n"; //普高
	printText += printLine('-'); //线条

	printText += selectCommand(DOUBLE_HEIGHT);
	printText += selectCommand(ALIGN_LEFT);
	printText += printThreeData('商品', '数量', '小计');
	printText += selectCommand(FONT_NORMAL); //普通高度

	//商品
	res.LineItems.forEach(function(item, index, array) {
		var proname = item.Name;
		if(proname.length > 32) {
			proname = proname.substr(0, 32) + "..";
		}
		if(item.SkuText.length > 0) {
			proname += "(" + item.SkuText + ")";
		}
		var pronum = "X" + item.Amount;
		if(item.PromotionName != null) {
			pronum += item.PromotionName;
		}
		printText += printThreeData(proname, pronum, item.Price);
	});
	printText += selectCommand(ALIGN_LEFT) + "\n"; //左边
	printText += printLine('-'); //线条

	//礼品订单
	if(res.Gifts.length > 0) {
		printText += "\n";
		res.Gifts.forEach(function(item, index, array) {
			var giftname = '礼品：' + item.GiftName;
			var giftnum = "X" + item.Quantity;
			if(item.PromotionName != null) {
				pronum += item.PromotionName;
			}
			printText += printThreeData(giftname, giftnum, '0.00');
		});
		printText += selectCommand(ALIGN_LEFT) + "\n\n"; //左边
		printText += printLine('-'); //线条
	}

	//配送费			
	printText += printTwoData('配送费', '￥' + res.Freight);
	//满减
	if(res.ReducedPromotionName != null)
		printText += printTwoData(res.ReducedPromotionName, '￥' + res.ReducedPromotionAmount);

	//优惠券
	if(res.CouponName != null && res.CouponName != undefined)
		printText += printTwoData(res.CouponName, '￥' + res.CouponAmount);

	//积分抵扣
	if(parseInt(res.DeductionMoney) > 0)
		printText += printTwoData('积分抵扣', res.DeductionMoney);

	printText += selectCommand(ALIGN_LEFT) + "\n"; //左边
	printText += printLine('-') + "\n"; //线条

	//合计订单
	printText += selectCommand(DOUBLE_HEIGHT);
	printText += printTwoData('合计：', res.OrderTotal);

	printText += selectCommand(FONT_NORMAL);
	printText += selectCommand(ALIGN_LEFT) + "\n"; //左边
	printText += printLine('-') + "\n"; //线条

	printText += selectCommand(DOUBLE_HEIGHT);
	printText += printTwoData('备注：', res.Remark);

	printText += selectCommand(FONT_NORMAL);
	printText += selectCommand(ALIGN_LEFT) + "\n"; //左边
	printText += printLine('-') + "\n"; //线条

	if(res.ShipTo.length > 0) {
		printText += selectCommand(DOUBLE_HEIGHT);
		printText += res.ShipTo + "\n";
		if(res.Cellphone.length == 11) {
			printText += res.Cellphone.substr(0, 3) + "-" + res.Cellphone.substr(3, 4) + "-" + res.Cellphone.substr(7, 4) + "\n";
		} else {
			printText += res.Cellphone + "\n";
		}
	}

	printText += selectCommand(DOUBLE_HEIGHT);
	printText += selectCommand(ALIGN_LEFT) + "\n"; //左边
	if(res.Address.length > 0) {
		printText += res.Address + "\n\n";
	}
	printText += selectCommand(FONT_NORMAL);
	printText += printCenterLine('*', oid + '完');
	printText += printLine(' ');
	printText += printLine(' ');
	printText += "\n\n"
	printInit(); //初始化

	bleObj.gotoPrint(printText);
}