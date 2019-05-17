$(function () {
    "use strict";
    var $stockTable = $('#stockTable');
    var $stockTableBody = $stockTable.find('tbody');

    var connection = $.connection('/raw-connection');
    var rowTemplate = '<tr data-symbol="{CommodityNo}"><td>{CommodityNo}</td> <td>{ContractNo}</td><td>{CurrentTime}</td><td>{Time}</td><td>{TimeDiff}</td><td>{HighPrice}</td><td>{LastPrice}</td> <td>{LastSize}</td><td>{LowPrice}</td> <td>{NowClosePrice}</td><td>{ClosePrice}</td> <td>{OpenPrice}</td><td>{PercentChange}</td><td>{PositionQty}</td> <td>{PrePositionQty}</td> <td>{PreSettlePrice}</td> <td>{TotalQty}</td><td>{TotalVolume}</td><td>{Volume}</td><td>{AskPrice}</td><td>{AskSize}</td><td>{BidPrice}</td><td>{BidSize}</td></tr>';
    connection.logging = true;
    $.getJSON("https://api.db-ip.com/v2/free/self").then(addrInfo => {
        var t = "您的IP是:" + addrInfo.ipAddress + " 位于： " + addrInfo.city + ", " + addrInfo.stateProv + "," + addrInfo.countryName;
        $('#my-location').html('<i>' + t + '</i>');
    });


    connection.received(function (data) {
        console.dir(data);
        if (data.Code == 200) {
            if (data.CmdType == 0) {
                $.each(data.Result, function () {
                    var item = this;
                    var html = rowTemplate.supplant(item);
                    $stockTableBody.append(html);
                });
            }
            if (data.CmdType == 18) {
                var row = rowTemplate.supplant(data.Result);
                $stockTableBody.find('tr[data-symbol=' + data.Result.CommodityNo + ']').replaceWith(row);
            }
            if (data.CmdType == 5) {
                $('#time-now').html('<i>' + data.Result + '</i>');
            }
            if (data.CmdType == 12) {
                var url = "https://api.db-ip.com/v2/free/" + data.Result;
                $.getJSON(url).then(addrInfo => {
                    if (addrInfo.errorCode == "RESTRICTED") {
                        $('#clients').append('<tr><th>' + data.Result + addrInfo + '</th></tr>');
                    }
                    else {
                        var addr = addrInfo.ipAddress + " is in " + addrInfo.city + ", " + addrInfo.stateProv + "," + addrInfo.countryName;
                        $('#clients').append('<tr><th>' + addr + '</th></tr>');
                    }
                }
                );
            }
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

        $("<li/>").html(oldState + " => " + newState).appendTo($("#messages"));
    });

    var start = function () {
        connection.start({
            transport: activeTransport,
            jsonp: isJsonp
        }).then(function () {
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
        var data = {
            type: 2,
            value: $("#msg").val()
        };
        connection.send(window.JSON.stringify(data));
    });

    $("#privatemsg").click(function () {
        var data = {
            type: 3,
            value: $("#user").val() + "|" + $("#message").val()
        };
        connection.send(window.JSON.stringify(data));
    });

    $('#join-group').click(function () {
        var data = {
            type: 4,
            value: $("#msg").val()
        };
        connection.send(window.JSON.stringify(data));
    });

    $('#leave-group').click(function () {
        var data = {
            type: 5,
            value: $("#msg").val()
        };
        connection.send(window.JSON.stringify(data));
    });

    $("#groupmsg").click(function () {
        var data = {
            type: 6,
            value: $("#user").val() + "|" + $("#message").val()
        };
        connection.send(window.JSON.stringify(data));
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
