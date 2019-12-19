$(function () {
	var namespace = ".assess";

	$.extend({
		isNull: function (obj) {
			return obj == null || obj == undefined;
		},
		//是null或空
		isNullOrEmpty: function (str) {
			if (typeof str === 'string')
				return str == '';
			return $.isNull(str);
		},
		notNullOrEmpty: function (str) {
			return $.isNullOrEmpty(str) == false;
		}
	});

	String.prototype.format = function () {
		var ps = arguments;
		return this.replace(/\{\{/g, '{').replace(/\}\}/g, '}').replace(/\{[^\{\}]+\}/g, function (str) {
			var number, _default, format;
			var temp = str.replace('{', '').replace('}', '').split('?');
			number = temp[0];
			_default = temp[1];
			if (number.indexOf(':') > -1) {
				temp = number.split(':');
				number = temp[0];
				format = temp[1];
			} else if (temp.length > 1 && _default.indexOf(':') > -1) {
				temp = _default.split(':');
				_default = temp[0];
				format = temp[1];
			}
			var index = parseInt(number);
			if (isNaN(index))
				return '';
			var value = ps[index];
			if ($.isNullOrEmpty(value))
				value = _default || '';
			if ($.notNullOrEmpty(format) && (format == 'src' || format == "'src'" || format == '"src"'))
				value = 'src="' + value + '"';
			return value;
		});
	};


	Array.prototype.contains = function (obj) {
		var flag = false;
		this.forEach(function (item, i) {
			if (item == obj) {
				flag = true;
				return flag;
			}
		});
		return flag;
	};
	Array.prototype.clear = function () {
		if (this.length > 0)
			this.splice(0, this.length);
	};
	Array.prototype.pushArray = function (array, distinct) {
		for (var i = 0; i < array.length; i++) {
			var item = array[i];
			if (distinct == true && this.contains(item))
				continue;
			else if (typeof distinct == 'function' && distinct(item, i) == true)
				continue;
			this.push(array[i]);
		}
	};
	Array.prototype.remove = function (item) {
		var self = this;
		if ($.isFunction(item)) {
			self.where(item).forEach(function (p) {
				var index = self.indexOf(p);
				while (index >= 0) {
					self.splice(index, 1);
					index = self.indexOf(p);
				}
			});
		}
		var index = self.indexOf(item);
		while (index >= 0) {
			self.splice(index, 1);
			index = self.indexOf(item);
		}
	};
	Array.prototype.removeAt = function (index) {
		this.splice(index, 1);
	};
	Array.prototype.removeArray = function (array) {
		for (var i = 0; i < array.length; i++) {
			this.remove(array[i]);
		}
	};
	Array.prototype.first = function (where) {
		if (!$.isFunction(where))
			return this[0];
		var result;
		this.forEach(function (item, i) {
			if (where(item, i)) {
				result = item;
				return;
			}
		});
		return result;
	};
	Array.prototype.last = function (where) {
		if (!$.isFunction(where))
			return this[this.length - 1];
		var result;
		for (var i = this.length - 1; i >= 0; i--) {
			result = this[i];
			if (where(result, i))
				break;
		}
		return result;
	};
	Array.prototype.clone = function () {
		var newArray = [];
		for (var i = 0; i < this.length; i++) {
			newArray.push(this[i]);
		}
		return newArray;
	};
	Array.prototype.all = function (fn) {
		var me = this;
		for (var i = 0; i < me.length; i++) {
			if (fn(me[i], i) == false)
				return false;
		}
		return true;
	};
	Array.prototype.any = function (fn) {
		var me = this;
		for (var i = 0; i < me.length; i++) {
			if (fn(me[i], i) == true)
				return true;
		}
		return false;
	};
	Array.prototype.newitem = function (fn) {
		var result = [];
		this.forEach(function (item, i) {
			var newitem = fn(item, i);
			if (newitem != undefined)
				result.push(newitem);
		});
		return result;
	};
	Array.prototype.sum = function (fn) {
		var number = 0;
		var _fn = $.isFunction(fn) ? fn : null;
		this.forEach(function (item, i) {
			if (_fn == null && !isNaN(item))
				number += item;
			else if (_fn != null)
				number += fn(item, i);
		});
		return number;
	};
	Array.prototype.where = function (fn) {
		var result = [];
		this.forEach(function (item, i) {
			if (fn(item, i) == true)
				result.push(item);
		});
		return result;
	};

	$.extend(Array.prototype, {
		indexOf: function (o) {
			for (var i = 0, len = this.length; i < len; i++) {
				if (this[i] == o) {
					return i;
				}
			}
			return -1;
		}, remove: function (o) {
			var index = this.indexOf(o);
			if (index != -1) {
				this.splice(index, 1);
			}
			return this;
		}, removeById: function (filed, id) {
			for (var i = 0, len = this.length; i < len; i++) {
				if (this[i][filed] == id) {
					this.splice(i, 1);
					return this;
				}
			}
			return this;
		}
	});


});


String.prototype.trim = function () {
	return this.replace(/(^\s*)|(\s*$)/g, "");
}


window.saveToLocalStorage = function (key, value) {
	window.localStorage.setItem(key, JSON.stringify(value));
};

window.getFromLocalStorate = function (key) {
	var value = window.localStorage.getItem(key);
	if (value != null)
		return JSON.parse(value);
	return value;
};
