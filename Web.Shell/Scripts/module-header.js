function Header(d){
	this.d = $(d);
	this.dom = {};
	this.timer = null;
}

Header.prototype.init = function () {
    var _this = this;
	this.dom.loginout = $("#logout").on("click", function () {
	    $.confirm("确定要退出吗？", function () {
	        G.get({url: "/Member/Logout"});
	        location.href = "/Member/Login";
	    });
	    return false;
	});
	this.timer = setInterval(function () {
	    G.get({
	        url: "/Member/Online",
	        bussiness: function () {
	            clearInterval(_this.timer);
	        }
	    });
	    G.instance.footer.getMarquee();
	}, 25000);
};

G.modules.header = Header;

function Footer(d) {
    this.d = $(d);
    this.dom = {};
}

Footer.prototype.init = function () {
    var _this = this;
    this.dom.marquee = $("#mq-notice");
    if(this.dom.marquee[0].stop){
        this.dom.marquee.on("mouseenter", function () {
            if (this.stop) { this.stop(); }
        }).on("mouseleave", function () {
            if (this.start) { this.start(); }
        });
    }
    this.getMarquee();
};

Footer.prototype.getMarquee = function () {
    var _this = this;
    G.get({
        url: "/Marquee/GetModel",
        success: function (x) {
            _this.dom.marquee.html(x.Data.Content);
        }
    });
};

G.modules.footer = Footer;