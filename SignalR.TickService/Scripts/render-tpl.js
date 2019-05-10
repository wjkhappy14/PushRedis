(function (win) {
    win.Tpl = {
        'theader': '',
        'thbody': '',

    };
    win.timestamp = function () {
        var date = new Date();
        var milliseconds = date.valueOf();
        return milliseconds;
    };
})(window);