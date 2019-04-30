function Monitor(d) {
    this.d = $(d);
    this.dom = {};
    this.countdown = G.util.getCookie("monitor_refresh_time")? parseInt(G.util.getCookie("monitor_refresh_time"), 10) : 10;
    this.roomtype = null;
    this.timer = null;
    this.enable = true;
    this.areaid = "";
    this.timer_remain = {};
}

Monitor.prototype.init = function () {
    var _this = this;
    this.dom.dictAreaId = $("#DictAreaId").on("change", function () {
        _this.areaid = this.value;
        _this.doRefresh(0);
    });
    this.template_roomtype = $("#template_roomtype").html();
    this.dom.tbody = $("#tbody");
    this.dom.times = $("#times").on("change", function () {
        G.util.setCookie("monitor_refresh_time", this.value);
        _this.countdown = parseInt(this.value, 10);
        _this.doRefresh(0);
    }).val(G.util.getCookie("monitor_refresh_time") || this.countdown);
    G.util.hover(this.dom.tbody, "tr");
    this.template_detail = $("#template_detail").html();
    this.template_total = $("#template_total").html();
    this.template_list = $("#template_list").html();
    this.dom.btnRefresh = $("#btn-refresh").on("click", function () {
        _this.doRefresh(0);
    });
	this.dom.bd_list = $("#bd_list").on("click", ".fn-view", function () {
	    _this.viewDetail(this);
	});
	this.dom.roomtype = $("#roomtype").on("click", "a", function () {
	    _this.roomtype = this.id;
	    $(this).addClass("red").siblings().removeClass("red");
	    _this.doRefresh(0);
	});
	this.getTotal();
	this.doRefresh(G.util.getCookie("monitor_refresh_time") || this.countdown);
	this.getAreaId();
	this.getRoomType();
};

Monitor.prototype.getAreaId = function () {
    var _this = this;
    G.get({
        url: "/monitor/GetDictAreaList",
        success: function (x) {
            $.each(x.Data, function (i, item) {
                _this.dom.dictAreaId.append("<option value='" + item.DictAreaId + "'>" + item.AreaName + "</option>");
            });
        }
    });
};

Monitor.prototype.getRoomType = function () {
    loadPage({
        htmlurl: this.template_roomtype,
        jsonurl: "/monitor/GetRoomType",
        container: $("#roomtype")
    });
};

Monitor.prototype.getTotal = function () {
    var _this = this;
    loadPage({
        name: "monitor",
        htmlurl: this.template_total,
        jsonurl: "total",
        container: $("#sidebar-right"),
        renderSuccess: function (json) {
            $.each(json.Data, function (i, item) {
                _this.doRemainTime(item.Second, document.getElementById("remain" + i), i);
            });
        }
    });
};

Monitor.prototype.doRefresh = function (countdown) {
    var _this = this;
    if (this.timer) {
        clearTimeout(this.timer);
        this.timer = null;
    }
    if (countdown === 0) {
        this.changeRoom({ id: this.roomtype }, function () {
            _this.getTotal();
            countdown = _this.countdown;
            _this.doRefresh(countdown);
        });
    }
    else{
        this.dom.btnRefresh.val("刷新(" + countdown + ")");
        countdown -= 1;
        this.timer = setTimeout(function () {
            _this.doRefresh(countdown);
        }, 1000);
    }
};

Monitor.prototype.doRemainTime = function (remain, target, index) {
    var _this = this;
    if (this.timer_remain[index]) {
        clearTimeout(this.timer_remain[index]);
        this.timer_remain[index] = null;
    }
    if (!remain || remain <= 0) { remain = 0; }
    this.setCountDown(remain, target);
    if (!remain) {
        return;
    } else {
        remain -= 1;
        this.timer_remain[index] = setTimeout(function () {
            _this.doRemainTime(remain, target, index);
        }, 1000);
    }
};

//设置倒计时
Monitor.prototype.setCountDown = function (remain, target) {
    target.innerHTML = G.util.formatSeconds(remain);
};

Monitor.prototype.changeRoom = function (opt, callback) {
    var _this = this, id = opt.id;
    loadPage({
        name: "monitor",
        htmlurl: this.template_list,
        jsonurl: "monitor",
        container: this.dom.bd_list,
        param: { dictRoomTypeId: id, dictAreaId: _this.areaid },
        jsonSuccess: function (x) {
            if(callback){
                callback();
            }
        }
    });
};

Monitor.prototype.viewDetail = function (t) {
    var _this = this,
        param = {
            roomId: t.parentNode.parentNode.getAttribute("roomid"),
            dictSeatId : t.parentNode.getAttribute("seatid")
        };
    $.dialog({
        title : "投注明细",
        module : "monitor",
        htmlurl: this.template_detail,
        jsonurl: "detail",
        param: param,
        width:500
    });
};

Monitor.prototype.destroy = function () {
    var _this = this;
    if (this.timer) {
        clearTimeout(this.timer);
        this.timer = null;
    }
    $.each(this.timer_remain, function (i) {
        if (_this.timer_remain[i]) {
            clearTimeout(_this.timer_remain[i]);
            _this.timer_remain[i] = null;
        }
    });
    
};

G.modules.monitor = Monitor;
