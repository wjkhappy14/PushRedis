function Report(d) {
    this.d = $(d);
    this.dom = {};
}

Report.prototype.init = function () {
	var _this = this;
	this.dom.beginDate = $("#beginDate").val(G.util.getDate($.now())).datepicker();
	this.dom.endDate = $("#endDate").val(G.util.getDate($.now())).datepicker();
	this.dom.period = $("#period");
	this.dom.tbody = $("#tbody");
	G.util.hover(this.dom.tbody, "tr");
	this.dom.bd_period = $("#bd-period");
	this.template_period = $("#template_period").html();
	this.dom.form = $("form", this.d);
	this.dom.form.validate({
	    rules : {
	        beginDate: {
	            required: function () {
	                return _this.dom.period.val() == "" || _this.dom.endDate.val() != "";
	            },
	            dateISO : true
	        },
	        endDate: {
	            required: function () {
	                return _this.dom.period.val() == "" || _this.dom.beginDate.val() != "";
	            },
	            dateISO: true
	        },
	        period: {
	            required: function () {
	                return _this.dom.beginDate.val() == "";
	            }
	        }
	    },
	    submitHandler: function () {
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
	this.d.on("click", ".fn-reportinfo", function () {
	    var __this = $(this);
	    if (!__this.attr("dict_poker_ids")) {
	        $.alert("该期尚未开奖!");
	        return false;
	    }
	    $.dialog({
	        title: "开奖结果",
            width : 750,
	        module: "report",
	        htmlurl: "report_info",
	        jsonurl: "report_info",
	        param: {
	            betid: __this.attr("betid"),
	            betdate: __this.attr("betdate"),
	            roomid: __this.attr("roomid"),
	            periodid: __this.attr("periodid"),
	            memberid: __this.attr("memberid")
	        },
	        jsonSuccess: function (json) {
	            return _this.filter(json);
	        }
	    });
	});
	this.type = 1;
	if (this.dom.beginDate.length) {
        this.getTodayPeriod(G.util.getDate($.now()), this.type);
    }
	doc.bind("datechange." + this.d.attr("id"), function () {
	    if (_this.dom.beginDate.val() == _this.dom.endDate.val()) {
	        _this.getTodayPeriod(_this.dom.beginDate.val(), _this.type);
	    } else {
	        $("#period")[0].options.length = 1;
	    }
	});
};

Report.prototype.getTodayPeriod = function (date, type) {
    loadPage({
        htmlurl: this.template_period,
        jsonurl: "/Report/ListPeriod",
        param: { date: date , type: type},
        container: this.dom.bd_period
    });
};

Report.prototype.filter = function (json) {
    var myMemberId = json.Data.Member.MemberId, obj = { length: 0 };
    var apperance = {};
    json.Data.RoomPoker.RoomName = this.roomname;
    json.Data.RoomPoker.RandomPokerIds = json.Data.RoomPoker.RandomPokerIds.split(",");
    json.Data.RoomPoker.DrawNumber = json.Data.RoomPoker.DrawNumber ? json.Data.RoomPoker.DrawNumber.split(",") : [];
    json.Data.RoomPoker.SelectedPokerIds = json.Data.RoomPoker.SelectedPokerIds ? json.Data.RoomPoker.SelectedPokerIds.split(",") : [];

    var i = 0, j = 0, BetInfo = json.Data.BetInfo, len, SeatInfo = json.Data.SeatInfo, item, TotalBetMoney, MySequenceIds = [], MyBetMoney = 0, SeatBetInfo = [], TotalBetCount, MyBetTotalMoney = 0, MyTotalProfit = 0;
    $.each(SeatInfo, function (i) {
        item = this;
        TotalBetMoney = 0;
        TotalBetCount = 0;
        MyBetMoney = 0;
        MySequenceIds = [];
        SeatBetInfo = [];
        for (j = 0, len = BetInfo.length; j < len; j++) {
            if (BetInfo[j].SeatId == item.SeatId) {
                TotalBetMoney += BetInfo[j].BetMoney;
                json.Data.RoomPoker.TotalBetMoney += BetInfo[j].BetMoney;
                SeatBetInfo.push(BetInfo[j]);
                TotalBetCount += 1;
                if (BetInfo[j].MemberId == myMemberId) {
                    MyBetMoney += BetInfo[j].BetMoney;
                    MyTotalProfit += BetInfo[j].TP;
                }
            }
        }
        item.TotalBetMoney = TotalBetMoney;
        item.MyBetMoney = MyBetMoney;
        MyBetTotalMoney += item.MyBetMoney;
        item.BetInfo = SeatBetInfo;
        $(item.BetInfo).each(function (n, m) {
            if (m.MemberId == myMemberId) {
                MySequenceIds.push(n + 1);
            }
        });
        MySequenceIds = MySequenceIds.length >= 3 ? MySequenceIds[0] + "..." + MySequenceIds[MySequenceIds.length - 1] : MySequenceIds.join(",");
        item.MySequenceIds = MySequenceIds;
        item.MyMemberId = myMemberId;
        item.PeriodId = json.Data.RoomPoker.PeriodId;
        item.RoomId = json.Data.RoomPoker.RoomId;
        item.TotalBetCount = TotalBetCount;
        item.PokerIds = item.PokerIds ? item.PokerIds.split(",") : [];
        item.Enable = json.Data.Other && json.Data.Other.CloseRemainSeconds != 0 ? 1 : 0;
        item.RoomInfo = json.Data.RoomPoker.RoomName + "/" + json.Data.RoomPoker.RoomId + "号台/" + json.Data.RoomPoker.TodayPeriod + "期";
    });
    json.Data.MyBetTotalMoney = MyBetTotalMoney;
    MyTotalProfit = parseFloat(MyTotalProfit.toFixed(2), 10);
    json.Data.RoomPoker.MyTotalProfit = MyTotalProfit;
    json.Data.RoomPoker.PokerIndex = [];
    json.Data.RoomPoker.realNumber = [];
    json.Data.RoomPoker.NumberTimes = {};
    json.Data.RoomPoker.SelectedPoker = []
    json.Data.RoomPoker.SelectedPokerName = [];
    $(json.Data.RoomPoker.SelectedPokerIds).each(function (a, b) {
        obj[this] = true;
        obj.length++;
        for (var k = 0; k < 6; k++) {
            if ($.inArray(b, SeatInfo[k].PokerIds) != -1) {
                json.Data.RoomPoker.PokerIndex.push(k + 1);
                break;
            }
        }
        json.Data.RoomPoker.realNumber.push(json.Data.RoomPoker.DrawNumber[a + 1]);
        json.Data.RoomPoker.SelectedPoker.push(b);
        json.Data.RoomPoker.SelectedPokerName.push(G.poker(b));
    });
    json.Data.RoomPoker.SelectedPokerIds = obj;
    obj = null;

    len = json.Data.RoomPoker.SelectedPokerIds.length;
    $.each(json.Data.RoomPoker.DrawNumber, function (a, b) {
        if (a < len) {
            if (!apperance[b % 52]) {
                json.Data.RoomPoker.NumberTimes[parseInt(b, 10)] = 1;
                apperance[b % 52] = 1;
            } else {
                json.Data.RoomPoker.NumberTimes[parseInt(b, 10)] = 2;
            }
        }
    });

    json.Data.RoomPoker.realNumber = null;

    json.Data.Enable = json.Data.Other && json.Data.Other.CloseRemainSeconds != 0 ? 1 : 0;
    json.Data.BetInfo = null;
    delete json.Data.BetInfo;
    return json;
};

//今天
Report.prototype.getToday = function(){
	this.dom.beginDate.val(G.util.getDate());
	this.dom.endDate.val(G.util.getDate());
	this.getTodayPeriod(G.util.getDate(), this.type);
};

//昨天
Report.prototype.getYestoday = function(){
	var time = $.now() - 86400000;
	this.dom.beginDate.val(G.util.getDate(time));
	this.dom.endDate.val(G.util.getDate(time));
	this.getTodayPeriod(G.util.getDate(time), this.type);
};

//本周
Report.prototype.getThisweek = function(){
	var date = new Date(), range, week = date.getDay();
	range = 86400000 * (week == 0? 6 : week - 1);
	this.dom.beginDate.val(G.util.getDate($.now() - range));
	this.dom.endDate.val(G.util.getDate($.now()));
	$("#period")[0].options.length = 1;
};

//上周
Report.prototype.getLastweek = function(){
	var date = new Date(), range, week = date.getDay();
	range = 86400000 * (week == 0 ? 6 : week - 1);
	this.dom.beginDate.val(G.util.getDate($.now() - range - 7 * 86400000));
	this.dom.endDate.val(G.util.getDate($.now() - range - 86400000));
	$("#period")[0].options.length = 1;
};

//查询
Report.prototype.doQuery = function () {
    var start = this.dom.beginDate.val(), end = this.dom.endDate.val(), period = $("#period").val(), betdone = $("input:radio:checked").val();
    location.hash = '#module=report|htmlurl=report_list|jsonurl=report_list|param={"start":"' + start + '","end":"' + end + '","period":"' + period + '","betdone":"' + betdone + '"}';
};

G.modules.report = Report;
G.modules.report_member = Report;
G.modules.report_detail = Report;
