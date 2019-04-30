function Account(d) {
    this.d = $(d);
    this.dom = {};
}

Account.prototype.init = function () {
    var _this = this;
};

G.modules.account = Account;

function AccountList(d) {
    this.d = $(d);
    this.dom = {};
}

AccountList.prototype.init = function () {
    var _this = this;
    this.dom.tbody = $("#tbody");
    this.dom.form = $("form");
    this.dom.form.validate({
        submitHandler: function () {
            _this.doQuery();
        }
    });
    this.dom.btnSubmit = $("#btn-submit");
    G.util.hover(this.dom.tbody, "tr");
    this.d.on("click", ".fn-enable", function () {
        _this.doEnable(this);
    });
    this.d.on("click", ".fn-stop", function(){
        _this.doStop(this);
    });
};

AccountList.prototype.doStop = function(t){
    var id = t.parentNode.parentNode.id, enable = t.getAttribute("value"), strfn, value, prev, is_enabled;
    $.confirm("确定对该账号进行 <span class='red'>" + (enable == "0" ? "恢复" : "停押") + "</span> 操作吗？", function () {
        G.post({
            url: "/Account/StopCharge",
            data: { memberId: id, isStopCharge: enable },
            success: function () {
                strfn = enable == "0" ? "停押" : "恢复";
                value = enable == "0" ? "1" : "0";
                prev = $(t).parent().prev().attr("is_stop_charge", enable);
                is_enabled = prev.attr("is_enabled");
                if (is_enabled == "1") {
                    if(enable == "1"){
                        prev.html("<span class='red'>停押</span>");
                    } else {
                        prev.html("启用");
                    }
                }
                $(t).attr("value", value).html(strfn);
                G.util.tip();
            }
        });
    });
};

AccountList.prototype.doEnable = function (t) {
    var id = t.parentNode.parentNode.id, enable = t.getAttribute("value"), status, strfn, value, prev, is_stop_charge;
    $.confirm("确定对该账号进行 <span class='red'>" + (enable == "0" ? "禁用" : "启用") + "</span> 操作吗？", function () {
        G.post({
            url: "/Account/Enable",
            data: { memberId: id, enabled: enable },
            success: function () {
                strfn = enable == "0" ? "启用" : "停用";
                value = enable == "0" ? "1" : "0";
                prev = $(t).parent().prev().attr("is_enabled", enable);
                is_stop_charge = prev.attr("is_stop_charge");
                if (enable == "1") {
                    if (is_stop_charge == "1") {
                        prev.html("<span class='red'>停押</span>");
                    } else {
                        prev.html("启用");
                    }
                } else {
                    prev.html("<span class='red'>禁用</span>");
                }
                $(t).attr("value", value).html(strfn);
                G.util.tip();
            }
        });
    });
};

AccountList.prototype.doQuery = function () {
    doc.triggerHandler("pageinit", [this.dom.form.serialize()]);
};

G.modules.account_list = AccountList;


//增加下级和直属会员
function AccountAdd(d) {
    this.d = $(d);
    this.dom = {};
}

