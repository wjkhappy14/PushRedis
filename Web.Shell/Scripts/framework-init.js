if (!window.console) {
    window.console = {
        log: function () { }
    };
}
if (!$.support.boxSizing) {
    document.execCommand("BackgroundImageCache", false, true);
}
(function (window, $) {
    var win = $(window),
        doc = $(document),
        ie9 = !$.support.clearCloneStyle;

    $.ajaxSetup({
        global: false,
        cache: false
    });

    function Loader(option, loadid) {
        option = option || {};
        this.template = null;
        this.jsondata = { Status: 1, Data: {} };
        this.container = option.container || null;
        this.timer_html = null;
        this.timer_bind = null;
        this.success = option.success || null;
        this.renderSuccess = option.renderSuccess || null;
        this.compile = option.compile || null;
        this.loadid = loadid;
        this.flag = 0;
        this.loadPage(option, loadid);
    }

    Loader.prototype.loadPage = function (option, loadid) {
        option = option || {};
        this.getHTML(option);
        this.getJSON(option, loadid);
    };

    Loader.prototype.render = function (strHTML) {
        if (ie9) {
            strHTML = strHTML.replace(/\/td>\s+<td/g, "/td><td");
        }
        var _this = this;
        if (this.success) {
            this.success(strHTML, this.jsondata);
        } else {
            this.unbind(this.container);
            this.container.html(strHTML);
            if (this.renderSuccess) {
                this.renderSuccess(this.jsondata);
            }
        }
        this.timer_bind = setTimeout(function () {
            _this.bind(_this.container);
            _this.destroy();
        }, 0);
    };

    Loader.prototype.destroy = function () {
        if (this.timer_bind) {
            clearTimeout(this.timer_bind);
        }
        this.template = null;
        this.jsondata = null;
        this.container = null;
        this.timer_html = null;
        this.timer_bind = null;
        this.flag = null;
        G[this.loadid] = null;
        delete G[this.loadid];
        this.success = null;
        this.renderSuccess = null;
        this.compile = null;
    };

    Loader.prototype.getHTML = function (option) {
        var _this = this,
            htmlname, htmlurl;
        if (/<\w+.*?>/.test(option.htmlurl)) {
            this.template = option.htmlurl;
            this.flag += 1;
            this.flashHTML(option);
            return;
        }

        htmlname = option.htmlurl ? option.htmlurl : option.name;
        htmlurl = /^\/.*/.test(option.htmlurl) ? option.htmlurl : option.htmlurl ? G.map[option.name].html[option.htmlurl] : G.map[option.name].html[option.name];

        if (!G.cache.html[htmlname]) {
            G.util.getHTML({
                url: htmlurl,
                success: function (html) {
                    _this.template = G.cache.html[htmlname] = html;
                    _this.flag += 1;
                    _this.flashHTML(option, _this.jsondata);
                },
                error: function () {
                    $.alert("加载页面出错，请重试!");
                }
            });
        } else {
            this.template = G.cache.html[htmlname];
            this.flag += 1;
            this.flashHTML(option);
        }
    };

    Loader.prototype.getJSON = function (option, loadid) {
        var _this = this;
        jsonurl = /^\/.+/.test(option.jsonurl) ? option.jsonurl : option.jsonurl ? G.map[option.name].json[option.jsonurl] : option.name && !option.htmlurl ? G.map[option.name].json[option.name] : null;
        if (!jsonurl) {
            this.flag += 1;
            this.flashHTML(option);
            return;
        }
        G.get({
            url: jsonurl,
            data: option.param,
            success: function (json) {
                _this.flag += 1;
                _this.jsondata = json;
                if (option.jsonSuccess) {
                    _this.jsondata = option.jsonSuccess(json) || json;
                }
                _this.flashHTML(option);
            },
            error: function () {
                if (option.jsonError) {
                    option.jsonError();
                } else {
                    $.alert("加载页面出错，请重试!");
                }
            }
        });
    };

    Loader.prototype.flashHTML = function (option) {
        if (this.flag < 2) {
            return;
        }
        var strHTML = "",
            strTemplate = "";
        this.template = this.template.replace(new RegExp('<script multiple[^>]*?>([\\s\\S]*?)<\\/script>', 'ig'), function (a, b) {
            return b + a;
        });
        this.template = this.template.replace(new RegExp('<script[^>]*?>[\\s\\S]*?<\\/script>', 'ig'), function (a) {
            strTemplate += a;
            return "";
        });
        if (option.joindata) {
            this.jsondata.JoinData = option.joindata;
        }
        strHTML = this.doCompile(this.template, this.jsondata, option.name, option.htmlurl);
        strHTML += strTemplate;
        this.render(strHTML);
        strHTML = null;
        strTemplate = null;
    };

    Loader.prototype.doCompile = function (template, json, moduleid, htmlurl) {
        var html = "",
            arr,
            htmlname = htmlurl ? htmlurl : moduleid;
        if (this.compile) {
            if ($.isFunction(this.compile)) {
                this.compile.apply(this, [template, json, moduleid, htmlurl]);
            } else if (typeof this.compile == "string") {
                arr = this.compile.split(".");
                html = G.map[arr[0]].compile[arr[1] ? arr[1] : arr[0]].apply(this, [template, json, moduleid, htmlurl]);
                arr = null;
            }

        } else {
            if (!moduleid) {
                html = G.util.compile.apply(this, [template, json]);
            } else {
                if (G.map[moduleid].compile && G.map[moduleid].compile[htmlname]) {
                    html = G.map[moduleid].compile[htmlname].apply(this, [template, json, moduleid, htmlurl]);
                } else {
                    html = G.util.compile.apply(this, [template, json]);
                }
            }
        }
        return html;
    };

    Loader.prototype.bind = function (container) {
        if (!container || !container.length) {
            return;
        }
        $("div[name=module]", container).each(function () {
            var id = this.id;
            if (G.modules[id] && !G.instance[id]) {
                G.instance[id] = new G.modules[id](this);
                if (G.instance[id].init) {
                    G.instance[id].init();
                }
            }
        });
    };

    Loader.prototype.unbind = function (container) {
        var _this = this;
        if (!container || !container.length) {
            return;
        }
        $("div[name=module]", container).each(function () {
            var id = this.id;
            if (G.instance[id]) {
                if (G.instance[id].destroy) {
                    G.instance[id].destroy();
                }
                G.util.destroy(G.instance[id]);
                G.instance[id] = null;
                delete G.instance[id];
            }
        });
    };

    var G = {
        cache: {
            html: {}
        },
        instance: {},
        modules: {},
        dialog: {},
        now: 0
    };
    G.util = {
        compile: function (html, json) {
            var render = template.compile(html);
            var strHTML = render(json);
            return strHTML;
        },
        gethash: function (hash) {
            var obj = {};
            hash = (unescape(hash.replace(/^#/, ""))).split("|");
            $(hash).each(function (i, item) {
                item = item.split("=");
                obj[item[0]] = item[0] == "param" || item[0] == "joindata" ? $.parseJSON(item[1]) : item[1];
            });
            return obj;
        },
        load: function (option) {
            var id = $.now();
            G[id] = new Loader(option, id);
        },
        close: function (id) {
            var p;
            if (id) {
                doc.triggerHandler("dialog", [id, "close"]);
            }
            else {
                for (p in G.dialog) {
                    doc.triggerHandler("dialog", [p, "close"]);
                }
            }
        },
        getAll: function () {
            if (location.hostname == "localhost") { return; }
            this.getHTML({
                url: "/All/All.html",
                cache: false,
                success: function (html) {
                    if (!html) { return; }
                    var arr = html.split("êêê"),
                        match, reg = /name=\"module\"\s+id=\"([a-zA-Z0-9_-]+)\"/,
                        id;
                    for (var i = 0; i < arr.length; i++) {
                        match = arr[i].match(reg);
                        id = match && match[0] ? match[1] : null;
                        if (id && !G.cache.html[id]) {
                            G.cache.html[id] = arr[i];
                        }
                    }
                }
            });
        },
        destroy: function (instance) {
            var id, element, d = instance.d,
                dom = instance.dom;
            if (d) {
                id = instance.d.attr("id");
                instance.d.off();
                doc.off("." + id);
            }
            if (dom) {
                for (element in dom) {
                    dom[element].off();
                    dom[element] = null;
                    delete dom[element];
                }
            }
        },
        getHTML: function (option) {
            $.ajax({
                url: option.url + "?v=" + VERSION,
                dataType: "html",
                cache: option.cache || true,
                success: function (html) {
                    if (option.success) {
                        option.success(html);
                    }
                },
                error: function () {
                    if (option.error) {
                        option.error();
                    }
                }
            });
        },
        hover: function ($dom, agent) {
            if ($.support.boxSizing) { return; }
            $dom.on("mouseenter", agent, function () {
                $(this).addClass("hover");
            }).on("mouseleave", agent, function () {
                $(this).removeClass("hover");
            });
        },
        getDate: function (time) {
            var date, year, month, day;
            if (!time || !$.isNumeric(time)) {
                time = $.now();
            }
            date = new Date(time);
            year = date.getFullYear();
            month = date.getMonth() + 1;
            month = month < 10 ? "0" + month : month;
            day = date.getDate();
            day = day < 10 ? "0" + day : day;
            return year + "-" + month + "-" + day;
        },
        guid: function () {
            var s = [];
            var hexDigits = "0123456789abcdef";
            for (var i = 0; i < 36; i++) {
                s[i] = hexDigits.substr(Math.floor(Math.random() * 0x10), 1);
            }
            s[14] = "4";  // bits 12-15 of the time_hi_and_version field to 0010
            s[19] = hexDigits.substr((s[19] & 0x3) | 0x8, 1);  // bits 6-7 of the clock_seq_hi_and_reserved to 01
            s[8] = s[13] = s[18] = s[23] = "-";

            var uuid = s.join("");
            return uuid;
        },
        tip: function (str) {
            str = str || "操作成功";
            if (G.tipTimer) {
                clearTimeout(G.tipTimer);
            }
            if (!G.cache.tip) {
                G.cache.tip = $("<div class='g-tip hide'></div>").appendTo(G.instance.header.d);
            }
            G.cache.tip.html(str).stop().fadeIn();
            G.tipTimer = setTimeout(function () {
                G.cache.tip.fadeOut();
            }, 3000);
        },
        formatSeconds: function (a) {
            var m = parseInt(a / 60), s = a % 60;
            m = m < 10 ? "0" + m : m;
            s = s < 10 ? "0" + s : s;
            return m + ":" + s;
        },
        format_thousands: function (str) {
            str = (str + "").split(".");
            var str0 = str[0];
            while (/\d{4}/.test(str0)) {
                str0 = str0.replace(/(\d+)(\d{3})/, "$1,$2");
            }
            return str0 + (str[1] ? "." + str[1] : "");
        },
        format_number: function (str) {
            str = str + "";
            if (/\.[0-9][1-9]\d+$/.test(str)) {
                str = parseFloat(str, 10).toFixed(0);
            } else {
                str = parseFloat(str).toFixed(0);
            }
            str = G.util.format_thousands(str);
            return str;
        },
        input_format: function (element, option) {
            var target = option.target;
            element.on("keyup", function (e) {
                var keyCode = e.keyCode;
                if (keyCode >= 48 && keyCode <= 57 || keyCode >= 96 && keyCode <= 105 || keyCode == 189 || keyCode == 109 || keyCode == 8 || keyCode == 46) {
                    this.value = G.util.format_thousands(this.value.replace(/\,/g, ""));
                }
                target.val(this.value.replace(/\,/g, ""));
            });
        },
        setCookie: function (name, value) {
            var Days = 30;
            var exp = new Date();
            exp.setTime(exp.getTime() + Days * 24 * 60 * 60 * 1000);
            document.cookie = name + "=" + escape(value) + ";expires=" + exp.toGMTString();
        },
        getCookie: function (name) {
            var arr, reg = new RegExp("(^| )" + name + "=([^;]*)(;|$)");
            if (arr = document.cookie.match(reg)) {
                return unescape(arr[2]);
            } else {
                return null;
            }
        },
        delCookie: function (name) {
            var exp = new Date();
            exp.setTime(exp.getTime() - 1);
            var cval = G.util.getCookie(name);
            if (cval != null) {
                document.cookie = name + "=" + cval + ";expires=" + exp.toGMTString();
            }
        }
    };

    $(["get", "post"]).each(function (i, ajaxType) {
        G[ajaxType] = function (option) {
            $.ajax({
                url: option.url,
                data: option.data,
                type: ajaxType,
                dataType: "json",
                cache: option.cache,
                async: option.async,
                success: function (d) {
                    switch (d.Status) {
                        case 2:
                        case 4:
                        case 6:
                        case 7:
                            if (option.bussiness) {
                                option.bussiness(d.Data)
                            } else {
                                $.alert(d.Data);
                            }
                            break;
                        case 3:
                            $.alert(d.Data);
                            if (option.bussiness) {
                                option.bussiness(d.Data);
                            }
                            break;
                        case 5:
                            $.alert(d.Data, function () {
                                location.href = "/Member/Login";
                            });
                            if (option.bussiness) {
                                option.bussiness(d.Data);
                            }
                            break;
                        case 500:
                            $.alert("请求数据失败，错误码：500，具体原因如下：\r\n\r\n" + d.Data);
                            break;
                        default:
                            if (option.success) {
                                option.success(d);
                            }
                    }
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    if (option.error) {
                        option.error(XMLHttpRequest, textStatus, errorThrown);
                    }
                },
                complete: function (XMLHttpRequest, textStatus) {
                    if (option.complete) {
                        option.complete(XMLHttpRequest, textStatus);
                    }
                }
            });
        };
    });
    $.extend(window, {
        Loader: Loader,
        G: G,
        main: $("#main"),
        loadPage: G.util.load,
        doc: $(document)
    });
})(window, jQuery);

$(function () {
    $.fn.hashchange.src = '/All/Domain.html';
    $.fn.hashchange.domain = document.domain;
    $(window).hashchange(function () {
        var hash_module = G.util.gethash(location.hash),
            module = hash_module.module;
        if (G.hashFn[module]) {
            G.hashFn[module](hash_module);
        }
        G.util.close();
    });
    if (main.length) {
        if (!/^#module=[a-zA-Z0-9_]+(\|[a-zA-Z0-9_]+=(([a-zA-Z0-9_]+)|{\"[a-zA-Z0-9_]+\":\"?[a-zA-Z0-9\-:\s]*\"?(,\"[a-zA-Z0-9_]+\":\"?[a-zA-Z0-9\-:\s]*\"?)*}))*$/.test(unescape(location.hash))) {
            location.hash = G.InitHash;
        }
        $(window).hashchange();
    }
    G.util.getAll();
    Loader.prototype.bind($(document.body));
});

String.prototype.toFloat = function () {
    return parseFloat(this);
};

String.prototype.toInt = function () {
    return parseInt(this, 10);
};

Number.prototype.toFixed = function (c) {
    var s = this + "", i = 0, newstr = "";
    var arr = s.split(".");
    arr[1] = arr[1] ? arr[1] : "";
    if (c <= 0) {
        return arr[0];
    } else {
        for (i = 0; i < c; i++) {
            if (arr[1][i]) {
                newstr += arr[1][i];
            }
            else {
                newstr += "0";
            }
        }
        return arr[0] + "." + newstr;
    }
};
