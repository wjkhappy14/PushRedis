function Password(d) {
    this.d = $(d);
    this.dom = {};
}

Password.prototype.init = function () {
    var _this = this;
    this.template_pin = $("#template_pin").html();
    this.dom.btnSubmit = $("#btn-submit");
    this.dom.form = $("form", this.d);
    this.dom.form.validate({
        rules: {
            oldPwd: {
                required: true,
                minlength: 6
            },
            newPwd: {
                required: true,
                minlength: 6
            },
            repeatNewPwd: {
                required: true,
                equalTo: "#newPwd"
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
    this.d.on("click", ".fn-changepin", function () {
        $.dialog({
            title: "修改PIN码",
            width: 600,
            htmlurl: _this.template_pin
        });
    });
};

Password.prototype.doSave = function () {
    this.dom.btnSubmit.prop("disabled", true);
    G.post({
        url: "/Member/ChangePassword",
        data: this.dom.form.serialize(),
        success: function () {
            $.alert("保存成功。", function () {
                location.hash = G.InitHash;
            });
        },
        complete: $.proxy(function () {
            this.dom.btnSubmit.prop("disabled", false);
        }, this)
    });
};

G.modules.password = Password;

function Pin(d) {
    this.d = $(d);
    this.dom = {};
}

Pin.prototype.init = function () {
    var _this = this;
    this.dom.btnSubmit = $("#btn-submit", this.d);
    this.dom.form = $("form", this.d);
    this.dom.form.validate({
        rules: {
            pin: {
                required: true,
                minlength: 6
            },
            repeatPin: {
                required: true,
                equalTo: "#pin",
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
        },
        focusInvalid: false
    });
    this.dom.password = $(":password", this.d);
    this.dom.inputNumber = $(".inputNumber", this.d).each(function (i) {
        this.innerHTML = _this.getLayout();
        $(this).on("click", function (e) {
            var t = e.target;
            if (t.className) {
                if (t.className == "fn-clear") {
                    _this.dom.password[i].value = "";
                }
            } else {
                _this.dom.password[i].value = _this.dom.password[i].value.length < 6 ? _this.dom.password[i].value + t.innerHTML : _this.dom.password[i].value;
            }
        });
    });
    this.d.on("click", ".btn-close", function () {
        _this.close();
    });
};

Pin.prototype.getLayout = function () {
    var arr = [1, 2, 3, 4, 5, 6, 7, 8, 9, 0], randomsort = function (a, b) {
        return Math.random() > 0.5 ? -1 : 1;
    };
    arr = arr.sort(randomsort);
    var i = 0, str = "";
    for (; i < 10; i++) {
        str += "<a href='javascript:void(0)'>" + arr[i] + "</a>";
        if (i == 8) {
            str += "<a href='javascript:void(0)' class='fn-clear'>清除</a>";
        } else if (i == 9) {
            str += "<a href='javascript:void(0)'></a>";
        }
    }
    return str;
};

Pin.prototype.doSave = function () {
    var _this = this;
    _this.dom.btnSubmit = $("#btn-submit");
    G.post({
        url: "/Member/ChangePin",
        data: _this.dom.form.serialize(),
        success: function () {
            G.util.close();
            $.alert("修改成功");
        },
        error: function () {
            _this.dom.btnSubmit.prop("disabled", false);
        }
    });
};

G.modules.changePin = Pin;