AccountAdd.prototype.init = function () {
    var _this = this;
    this.dom.MemberId = $("#MemberId");
    this.dom.MemberLevel = $("#MemberLevel");
    this.dom.IsSubAccount = $("#IsSubAccount");
    this.dom.MaxAvailablePumpRate = $("#MaxAvailablePumpRate");
    this.dom.Credit = $("#Credit");
    this.dom.dup_credit = $("#dup_credit").val(G.util.format_thousands(this.dom.Credit.val()));
    G.util.input_format(this.dom.dup_credit, {target : this.dom.Credit});
    this.dom.isPumpEnabled = $("[name=isPumpEnabled]", this.d).on("click", function () {
        _this.dom.MaxAvailablePumpRate[0].className = this.value == "1" ? "" : "hide";
    });
    this.dom.Account = $("#Account").on("focusout", function () {
        if ($.trim(this.value)) {
            var __this = $(this);
            __this.next().remove();
            G.get({
                url: "/Account/IsExisted",
                data: { Account: this.value },
                success: function(){
                    __this.next().remove();
                },
                bussiness: function (data) {
                    __this.after(" <label class='red'>"+ data +"</label>");
                }
            });
        }
    });
    this.dom.form = $("form");
    this.dom.form.validate({
        rules : {
            Account : {
                required: true
            },
            Nickname : {
                required : true
            },
            LoginPwd : {
                required: function () {
                    return _this.dom.MemberId.val() === "";
                },
                minlength : 6
            },
            ReLoginPwd : {
                required: function () {
                    return _this.dom.MemberId.val() === "";
                },
                equalTo : "#LoginPwd"
            },
            PinPwd: {
                required: function () {
                    return _this.dom.MemberId.val() === "";
                },
                minlength: 6,
                digits: true
            },
            RePinPwd: {
                required: function () {
                    return _this.dom.MemberId.val() === "";
                },
                equalTo: "#PinPwd"
            },
            TransferPwd : {
                required: function () {
                    return _this.dom.MemberId.val() === "";
                },
                minlength : 6
            },
            ReTransferPwd : {
                required: function () {
                    return _this.dom.MemberId.val() === "";
                },
                equalTo : "#TransferPwd"
            },
            Credit : {
                required : true,
                digits: true
            },
            ReturnRate : {
                required : true,
                range : [0, 100]
            },
            PumpRate : {
                required: true,
                range: [0, 100]
            },
            MaxAvailablePumpRate: {
                required: function () {
                    return !_this.dom.MaxAvailablePumpRate.hasClass("hide");
                },
                range: [0, 100]
            }
        },
        submitHandler: function () {
            _this.doSave();
        },
        showErrors: function (errorMap, errorList) {
            if (!errorList.length) {
                return;
            }
            $(errorList[0].element).tooltip({ html: errorList[0].message });
        }
    });
    this.dom.btnSubmit = $("#btn-submit");
    this.dom.parentList = $("#parentList").change(function () {
        var item = _this.parent[this.value];
        $("#pumprate_parent").html(G.mapping_name.level[item.member_level] + "抽水比例");
        $(this).addClass("hide").next().html(item.account + "(" + G.mapping_name.level[item.member_level] + ")").parent().next().removeClass("hide");
        _this.getParentMaxCredit(this.value);
    });
    if (!this.dom.parentList.parent().hasClass("hide")) {
        this.getParent();
    }
    this.dom.btnCancel = $("#btn-cancel").on("click", function () {
        history.go(-1);
    });
    
    if (this.dom.IsSubAccount.val() == 1) {
        this.template_privilege = $("#template_privilege").html();
        this.dom.bd_power = $("#bd_power");
        this.getPower();
    }
    if (this.dom.parentList.is(":hidden")) {
        this.getParentMaxCredit(this.dom.parentList.val());
    }
};

AccountAdd.prototype.getParentMaxCredit = function (parentId) {
    G.get({
        url: "/Account/GetMemberAvailableInfo",
        data: { parentId: parentId, memberId: this.dom.MemberId.val(), level: this.dom.MemberLevel.val() },
        success: function (x) {
            $("#MaxCredit").html(G.util.format_thousands(x.Data.CreditBalance)).parent().removeClass("hide");
            $("#MaxRetrunRate").html(G.util.format_thousands(x.Data.ReturnRate)).parent().removeClass("hide");
            $("#MaxPumpRate").html(G.util.format_thousands(x.Data.PumpRate)).parent().removeClass("hide");
            $("#MaxAvailablePumpRate2").html(G.util.format_thousands(x.Data.MaxAvailablePumpRate)).parent().removeClass("hide");
            if (x.Data.IsPumpEnabled == 0) {
                $("#MaxPumpRate").parents("tr").remove();
            }
        }
    });
};

AccountAdd.prototype.getPower = function () {
    loadPage({
        htmlurl: this.template_privilege,
        jsonurl: "/Account/GetSubaccountPrivilegeTable",
        container: this.dom.bd_power,
        param: { memberId: this.dom.MemberId.val() },
        jsonSuccess: function (x) {
            var i = 0, len = x.Data.Privileges.length;
            $(x.Data.SubaccountPrivileges).each(function () {
                for (; i < len; i++) {
                    if (this.dict_privilege_id == x.Data.Privileges[i].dict_privilege_id) {
                        x.Data.Privileges[i].selected = true;
                        break;
                    }
                }
            });
        }
    });
};

