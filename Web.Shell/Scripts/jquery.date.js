(function ($) {
    var offset, t = this,
        out, tdiv, selectB, setTime, still, re; //弹出框的坐标
    var time, years, mouths, date, inputyear, inputmouth, inputday, dateYears, ifr = $.support.boxSizing ? "" : "<iframe class='ifr-fix-ie6' frameborder='0'></iframe>";
    //日期框的HTML=======================================================
    var dayCode = $('<dl class="boxDay" style="display:none">' + ifr + '<dt class="dt"><a class="l" href="#">◄</a><a class="r" href="#">►</a><b><span name="reyear"></span>年<span name="remouth"></span>月</b></dt><dd class="hd"><span>日</span><span>一</span><span>二</span><span>三</span><span>四</span><span>五</span><span>六</span></dd><dd name="content" class="bd"></dd></dl>');
    $('body').append(dayCode);
    var content = $("dd.bd", dayCode),
        reyear = $(".dt span[name='reyear']", dayCode),
        remouth = $(".dt span[name='remouth']", dayCode);
    //日期框的表现函数：
    //求出一月中的1号是星期几，及一个月中最有多少天：setOption(2008,2)
    function setOption(y, m) {
        this.year = y;
        this.mouth = m;
        this.maxDay = function () {
            return (new Date(this.year, this.mouth, 0)).getDate();
        };
        this.minDay = function () {
            return (new Date(new Date(this.year, this.mouth, 0).setDate(1))).getDay();
        };
    }
    function days(year, mouth) {
        content.html('');
        reyear.html(year);
        remouth.html(mouth);
        var day = new Date(),
            dayNum = '', //日期显示
            q, w = 0, css;

        var set = new setOption(year, mouth);
        //循坏出日期显示内容，当前日期用CLASS：.now表示：
        for (q = 0; q <= (set.maxDay() + set.minDay() - 1); q++) {
            if (q >= set.minDay()) {
                w = w + 1; //日期数值
                //判断选中的日期:加CLASS='on'；
                if (w == inputday && year == inputyear && mouth == inputmouth) {
                    css = 'on';
                } else {
                    css = '';
                }
                if (w == day.getUTCDate() && year == day.getUTCFullYear() && mouth == (day.getUTCMonth() + 1)) {
                    dayNum += '<a name="' + w + '" href="#" class="now ' + css + '">' + w + '</a>';
                } else {
                    dayNum += '<a name="' + w + '" href="#" class="' + css + '">' + w + '</a>';
                }
            } else {
                dayNum += '<a href="javascript:void(0)" class="def"></a>';
            }
        }
        content.html(dayNum);
        mouth = mouth.length == 1 ? "0" + mouth : mouth;
        dateYears = year + '-' + mouth + '-';
        dayNum = css = q = w = null;
    }
    dayCode.on("click", ".bd a", function () {
        var _this = $(this), day = _this.attr("name");
        day = day.length == 1 ? '0' + day : day;
        $(this).addClass("on").siblings().removeClass("on");
        $(dayCode.target).val(dateYears + day);
        dayCode.on('hidden', hidden);
        clearTimeout(setTime);
        setTime = setTimeout(function () {
            clearTimeout(setTime);
            setTime = null;
            dayCode.triggerHandler("hidden");
        }, 0);
        $(document).triggerHandler("datechange");
        return false;
    });
    dayCode.on("click", ".dt .l", function () {
        clearTimeout(setTime);
        setTime = null;
        years = reyear.html();
        mouths = Number(remouth.html()) - 1;
        if (mouths === 0) {
            mouths = 12;
            years = Number(years) - 1;
        }
        if (mouths.toString().length == 1) {
            mouths = '0' + mouths;
        }
        days(years, mouths);
        return false;
    });
    dayCode.on("click", ".dt .r", function () {
        clearTimeout(setTime);
        setTime = null;
        years = reyear.html();
        mouths = Number(remouth.html()) + 1;
        if (mouths === 13) {
            mouths = 1;
            years = Number(years) + 1;
        }
        if (mouths.toString().length == 1) {
            mouths = '0' + mouths;
        }
        days(years, mouths);
        return false;
    });
    dayCode.mouseenter(function () {
        still = 1;
        clearTimeout(setTime);
        setTime = null;
    }).mouseleave(function () {
        still = 0;
        clearTimeout(setTime);
        setTime = setTimeout(function () {
            clearTimeout(setTime);
            setTime = null;
            dayCode.triggerHandler("hidden");
        }, 1000);
    });
    //弹出框显示
    function today() {
        var date = new Date(),
            year = date.getFullYear(),
            month = date.getMonth() + 1,
            day = date.getDate();
        return year + "-" + month + "-" + day;
    }

    function show(e, obj) {
        if (!/^\d{4}-\d{2}-\d{2}$/.test(obj.value)) {
            time = today().split(/\D+/);
        } else {
            time = obj.value.split(/\D+/);
        }
        inputyear = time[0];
        inputmouth = time[1];
        inputday = time[2];
        days(inputyear, inputmouth);
        offset = $(obj).offset();
        dayCode.css({ left: offset.left, top: offset.top + 20 }).fadeIn('fast');
    }
    dayCode.on('show', show);
    //弹出框关闭
    function hidden() {
        dayCode.fadeOut('fast');
        dayCode.target = null;
        delete dayCode.target;
        re = 0;
    }
    $.fn.datepicker = function () {
        return this.each(function () {
            $(this).focus(function () {
                dayCode.off('hidden', hidden);
                dayCode.target = this;
                dayCode.triggerHandler("show", [this]);
            }).blur(function () {
                dayCode.on('hidden', hidden);
                clearTimeout(setTime);
                if (still) {
                    return;
                }
                setTime = setTimeout(function () {
                    clearTimeout(setTime);
                    setTime = null;
                    dayCode.triggerHandler("hidden");
                }, 0);
            });
        });
    };
})(jQuery);
