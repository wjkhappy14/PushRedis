G.mapping_name = {
    level: {
        9: "公司",
        5: "大股东",
        4: "股东",
        3: "总代理",
        2: "代理",
        1: "会员"
    }
};

G.poker = function (index) {
    var poker;
    switch (index) {
        case "1":
        case "2":
        case "3":
        case "4":
            poker = "A";
            break;
        case "5":
        case "6":
        case "7":
        case "8":
            poker = "2";
            break;
        case "9":
        case "10":
        case "11":
        case "12":
            poker = "3";
            break;
        case "13":
        case "14":
        case "15":
        case "16":
            poker = "4";
            break;
        case "17":
        case "18":
        case "19":
        case "20":
            poker = "5";
            break;
        case "21":
        case "22":
        case "23":
        case "24":
            poker = "6";
            break;
        case "25":
        case "26":
        case "27":
        case "28":
            poker = "7";
            break;
        case "29":
        case "30":
        case "31":
        case "32":
            poker = "8";
            break;
        case "33":
        case "34":
        case "35":
        case "36":
            poker = "9";
            break;
        case "37":
        case "38":
        case "39":
        case "40":
            poker = "10";
            break;
        case "41":
        case "42":
        case "43":
        case "44":
            poker = "J";
            break;
        case "45":
        case "46":
        case "47":
        case "48":
            poker = "Q";
            break;
        case "49":
        case "50":
        case "51":
        case "52":
            poker = "K";
            break;
    }
    return poker;
};

template.helper("getPoker", function (a) {
    return G.poker(a);
});

template.helper("format_poker_ids", function (a) {
    if (!a) { return ""; }
    var arr = a.split(","), str = "", i = 0;
    for (; i < 3; i++) {
        str += "<span class='s-poker p" + arr[i] + "'>" + G.poker(arr[i]) + "</span> ";
    }
    return str;
});
template.helper("getLevelName", function (a) {
    return G.mapping_name.level[a];
});
template.helper("formatRemainSeconds", function (a) {
    a = parseInt(a, 10);
    return G.util.formatSeconds(a);
});
template.helper("format_account_number", function (a) {
    if (/<br\/>/.test(a)) {
        a = a.split("<br/>");
        if (a.length == 2) {
            a.unshift(null);
        }
        a = (a[0] ? a[0] + "<br/>" : "") + "<span class='blue' title='退水比例'>" + parseFloat(parseFloat(a[1], 10).toFixed(2), 10) + "%</span>" + (a[2] ? "/<span class='red' title='抽水比例'>" + parseFloat(parseFloat(a[2], 10).toFixed(2), 10) + "%</span>" : "");
    } else {
        a = a ? parseFloat(parseFloat(a, 10).toFixed(2), 10) + "%" : "-";
    }
    return a;
});
template.helper("format_number", function (a, b) {
    if (a === "" || a === undefined) { return "-"; }
    if (b == 1) {
        if (/^\-\d+/.test(a)) {
            return "<span class='green'>" + G.util.format_number(a) + "</span>";
        } else {
            return "<span class='red'>" + G.util.format_number(a) + "</span>";
        }
    } else {
        return G.util.format_number(a);
    }
    //return G.util.format_number(a) + b;
});
//配置默认模块
G.InitHash = "#module=account";

G.map = {
    account: {
        html: {
            account: "/Htmls/account-list.html",
            account_add: "/Htmls/account-add.html",
            transfer: "/Htmls/account-transfer.html",
            transferRecord: "/Htmls/account-transferRecord.html",
            iplimit: "/Htmls/account-iplimit.html"
        },
        json: {
            account: "/Account/GetAccount",
            edit: "/Account/GetModel",
            transfer: "/Account/GetTransferInfo",
            transferRecord: "/Account/GetTransferRecord",
            sub: "/Account/GetSubAccount",
            edit_sub: "/Member/EditSubAccountModel",
            iplimit: "/Account/GetSubaccountIpLimitModel"
        }
    },
    report: {
        html: {
            report: "/Htmls/report.html",
            report_list: "/Htmls/report-list.html",
            report_member: "/Htmls/report-member.html",
            report_detail: "/Htmls/report-detail.html",
            report_info: "/Htmls/report-info.html"
        },
        json: {
            report_list: "/Report/GetData",
            report_detail: "/Report/GetDataBetDetail",
            report_info: "/RoomPoker/GetDrawDetail"
        }
    },
    monitor: {
        html: {
            monitor: "/Htmls/monitor.html",
            betDetail: "/Htmls/monitor-betDetail.html"
        },
        json: {
            monitor: "/Monitor/GetRoom",
            total: "/Monitor/GetTotal",
            detail: "/Monitor/GetDetail"
        }
    },
    info: {
        html: {
            info: "/Htmls/info.html"
        },
        json: {
            info: "/Personinfo/Getpersoninfo"
        }
    },
    notice: {
        html: {
            notice: "/Htmls/notice-list.html",
            notice_edit: "/Htmls/notice-edit.html"
        },
        json: {
            notice: "/Notice/GetPageData",
            notice_edit: "/Notice/GetModel"
        }
    },
    log: {
        html: {
            log: "/Htmls/log-list.html"
        },
        json: {
            log: "/Log/GetLogPageData",
            loadip: "/log/GetLoginLogPageData"
        },
        compile: {

        }
    },
    password: {
        html: {
            password: "/Htmls/password.html"
        },
        json: {}
    },
    settings: {
        html: {
            settings: "/Htmls/settings.html",
            marquee: "/Htmls/settings-marquee.html"
        },
        json: {
            settings: "/System/GetCloseLog",
            get_close_status: "/System/getCloseState",
            marquee: "/Marquee/GetEditModel"
        }
    }
};

G.hashFn = {
    supper: function (hash_module) {
        var ele_name = hash_module.module;
        G.util.load({
            container: main,
            name: ele_name,
            param: hash_module.param || {},
            htmlurl: hash_module.htmlurl,
            jsonurl: hash_module.jsonurl,
            joindata: hash_module.joindata
        });
        $("#nav li[name=" + ele_name + "]").addClass("on").siblings().removeClass("on");
    }
};

$(["account", "log", "report", "monitor", "notice", "info", "password", "settings"]).each(function (i, name) {
    G.hashFn[name] = G.hashFn.supper;
});
