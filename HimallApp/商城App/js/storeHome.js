    var ws,
    	type=1,//用来区分是否第一次点击商品进来
    	dataC,
        categoryElem2 = document.getElementById('categoryRight'),
        Longitude="",
        Latitude="",
        storeName="",
        doscroll=true,
        fromLatLng = ''; //这里要获取当前用户定位
    var productList = document.getElementById('productlist'),
    	keywordsText=document.getElementById('keywordsText');
    var curpagesize = 10, curpageindex = 1, total = -1, lodeEnd = false, shopCategoryId = 0;
        mui.init({
            swipeBack: true
        });
        mui('#scroll1').scroll({
            indicators: false //是否显示滚动条
        });
        
        /*事件绑定*/
       mui('#dialogForbid').on('tap', 'div', function () {
       		mui.back();
       });
        mui('#categoryLeft').on('tap', 'li', function () {
            categoryElem2.scrollTop = 0;
            document.getElementsByClassName('cur')[0].className = '';
            this.className = 'cur';
            shopCategoryId = Number(this.getAttribute('id'));
            lodeEnd = false;
            curpageindex = 1;
            total = -1;
            if($(this).index() == 1)
            	$(".getCoupon-d").removeClass("hidden");
            else
            	$(".getCoupon-d").addClass("hidden");
            storeObj.LoadView(shopCategoryId);
        });
        document.getElementById("call").addEventListener('tap', function () {
            var btnArray = ['取消', '呼叫'];
            mui.confirm(contactPhone, '', btnArray, function (e) {
                if (e.index == 1) {
                    location.href = "tel:" + contactPhone;
                }
            })
        });
        document.getElementById('searchBtn').addEventListener('tap',function(){
			lodeEnd = false;
		    curpageindex = 1;
		    total = -1;
		    $("#-1").removeClass('hidden').addClass("cur").siblings().removeClass("cur");
		    $(".getCoupon-d").addClass("hidden");
    		shopCategoryId = -1;
		    loadData();
		});
		document.addEventListener("keyup",function(event){
			if((event||window.event).keyCode==13){
				keywordsText.blur();
				lodeEnd = false;
			    curpageindex = 1;
			    total = -1;
			    $("#-1").removeClass('hidden').addClass("cur").siblings().removeClass("cur");
    			shopCategoryId = -1;
			    loadData();
			}
		});
        mui('#productlist').on('tap', 'li a', function() {
			himall.openVW('store-product-detail.html', {proid: this.getAttribute('data-id'),storeid:shopBranchId});
		});
		mui('#productlist').on('tap', 'li .money', function() {
			himall.openVW('store-product-detail.html', {proid: this.getAttribute('data-id'),storeid:shopBranchId});
		});
		
		var dialogCoupon=document.getElementById('dialogCoupon'),
			couponMask;
		mui("#categoryRight").on('tap', '.getCoupon', function() {
			if(!couponMask){
				couponMask=mui.createMask(function(){himall.removeClass(dialogCoupon,' active') });
			}
			couponMask.show();
	    	dialogCoupon.className+=' active';
		});
		document.querySelector('#dialogCoupon .mui-icon').addEventListener('tap',function(){
			couponMask.close();
		});
		
		mui("#pop-sku").on('tap', '.comm-icon', function() {
			$('#pop-sku').addClass('hidden');
		});
        $("#categoryRight").scroll(function () {
            var scrollTop = $(this).scrollTop();
            var scrollHeight = $(this)[0].scrollHeight;
            var windowHeight = $(this).height();
            if (scrollTop + windowHeight >= scrollHeight) {
				if(doscroll){
					setTimeout(function () {
	                    loadData();
	                }, 200);
	                doscroll=false;
				}
            }
        });
        //设置滚动区域高度
        function setScrollHeight(){
        	var hminus = $('.dingwei').height() + $('.sales').height() + $('.search').height() + $('.sbottom-btn').height() + $('.mui-bar-nav').height();
        	if(mui.os.ios){
        		$('.index-category').height($(window).height() - hminus - 75);
        	}else{
        		$('.index-category').height($(window).height() - hminus - 50);
        	}
        	
        }
        
        var content = document.getElementById('categoryOneList');
        var contactPhone = '', shopId = 0, shopBranchId=0;
        
        //加入购物车方法
        function catchAddCart(obj){
        	var curOP=$(obj).attr('data-operator'),num,curProId=$(obj).attr('data-id');
        	num = parseInt(curOP+"1");
        	var curQuantity=0;
        	if(num>0){
        		curQuantity = parseInt($(obj).prev().val())+1;
        	}else{
        		curQuantity = parseInt($(obj).next().val())-1;
        		if(curQuantity==0 && $("#pop-cart").hasClass('is-visible')){
        			$(obj).parents('li').remove();
        		}
        	}
        	addToCart(curProId, $(obj).attr('data-sku'), num,obj,curQuantity);
        }
        
        function addToCart(id,skuId, quantity,obj,currQuantity){
        	if(himall.isLogin()){
        		if(!skuId || skuId.lenght<1){
			        plus.nativeUI.toast("请选择规格");
			      	return;
			    }
			    mui.ajax(URL + 'api/BranchCart/GetUpdateCartItem', {
	                data:himall.md5Data({
	                	shopBranchId:shopBranchId,
	                	skuId: skuId,
	                	count: currQuantity,
	                	userkey:himall.getState().userkey
	                }),
	                dataType: 'json',
	                type: 'get',
	                timeout: 10000,
	                success: function (data) {
	                	if(data.success){
	                		if($(obj).attr('data-hasSku')){
	                			if(currQuantity==0){
                					$(skuSelBtn).html('选择规格');
                				}else{
                					$(skuSelBtn).html('选择规格<i class="cart-num">'+currQuantity+'</i>');
                				}
	                			setSkuCartQuantity(skuId, quantity, "+");
	                		}else{
	                			if(quantity>0){
					        		$(obj).prev().val(currQuantity).removeClass('not-visible');
	        						$(obj).prev().prev().removeClass('not-visible');
					        	}else{
					        		$(obj).next().val(currQuantity);
					        		if(currQuantity==0){
					        			$(obj).addClass('not-visible');
	        							$(obj).next().addClass('not-visible');
					        		}
					        	}
	                		}
            				if($("#pop-cart").hasClass('is-visible')){
            					SetBottomData();
            					lodeEnd = false;
							    curpageindex = 1;
							    total = -1;
							    loadData();
            				}else{
            					getCartInfo();
            				}
	                	}else{
	                		mui.toast(data.msg);
	                	}
	                },
	                error: function (xhr) { 
	                   	mui.toast('请求超时，请检查网络');
	                }
	            });
        	}else{
        		showLogin({fireId:'store-home.html'});
        	}
        }
        
        var selectedSku,selectedskuList,curSkuInfo={},skuSelBtn;
        //获取商品规格
        function showSku(obj,e){
        	skuSelBtn = obj; //保存 选择规格 按钮的对象
        	if(himall.isLogin()){
        		mui.ajax(URL + 'api/ShopBranchWeb/GetProductSkuInfo', {
	                data: himall.md5Data({
	                	id:$(obj).attr('data-id'),
	                    shopBranchId: shopBranchId,
	                    userkey:himall.getState().userkey
	                }),
	                dataType: 'json',
	                type: 'get',
	                timeout: 10000,
	                success: function (data) {
	                    if (data.success) {  
	                    	var productInfo = data.data;
							var cursku = productInfo.DefaultSku;
							var curskuselContent=null;
							var selectsku=[];
							if (productInfo!=null){
								$.each(productInfo.SkuItems, function (idex, item) {
									item.AttributeValue[0].UseAttributeImage ='selected';
									var defaultsku=new Object();
									defaultsku.ValueId = item.AttributeValue[0].ValueId;
									defaultsku.Value = item.AttributeValue[0].Value;
									selectsku.push(defaultsku);
								});
							}
							curSkuInfo.CurrentProduct = productInfo;
							curSkuInfo.CurrentSku = cursku;
							selectedskuList=selectsku;
							selectedSku=cursku.SkuId;
							document.getElementById("pop-sku").innerHTML = template('initSkuInfo',curSkuInfo);
							$("#pop-sku").removeClass('hidden');
	                    } else {
	                        plus.nativeUI.toast(data.msg)
	                    }
	                }
	            });
        	}else{
        		showLogin({fireId:'store-home.html'});
        	}
        }
        //选择规格
        mui("#pop-sku").on('tap', '.comm-attr span', function() {
        	var index = this.getAttribute('data-indexcount');
		    var valueid = this.getAttribute('data-id');
		    var value = this.getAttribute('data-skuvalue');
		    var selInfo = new Object();
		    selInfo.ValueId = valueid;
		    selInfo.Value = value;
		    var selSku = selectedskuList;
		    selSku[index] = selInfo;
		
		    var selContent = "";
		    var isAlSelected = false;
		    var tempcurrentproduct = curSkuInfo.CurrentProduct;
		    var itemList = curSkuInfo.CurrentProduct.SkuItems;
		    if (tempcurrentproduct.SkuItems.length == selSku.length) isAlSelected = true;
		    var skuId = tempcurrentproduct.ProductId;
		    for (var i = 0; i < selSku.length; i++) {
		      var info = selSku[i];
		      if (info != undefined) {
		        selContent += selContent == "" ? info.Value : "," + info.Value;
		        skuId += "_" + info.ValueId;
		      }
		    }
		    //var curentItem = itemList[index];
		    for (var j = 0; j < tempcurrentproduct.SkuItems[index].AttributeValue.length; j++) {
		      var item = tempcurrentproduct.SkuItems[index].AttributeValue[j];
		      if (item.ValueId == valueid) {
		        tempcurrentproduct.SkuItems[index].AttributeValue[j].UseAttributeImage = 'selected';
		      }
		      else {
		        tempcurrentproduct.SkuItems[index].AttributeValue[j].UseAttributeImage = 'False';
		      }
		    }
		    
		    var currentProductSku = null;
		    $.each(curSkuInfo.CurrentProduct.Skus, function (idex, item) {
		    	  var found=true;
			      for (var i = 0; i < selSku.length; i++) {
			        if (selSku[i] ==undefined||item.SkuId.indexOf('_' + selSku[i].ValueId) == -1)
			          found = false;
			      }
			      if (found && itemList.length == selSku.length) {
			        currentProductSku = item;
			        skuId=item.SkuId;
			        return;
			      }
		    });
		    curSkuInfo.CurrentProduct = tempcurrentproduct;
            curSkuInfo.CurrentSku = currentProductSku;
            selectedskuList=selSku;
            selectedSku=skuId;
            document.getElementById("pop-sku").innerHTML = template('initSkuInfo',curSkuInfo);
        });
        
        //修改商品失规格购物车中存在数量,只能操作this.data.CurrentProduct中的规格
        function setSkuCartQuantity(skuId, num, operator){
		    var hasEdit = false;
		    var _curProduct = curSkuInfo.CurrentProduct;
		    if (_curProduct && _curProduct.Skus) {
		      var _sku; 
		      $.each(curSkuInfo.CurrentProduct.Skus, function (idex, item) {
		      		if(item.SkuId==skuId){
		      			_sku = item;
		      			return;
		      		}
		      });
		      var _cursku = curSkuInfo.CurrentSku;
		      if (_sku) {
		        num = parseInt(num);
		        switch (operator) {
		          case "=":
		            _sku.CartQuantity = num;
		            break;
		          case "+":
		            _sku.CartQuantity += num;
		            break;
		        }
		        if (_sku.CartQuantity < 0) {
		          _sku.CartQuantity = 0;
		        }
		        if (_cursku && _cursku.SkuId == _sku.SkuId) {
		          _cursku.CartQuantity = _sku.CartQuantity;
		        }
		        hasEdit = true;
		      }
		    }
		    if (hasEdit) {
		       curSkuInfo.CurrentProduct = _curProduct;
               curSkuInfo.CurrentSku = _cursku;
               document.getElementById("pop-sku").innerHTML = template('initSkuInfo',curSkuInfo);
		    }
        }
        
        function getPositionSuccess(position){
        	var lat = position.coords.latitude; //纬度 
			var lon = position.coords.longitude; //经度 
			mui.ajax(TENXUNMAP, {
                data: {
                	locations: lat+','+lon,
                	type: 1,
                	key: MAPKEY
                },
                dataType: 'json',
                type: 'get',
                timeout: 10000,
                success: function (data) {
                    lat = data.locations[0].lat;
                    lon = data.locations[0].lng;
                    fromLatLng = lat+','+lon;
                    getStoreInfo();
                    mui(".mui-content").on("tap",".locate-ic",function(){
    			      var local=new plus.maps.Point(lon,lat);
	            	  var destination=new plus.maps.Point(Longitude,Latitude);
	            	  plus.maps.openSysMap( destination, storeName, local );   
            		}); 
                }
            });
        }
        
        mui.plusReady(function () {
        	ws = plus.webview.currentWebview();
            shopBranchId = ws.shopid;
            keywordsText.value = ws.keywords?ws.keywords:''; 
        	if(ws.showcart){
        		$('#pop-cart').addClass('is-visible');
        	}
			 fromLatLng=plus.webview.currentWebview().fromLatLng;
			 if(!fromLatLng){
			    himall.getPosition(getPositionSuccess);
			 }else{
			 	getStoreInfo();
			 }
        });
        
        function getStoreInfo(isUpdate){      
            plus.nativeUI.showWaiting();
            //加载门店信息
            mui.ajax(URL + 'api/ShopBranchWeb/GetShopBranchInfo', {
                data: himall.md5Data({
                    id: shopBranchId,
                    fromLatLng: fromLatLng,
                    userkey:himall.getState().userkey
                }),
                dataType: 'json',
                type: 'get',
                timeout: 10000,
                success: function (data) {
                    if (data.success) {   
	                    //超出配送范围提示
	                    if (!isUpdate && !data.Store.IsAboveSelf && data.Store.IsStoreDelive && data.Store.ServeRadius && data.Store.Distance > data.Store.ServeRadius) {
	                		plus.nativeUI.alert("您的定位己经超过该店配送区域");
	                    }             
                        $(".mui-title").html(data.Store.ShopBranchName);
                        $("#storename").html(data.Store.ShopBranchName);
                        $("#opentime").html(data.Store.StoreOpenStartTime.substring(0,5)+"至"+data.Store.StoreOpenEndTime.substring(0,5));
                        $("#storeaddr").html(data.Store.AddressDetail);
                        //$("#commentscore").html(data.Store.CommentScore);
                        if(data.Store.ShopImages.length>0){
							$("#storelogo img").attr("src",data.Store.ShopImages);                        	
                        }else{
                        	$("#storelogo img").attr("src","images/index-banner.png");
                        }
                        //页面重置代码
					    var touxiangimg = $('#storelogo img').attr('src');
					    var beijingBG = 'url(' + touxiangimg + ') no-repeat center center';
					    $('.beijing').css({ 'background': beijingBG, 'background-size': '105% 105%' });
                        Longitude=data.Store.Longitude;
                        Latitude=data.Store.Latitude;
                        storeName=data.Store.ShopBranchName;
                        contactPhone = data.Store.ContactPhone;
                        shopId = data.Store.ShopId;
                        shopBranchId = data.Store.Id;
                        getCategorys();
                        var count = 0,types1 = new Array(),types3='',types2='';
        	    		var actives = data.ShopAllActives;
        	    		if(actives.ShopActives){
        	    			for(var j=0,lens=actives.ShopActives.length;j<lens;j++){
        	    				types1.push(''+actives.ShopActives[j].ActiveName);
        	    				count++;
        	    			}
        	    		}
        	    		if (actives.FreeFreightAmount > 0){
        	    			types3 = '满'+actives.FreeFreightAmount+'元免运费';
        	    			count++;
        	    		}
        	    		if(actives.ShopCoupons && actives.ShopCoupons.length>0){
        	    			for(var j=0,lens=actives.ShopCoupons.length;j<lens;j++){
        	    				var _item=actives.ShopCoupons[j];
        	    				var showOrderAmount=parseFloat( _item.OrderAmount);
        	    				var showPrice=_item.Price;
        	    				if(showOrderAmount<_item.Price){
        	    					showOrderAmount=_item.Price;
        	    				}
        	    				if(showOrderAmount=parseInt(showOrderAmount)){
        	    					showOrderAmount=showOrderAmount.toFixed(0);
        	    				}else{
        	    					showOrderAmount=showOrderAmount.toFixed(2);
        	    				}
        	    				if(showPrice=parseInt(showPrice)){
        	    					showPrice=showPrice.toFixed(0);
        	    				}else{
        	    					showPrice=showPrice.toFixed(2);
        	    				}
        	    				types2 = types2+',满'+ showOrderAmount +'减'+showPrice+'';
        	    			}
        	    			count++;
        	    			types2 = types2.substr(1,types2.length);
        	    			$(".getCoupon-d span").html(actives.ShopCoupons.length+"张优惠券");
        	    			$(".getCoupon-d p").html(types2);
        	    		}else{
        	    			$(".getCoupon-d").hide();
        	    		}
        	    		data.Store.types1=types1;
        	    		data.Store.count = count;
        	    		data.Store.types3=types3;
        	    		data.Store.types2=types2;
        	    		document.querySelector(".sales").innerHTML = template('initActive',data);
        	    		setScrollHeight();
        	    		$(".sale-num").click(function(){
					    	var obj = $(this).parent();
					    	if(obj.hasClass('active')){
					    		obj.removeClass('active');
					    	}else{
					    		obj.addClass('active');
					    	}
					    	setScrollHeight();
					    });
					    document.getElementById("coupons").innerHTML = template('initCoupon',actives.ShopCoupons);
                    } else {
                    	plus.nativeUI.closeWaiting();
                        var dialogForbid=document.getElementById('dialogForbid'),
							forbidMask=document.getElementById("pop-forbid");
						if(data.msg){
							document.querySelector("#dialogForbid h3").innerHTML = data.msg;
						}
						himall.removeClass(dialogForbid,'hidden');
						himall.removeClass(forbidMask,'hidden');
                    }
                }
            }); 
        }
        //获取分类数据
        function getCategorys(){
        	//获取一级分类列表
            mui.ajax(URL + 'api/ShopBranchWeb/GetShopCategory', {
                data: himall.md5Data({
                    shopId: shopId,
                    shopBranchId:shopBranchId
                }),
                dataType: 'json',
                type: 'get',
                timeout: 10000,
                success: function (data) {
                    if (data.success) {   
                    	content.innerHTML = template('initData', data);//商家一级分类
                    	if(keywordsText.value!=''){
                    		$("#-1").removeClass("hidden").addClass('cur');
                    		shopCategoryId = -1;
                    	}else{
                    		$("#-1").removeClass("cur").addClass('hidden');
                    		$("#categoryOneList li:nth-child(2)").addClass('cur');
                    		$(".getCoupon-d").removeClass("hidden");
                        	shopCategoryId = Number($("#categoryLeft li:nth-child(2)").attr("id"));//默认加载第一个
                    	}
                    	
                        storeObj.LoadView(shopCategoryId);//第一次默认加载
                        getCartInfo();//加载底部购物车信息
                    } else {
                        plus.nativeUI.toast(data.msg)
                    }
                }
            });
        }
        //加载商品列表
        function loadData() {
            if (lodeEnd)
                return;
            var keytxt;
            if(shopCategoryId>=0){
            	keytxt = '';
            }else{
            	keytxt = keywordsText.value;
            }
            mui.ajax(URL + 'api/ShopBranchWeb/GetProductList', {
                data: himall.md5Data({
                    pageNo: curpageindex,
                    pageSize: curpagesize,
                    shopId: shopId,
                    shopBranchId: shopBranchId,
                    shopCategoryId: shopCategoryId,
                    keyWords: keytxt,
                    type: type,
                    productId: ws.productid?ws.productid:0,
                    userkey:himall.isLogin()?himall.getState().userkey:''
                }),
                dataType: 'json',
                type: 'get',
                timeout: 10000,
                success: function (data) {
                	type=0;
                	plus.nativeUI.closeWaiting();
                    if (data.success) {
                        if (curpageindex == 1) {//每个分类加载第一页的时候开始清空，后面append
                            productList.innerHTML='';
                        }
                        total = data.total;
                        if(total==0&&data.Products.length<1){
                        	$("#categoryRight .no-content").removeClass('hidden');
                        }else{
                        	$("#categoryRight .no-content").addClass('hidden');
                        }
                        if (data.Products.length > 0) {
                        	if(curpageindex == 1){
                        		data.Products[0].showCategory=true;	 
                        		if(data.Products[0].IsTop && data.Products.length>1){
                        			data.Products[1].showCategory=true;	 
                        		}
                        	}
                        	for(var i=1,len=data.Products.length;i<len;i++){
	                        	if(data.Products[i].CategoryName!=data.Products[i-1].CategoryName){
	                        		data.Products[i].showCategory = true;
	                        	}
	                        }
                            productList.insertAdjacentHTML('beforeend', template('initProductData', data));//商家一级分类及其子类下商品
                            curpageindex += 1;
                            if (total == data.Products.length) {
                                lodeEnd = true;
                            }
                        } else {
                            lodeEnd = true; 
                        }
                    } else {
                        plus.nativeUI.toast(data.msg)
                    }
                    doscroll = true;
                },
                error: function (xhr) {
                	plus.nativeUI.closeWaiting();
                    reloadWvLoad();
                }
            });
        }
        var storeObj = {
            curView: -2,
            LoadView: function (suff) {
                if (storeObj.curView == suff) {
                    return;
                }
                loadData();
                storeObj.curView = shopCategoryId;
            }
        }
        
        
        //领取优惠券
        mui('#coupons').on('tap', '.getCoupon', function() {
			if(himall.isLogin()){
				var w=plus.nativeUI.showWaiting('',{padlock:true});
				var _this=this;
				var isUse = _this.getAttribute('data-use');				
				if(isUse > 0){
					w.close();
					return;
				}
				mui.ajax(URL+'api/coupon/PostAcceptCoupon',{
					data:himall.md5Data({
						vshopId:0,
						couponId:_this.getAttribute('data-id'),
						userkey:himall.getState().userkey
					}),
					dataType:'json',
					type:'POST',
					timeout:10000,
					success:function(data){
						w.close();
						if(data.success){
							plus.nativeUI.toast('优惠券领取成功');
							_this.className += ' disabled';		
							_this.innerText = '已领取';
						}else{
							if(data.code==2)
								plus.nativeUI.toast('优惠券已经过期');
							if(data.code==3){
								_this.className += ' disabled';		
								_this.innerText = '已领取';
								_this.setAttribute('data-use',1);
								plus.nativeUI.toast('达到每个用户领取最大张数');									
							}
							if(data.code==4){
								_this.className += ' disabled';
								_this.innerText = '已领完';
								_this.setAttribute('data-use',2);
								plus.nativeUI.toast('优惠券已领完');									
							}
							if(data.code==5)
								plus.nativeUI.toast('积分不足');
						}
					},
					error:function(xhr){
						console.log(JSON.stringify(xhr))
						w.close();
						plus.nativeUI.toast('优惠券领取失败，请检查网络')
					}
				});
			}else{
				showLogin({fireId:'store-home.html'});
			}
		});
	
	document.addEventListener('updateData', function() {
		storeObj.curView=-2;
		curpageindex = 1; 
		total = -1;
		lodeEnd = false;
		getStoreInfo("update");
	});
	
		
       