AccountAdd.prototype.getParent = function(){
    var _this = this;
    G.get({
        url: "/Account/GetParentTable",
        data: { MemberLevel: _this.dom.MemberLevel.val() },
        success: function (x) {
            var str = "", level;
            _this.parent = {};
            $(x.Data).each(function (i, item) {
                _this.parent[item.member_id] = item;
                if (!level) {
                    level = item.member_level;
                    str += "<optgroup label='" + G.mapping_name.level[item.member_level] + "'>";
                }
                if (level == item.member_level){
                    str += "<option value='" + item.member_id + "'>" + item.account + "</option>";
                } else {
                    level = item.member_level;
                    str += "</optgroup><optgroup label='" + G.mapping_name.level[item.member_level] + "'>";
                    str += "<option value='" + item.member_id + "'>" + item.account + "</option>";
                }
                if (i == x.Data.length - 1) {
                    str += "</optgroup>";
                }
            });
            _this.dom.parentList.append(str);
            str = null;
        }
    });
};

AccountAdd.prototype.doSave = function () {
    var url, arr = [];
    if (this.dom.IsSubAccount.val() == 1) {
        url = this.dom.MemberId.val() ? "/Account/EditSubAccount" : "/Account/AddSubAccount";
        $(":checkbox:checked", this.dom.bd_power).each(function () {
            arr.push(this.id);
        });
        $("#privilegeIds").val(arr.join(","));
    } else {
        url = this.dom.MemberId.val() ? "/Account/Edit" : "/Account/Add";
    }
    G.post({
        url : url,
        data : this.dom.form.serialize(),
        success : function(){
            $.alert("保存成功。", function () {
                history.go(-1);
            });
        }
    });
};

AccountAdd.prototype.destroy = function () {
    this.parent = null;
};

G.modules.account_add = AccountAdd;


//转账
function AccountTransfer(d){
	this.d = $(d);
    this.dom = {};
}

AccountTransfer.prototype.init = function(){
	var _this = this;
    this.dom.btnCancel = $("#btn-cancel").on("click", function(){
        history.go(-1);
    });
    this.dom.guid = $("#guid").val(G.util.guid());
    this.dom.btnSubmit = $("#btn-submit");
    this.dom.TransferMoney = $("#TransferMoney");
    this.dom.dup_transfermoney = $("#dup_transfermoney");
    G.util.input_format(this.dom.dup_transfermoney, { target: this.dom.TransferMoney });
	this.dom.form = $("form", this.d);
	this.dom.form.validate({
        rules : {
            TransferMoney: {
                required : true,
                number: true
            },
            TransferPwd: {
                required : true
            }
        },
		submitHandler : function(){
			_this.doSave();
		},
        showErrors: function (errorMap, errorList) {
            if (!errorList.length) {
                return;
            }
            $(errorList[0].element).tooltip({ html: errorList[0].message });
        }
	});
};

AccountTransfer.prototype.doSave = function () {
    var _this = this;
    this.dom.btnSubmit.prop("disabled", true);
    G.post({
        url: "/Account/TransferMoney",
        data : this.dom.form.serialize(),
        success : function(){
            $.alert("转账成功", function(){
                history.go(-1);
            });
        },
        complete: function () {
            _this.dom.btnSubmit.prop("disabled", false);
        }
    });
};

G.modules.account_transfer = AccountTransfer;

function Iplimit(d) {
    this.d = $(d);
    this.dom = {};
    this.arr_ip = [];
}

Iplimit.prototype.init = function () {
    var _this = this;
    this.dom.form = $("form", this.d);
    this.dom.form.validate({
        submitHandler: function () {
            _this.doSave();
        }
    });
    this.dom.btnCancel = $("#btn-cancel").on("click", function () {
        history.go(-1);
    });
    this.dom.ips = $("#ips");
};

Iplimit.prototype.doSave = function () {
    var ips = [];
    if (!/^(\d{1,3}.\d{1,3}.\d{1,3}.\d{1,3}(\,\d{1,3}.\d{1,3}.\d{1,3}.\d{1,3})*)?$/.test(this.dom.ips.val())) {
        this.dom.ips.tooltip({ html: "ip格式填写不正确，请检查格式!" });
        return false;
    }
    G.post({
        url: "/Account/EditSubaccountIpLimit",
        data: this.dom.form.serialize(),
        success: function () {
            $.alert("保存成功!", function () {
                history.go(-1);
            });
        }
    });
};

G.modules.iplimit = Iplimit;