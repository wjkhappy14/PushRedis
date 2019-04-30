(function ($) {
    var bd = $(document.body),
        ifr = $.support.boxSizing ? "" : "<iframe class='ifr-fix-ie6' frameborder='0'></iframe>",
        mask = $("<div class='mask hide'>" + ifr + "</div>").appendTo(bd);

    function drag(obj, agent) {
        obj.on("mousedown", agent, mousedown);

        function mousedown(e) {
            var offsetLeft = obj.offset().left;
            var offsetTop = obj.offset().top;
            var disX = e.clientX - offsetLeft;
            var disY = e.clientY - offsetTop;
            doc.on("mousemove", move).on("mouseup", up);

            function move(e) {
                var left = e.clientX - disX;
                var top = e.clientY - disY;
                if (obj[0].setCapture) {
                    obj[0].setCapture();
                }
                if (left < 0) {
                    left = 0;
                } else if (left > document.documentElement.clientWidth - obj.width() - 2) {
                    left = document.documentElement.clientWidth - obj.width() - 2;
                }
                if (top < 0) {
                    top = 0;
                } else if (top > document.documentElement.clientHeight - obj.height()) {
                    top = document.documentElement.clientHeight - obj.height();
                }
                obj.css({
                    left: left,
                    top: top
                });
                return false;
            }

            function up() {
                if (obj[0].releaseCapture) {
                    obj[0].releaseCapture();
                }
                doc.off("mousemove").off("mouseup");
            }
        }
    }

    function Dialog(param) {
        var _this = this, id = $.now(), s = null, str_button = "", str_closebtn, btn_class;
        param = $.isPlainObject(param) ? param : {};
        this.settings = {
            title: "弹出框",
            cls: "",
            htmlurl: "",
            button: [],
            bind: Loader.prototype.bind,
            unbind: Loader.prototype.unbind,
            closeBtn: true
        };
        s = $.extend({}, this.settings, param);
        str_closeBtn = s.closeBtn ? '<a href="javascript:void(0)" class="btn-close fn-close">×</a>' : '';
        $(s.button).each(function (i) {
            btn_class = i === 0 ? "btn" : "btn btn-gray";
            str_button += '<input type="button" value="' + this + '" class="' + btn_class + '" />';
        });
        this.body = $('<div id="dialog-' + id + '" class="g-dialog ' + s.cls + ' hide"><div class="dialog-hd"><div class="title fl">' + s.title + '</div><div class="fr">' + str_closeBtn + '</div></div><div class="dialog-bd">' + s.htmlurl + '</div><div class="dialog-ft">' + str_button + '</div></div>');
        this.body.on("click", ".fn-close", function () {
            s.closeCallback = null;
            _this.close();
        });
        this.body.on("click", ".dialog-ft input:last", function () {
            if ($(this).prev().length) {
                s.closeCallback = null;
            }
            _this.close();
        });
        doc.on("dialog.dialog-" + id, function (e, openid, type) {
            if (type == "close" && openid == id) {
                _this.close();
            }
        });
        if ($(".dialog-ft input", this.body).length == 2) {
            this.body.on("click", ".dialog-ft input:first", function () {
                _this.close();
            });
        }
        this.open = function () {
            mask.removeClass("hide");
            this.body.css("width", s.width || 600).appendTo(bd).removeClass("hide").css({
                width: s.width || 600,
                left: s.left || $(window).width() / 2 - _this.body.width() / 2,
                top: s.top || ($(window).height() - _this.body.height()) / 2
            });
            s.bind(this.body);
            G.dialog[id] = {
                id: id,
                instance: this
            };
            drag(this.body, ".dialog-hd");
            if ($.isFunction(s.openCallback)) {
                s.openCallback();
            }
            if (str_button) {
                setTimeout(function () {
                    $(".dialog-ft :button:last", this.body).focus();
                }, 0);
            }
        };
        this.close = function () {
            doc.off("dialog.dialog-" + id);
            s.unbind(this.body);
            this.body.off().remove();
            if ($.isFunction(s.closeCallback)) {
                s.closeCallback();
            }
            G.dialog[id].instance = null;
            delete G.dialog[id];
            if ($.isEmptyObject(G.dialog)) {
                mask.addClass("hide");
            }

        };
        this.open();
    }

    $.dialog = function (option) {
        var dialog;
        option = $.isPlainObject(option) ? option : {};
        G.util.load({
            name: option.module,
            param: option.param,
            htmlurl: option.htmlurl,
            jsonurl: option.jsonurl,
            jsonSuccess: option.jsonSuccess,
            joindata: option.joindata,
            success: function (html, id) {
                option.htmlurl = html;
                dialog = new Dialog(option);
            }
        });
    };
    $.alert = function (text, callback) {
        $.dialog({
            title: "提示",
            htmlurl: "<div>" + text + "</div>",
            closeCallback: callback,
            button: ["确定"],
            width: 350,
            cls: "g-alert",
            top: 200,
            closeBtn: false
        });
    };
    $.confirm = function (text, callback) {
        $.dialog({
            title: "确认",
            htmlurl: "<div>" + text + "</div>",
            closeCallback: callback,
            button: ["确定", "取消"],
            width: 350,
            cls: "g-confirm",
            top: 200,
            closeBtn: false
        });
    };

})(jQuery);
