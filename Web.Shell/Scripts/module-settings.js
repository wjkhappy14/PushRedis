function Settings(d) {
    this.d = $(d);
    this.dom = {};
    this.id = this.d.attr("id");
    this.url_save = {
        closestatus: "/System/ChangeCloseState",
        marquee: "/Marquee/Edit"
    }
}

Settings.prototype.init = function () {
    var _this = this;
    this.dom.form = $("form", this.d);
    this.dom.form.validate({
        rules:{
        },
        showErrors: function (errorMap, errorList) {
            if (!errorList.length) {
                return;
            }
            $(errorList[0].element).tooltip({ html: errorList[0].message });
        },
        submitHandler: function () {
            _this.doSave();
        }
    });
    if (this.d.attr("id") == "closestatus") {
        this.dom.bd_closestatus = $("#bd_closestatus");
        this.template_closestatus = $("#template_closestatus").html();
        this.getCloseState();
    }
    
};

Settings.prototype.getCloseState = function () {
    loadPage({
        name: "settings",
        htmlurl: this.template_closestatus,
        jsonurl: "get_close_status",
        container: this.dom.bd_closestatus
    });
};

Settings.prototype.doSave = function () {
    var _this = this;
    if (this.id == "closestatus") {
        if ($("input:radio:checked", this.d).val() == document.getElementById("crt_status").value) {
            $.alert("未进行任何状态修改");
            return false;
        }
    }
    G.post({
        url: this.url_save[this.id],
        data: this.dom.form.serialize(),
        success: function () {
            $.alert("保存成功。");
            _this.doSuccess();
        }
    });
};

Settings.prototype.doSuccess = function () {
    if (this.id == "closestatus") {
        this.getCloseState();
        loadPage({
            name: "settings",
            container: main
        });
    }
};

G.modules.closestatus = Settings;
G.modules.marquee = Settings;