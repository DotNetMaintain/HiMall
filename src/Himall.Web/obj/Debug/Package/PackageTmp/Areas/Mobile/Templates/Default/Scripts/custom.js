// JavaScript Document

$(function(){
	
	//图片延迟加载
	$(".lazyload").scrollLoading();
	
	//高度控制
	var w=$('.container').width();
	$('.recom-topic li').height(w*15/32);
	//$('.goods-list li').height(w/16*11);
	
	
	$('.wx_aside .btn_more').click(function() {
		$(this).parent().toggleClass('active');
	});
	
	
	$('.btn_top').click(function() {
		$('body,html').animate({scrollTop:0},300);
		return false;
	});
	
	$(window).scroll(function() {
		$('.wx_aside').removeClass('active');
		var s=$(window).scrollTop();
		if(s>=300){
			$('.btn_top').fadeIn();
		}else{
			$('.btn_top').fadeOut();
		}
	});
	
	/*$('.WX-return').click(function() {
		history.go(-1);
	});*/
	
	
});


//设置元素高宽相等
function square(elem){
	elem.height(elem.width())
}