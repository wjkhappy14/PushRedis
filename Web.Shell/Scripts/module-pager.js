function Pager(d) {
    this.d = $(d);
    this.dom = {};
}

Pager.prototype.init = function () {
    var _this = this;
    this.compile = this.d.attr("compile");
    this.dom.pagebody = $("#" + (this.d.attr("pagebody") || "pagebody"));
    this.dom.pageindex = $(".pageindex", this.d);
    this.dom.recordcount = $(".recordcount", this.d);
    this.dom.pagecount = $(".pagecount", this.d);
    this.dom.input_index = $(".fn-index", this.d).on("blur", function () {
        var value = parseInt(this.value, 10);
        if (isNaN(value) || value === 0) {
            this.value = "";
        }else if(value > _this.pageCount){
            this.value = _this.pageCount;
        }
    }).on("keydown", function (e) {
        if (e.keyCode === 13) {
            this.blur();
            _this.dom.go.trigger("click");
        }
    });
    this.dom.go = $(".fn-go", this.d).on("click", function () {
        if (_this.dom.input_index.val() === "") { return;}
        _this.original = _this.pageIndex;
        _this.pageIndex = parseInt(_this.dom.input_index.val(), 10);
        _this.getPage();
    });
    this.action = this.formatAction(this.d.attr("action"));
    this.template = this.d.attr("template")? $("#" + this.d.attr("template")).html() :$("#template_pager").html();
    this.pageCount = this.d.attr("pagecount");
    this.param = this.d.attr("param");
    this.pageIndex = 1;
    this.original = 1;
    this.dom.fnFirst = $(".fn-first", this.d).on("click", function () {
        if (_this.pageIndex == 1) { return; }
        _this.original = _this.pageIndex;
        _this.pageIndex = 1;
        _this.getPage();
    });
    this.dom.fnPrev = $(".fn-prev", this.d).on("click", function () {
        if (_this.pageIndex > 1) {
            _this.original = _this.pageIndex;
            _this.pageIndex -= 1;
            _this.getPage();
        }
    });
    this.dom.fnNext = $(".fn-next", this.d).on("click", function () {
        if (_this.pageIndex < _this.pageCount) {
            _this.original = _this.pageIndex;
            _this.pageIndex += 1;
            _this.getPage();
        }
    });
    this.dom.fnLast = $(".fn-last", this.d).on("click", function () {
        if (_this.pageIndex == _this.pageCount) { return; }
        _this.original = _this.pageIndex;
        _this.pageIndex = _this.pageCount;
        _this.getPage();
    });
    $(document).bind("pageinit." + this.d.attr("id"), function (e, param) {
        _this.pageInit(param);
    });
};

Pager.prototype.formatAction = function (str) {
    var arr = null;
    if(!/^\/[\w0-9]+\/[\w0-9]+$/.test(str)){
        arr = str.split(".");
        str = G.map[arr[0]].json[arr[1] ? arr[1] : arr[0]];
        arr = null;
    }
    return str;
};

Pager.prototype.pageInit = function (param) {
    this.pageIndex = 1;
    this.param = param;
    this.getPage();
};

Pager.prototype.bind = function (json) {
    this.dom.pageindex.html(json.Data.PageIndex);
    this.dom.recordcount.html(json.Data.RecordCount);
    this.dom.pagecount.html(json.Data.PageCount);
    this.pageCount = json.Data.PageCount;
    this.pageIndex = this.original = json.Data.PageIndex;
};

Pager.prototype.getPage = function () {
    var _this = this, param = (this.param? this.param + "&" : "") + "pageindex=" + this.pageIndex;
    loadPage({
        htmlurl: _this.template,
        jsonurl: _this.action,
        param: param,
        compile: _this.compile,
        success: function (html, json) {
            _this.dom.pagebody.html(html);
            _this.original = _this.pageIndex;
            _this.bind(json);
        },
        jsonError: function () { //分页不成功，还原pageindex
            _this.pageIndex = _this.original;
        }
    });
};

G.modules.pager = Pager;
G.modules.pager_loadip = Pager;
