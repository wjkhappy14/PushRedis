function Notice(d) {
    this.d = $(d);
    this.dom = {};
}

Notice.prototype.init = function () {
    var _this = this;
    this.dom.tbody = $("#tbody");
    G.util.hover(this.dom.tbody, "tr");
	this.d.on("click", ".fn-delete", function () {
	    var othis = this;
	    $.confirm("确定删除该项吗？", function () {
	        G.post({
	            url: "/Notice/Delete",
	            data: { noticeId: othis.id },
	            success: function (d) {
	                $(othis).parents("tr").remove();
	            }
	        });
	    });
	});
};

G.modules.notice = Notice;

function NoticeEdit(d) {
    this.d = $(d);
    this.dom = {};
}

NoticeEdit.prototype.init = function () {
    var _this = this;
    this.dom.NoticeId = $("#NoticeId");
    this.dom.btnSubmit = $("#btn-submit");
    this.dom.btnCancel = $("#btn-cancel").on("click", function () {
        history.go(-1);
    });
    this.dom.memberLevels = $("#memberLevels");
    this.dom.memberLevelList = $(":checkbox[id^=memberLevels]", this.d);
    this.dom.form = $("form", this.d);
    this.dom.form.validate({
        rules: {
            Title : {
                required : true
            },
            Content: {
                required: true,
                rangelength: [1, 150]
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
};

NoticeEdit.prototype.doSave = function () {
    var _this = this, memberLevels = [];
    var postUrl = this.dom.NoticeId.val() ? "/Notice/Edit" : "/Notice/Add";
    this.dom.memberLevelList.filter(":checked").each(function () {
        memberLevels.push(this.value);
    });
    if (!memberLevels.length) {
        $.alert("请设置公告权限");
        return false;
    }
    this.dom.memberLevels.val(memberLevels.join());
    this.dom.btnSubmit.prop("disabled", true);
    G.post({
        url: postUrl,
        data: _this.dom.form.serialize(),
        success: function () {
            $.alert("保存成功。", function () {
                history.go(-1);
            });
        },
        complete: function () {
            _this.dom.btnSubmit.prop("disabled", false);
        }
    });
    memberLevels = [];
    memberLevels = null;
};

G.modules.notice_edit = NoticeEdit;

function Info(d) {
    this.d = $(d);
    this.dom = {};
}

Info.prototype.init = function () {
    this.template_transferRecord = $("#template_transferRecord").html();
    this.dom.bd_transferRecord = $("#bd-transferRecord");
    loadPage({
        htmlurl: this.template_transferRecord,
        jsonurl: "/PersonInfo/TransferRecord",
        container: this.dom.bd_transferRecord
    });
};

G.modules.info = Info;