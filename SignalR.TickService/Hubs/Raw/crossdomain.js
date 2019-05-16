$(function () {
    "use strict";
    var connection = $.connection('https://www.angkorw.cn/raw-connection');
    connection.logging = true;

    connection.received(function (data) {
        $("<li/>").html(window.JSON.stringify(data)).appendTo($("#messages"));
        if (data.type == 2) {
            $.cookie('user', data.data);
        }
    });

    connection.reconnected(function () {
        $("<li/>").css("background-color", "green")
            .css("color", "white")
            .html("[" + new Date().toTimeString() + "]: Connection re-established")
            .appendTo($("#messages"));
    });

    connection.error(function (err) {
        $("<li/>").html(err || "Error occurred")
            .appendTo($("#messages"));
    });

    connection.stateChanged(function (change) {
        var oldState = null,
            newState = null;
        for (var p in $.signalR.connectionState) {
            if ($.signalR.connectionState[p] === change.oldState) {
                oldState = p;
            }

            if ($.signalR.connectionState[p] === change.newState) {
                newState = p;
            }
        }

        $("<li/>").html(oldState + " => " + newState)
            .appendTo($("#messages"));
    });

    var start = function () {
        connection.start({ transport: activeTransport, jsonp: isJsonp })
            .then(function () {
                $("#stopStart").prop("disabled", false);
            });
    };
    start();

    $("#send").click(function () {
        connection.send(window.JSON.stringify({ type: 0, value: $("#me").val() }));
    });

    $("#broadcast").click(function () {
        connection.send(window.JSON.stringify({ type: 1, value: $("#msg").val() }));
    });

    $("#broadcast-exceptme").click(function () {
        connection.send(window.JSON.stringify({ type: 7, value: $("#msg").val() }));
    });

    $("#join").click(function () {
        connection.send(window.JSON.stringify({ type: 2, value: $("#msg").val() }));
    });

    $("#privatemsg").click(function () {
        connection.send(window.JSON.stringify({ type: 3, value: $("#user").val() + "|" + $("#message").val() }));
    });

    $('#join-group').click(function () {
        connection.send(window.JSON.stringify({ type: 4, value: $("#msg").val() }));
    });

    $('#leave-group').click(function () {
        connection.send(window.JSON.stringify({ type: 5, value: $("#msg").val() }));
    });

    $("#groupmsg").click(function () {
        connection.send(window.JSON.stringify({ type: 6, value: $("#user").val() + "|" + $("#message").val() }));
    });

    $("#stopStart").click(function () {
        var $el = $(this);
        $el.prop("disabled", true);
        if ($el.val() === "Stop") {
            connection.stop();
            $el.val("Start")
                .prop("disabled", false);
        } else {
            start();
            $el.val("Stop");
        }
    });
});
