function Log(d) {
    this.d = $(d);
    this.dom = {};
}

Log.prototype.init = function () {
	var _this = this;
	this.dom.beginDate = $("#beginDate").val(G.util.getDate($.now())).datepicker();
	this.dom.endDate = $("#endDate").val(G.util.getDate($.now())).datepicker();
	this.dom.tbody = $("#tbody");
	this.dom.bd_type = $("#bd_type");
	G.util.hover(this.dom.tbody, "tr");
	this.template_loadip = $("#template_loadip").html();
	this.dom.form = $("form", this.d);
	this.dom.form.validate({
	    rules: {
	        begin: {
	            dateISO: function () {
	                return this.value !== "";
	            }
	        },
	        end: {
	            dateISO: function () {
	                return this.value !== "";
	            }
	        }
	    },
		submitHandler : function(){
			_this.doQuery();
		},
		showErrors: function (errorMap, errorList) {
		    if (!errorList.length) {
		        return;
		    }
		    $(errorList[0].element).tooltip({ html: errorList[0].message });
		}
	});
	this.dom.today = $("#today").on("click", function(){
		_this.getToday();
	});
	this.dom.yestoday = $("#yestoday").on("click", function(){
		_this.getYestoday();
	});
	this.dom.thisweek = $("#thisweek").on("click", function(){
		_this.getThisweek();
	});
	this.dom.lastweek = $("#lastweek").on("click", function(){
		_this.getLastweek();
	});
	this.d.on("click", ".fn-loadip", function () {
	    _this.viewLoadIp($(this).attr("account"));
	});
	this.getTypes();
};

//获取操作类型
Log.prototype.getTypes = function () {
    var _this = this;
    G.get({
        url: "/Log/GetDictLogTypeTable",
        success: function (x) {
            var str = "";
            $(x.Data).each(function (i, item) {
                str += '<label class="mr20" for="type_' + item.dict_log_type_id + '"><input type="radio" class="radio" id="type_' + item.dict_log_type_id + '" name="type" value="' + item.dict_log_type_id + '" />' + item.log_type_name + '</label>';
            });
            _this.dom.bd_type.append(str);
            str = null;
        }
    });
};

//今天
Log.prototype.getToday = function(){
	this.dom.beginDate.val(G.util.getDate());
	this.dom.endDate.val(G.util.getDate());
};

//昨天
Log.prototype.getYestoday = function(){
	var time = $.now() - 86400000;
	this.dom.beginDate.val(G.util.getDate(time));
	this.dom.endDate.val(G.util.getDate(time));
};

//本周
Log.prototype.getThisweek = function(){
	var date = new Date(), range, week = date.getDay();
	range = 86400000 * (week == 0 ? 6 : week - 1);
	this.dom.beginDate.val(G.util.getDate($.now() - range));
	this.dom.endDate.val(G.util.getDate($.now()));
};

//上周
Log.prototype.getLastweek = function(){
	var date = new Date(), range, week = date.getDay();
	range = 86400000 * (week == 0 ? 6 : week - 1);
	this.dom.beginDate.val(G.util.getDate($.now() - range - 7 * 86400000));
	this.dom.endDate.val(G.util.getDate($.now() - range - 86400000));
};

//查询
Log.prototype.doQuery = function(){
	var _this = this;
	doc.triggerHandler("pageinit", [_this.dom.form.serialize()]);
};

Log.prototype.viewLoadIp = function (account) {
    $.dialog({
        title : account + "的登录IP日志",
        module: "log",
        htmlurl: this.template_loadip,
        jsonurl: "loadip",
        param: { TargetAccount: account },
        joindata: { TargetAccount: account }
    });
};

G.modules.log = Log;
