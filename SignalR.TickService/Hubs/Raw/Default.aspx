<%@ Page Title="订阅行情推送API" Language="C#" MasterPageFile="~/SignalR.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="SignalR.Tick.Raw.Default" %>

<asp:Content runat="server" ContentPlaceHolderID="MainContent">
    <ul class="breadcrumb">
        <li><a href="<%: ResolveUrl("~/") %>">SignalR Samples</a> <span class="divider">/</span></li>
        <li class="active">Connection API</li>
    </ul>

    <div class="page-header">
        <h2>订阅行情推送API <small>实时，低延迟</small></h2>
        <p>
            对接demo
        </p>
    </div>
    <a href="crossdomain.htm">跨域(Cross Domain)</a>
    <h4>To All</h4>
    <form class="form-inline">
        <div class="input-append">
            <input type="text" id="msg" placeholder="Type a message" value="Hello  hao are  you ?" />
            <select id="group">
                <option value="All" selected="selected">All</option>
                <option value="AD1906">AD1906</option>
                <option value="BP1906">BP1906</option>
                <option value="CD1906">CD1906</option>
                <option value="CL1906">CL1906</option>
                <option value="CN1905">CL1906</option>
                <option value="HG1907">CL1906</option>
                <option value="GC1906">CL1906</option>
                <option value="EC1906">CL1906</option>
                <option value="HSI1905">CL1906</option>
                <option value="MHI1905">CL1906</option>
                <option value="DAX1906">DAX1906</option>
                <option value="NQ1906">NQ1906</option>
                <option value="SI1907">SI1907</option>
            </select>
            <input type="button" id="broadcast" class="btn" value="广播" />
            <input type="button" id="broadcast-exceptme" class="btn" value="Broadcast (All Except Me)" />
            <input type="button" id="join" class="btn" value="Enter Name" />
            <input type="button" id="join-group" class="btn" value="订阅" />
            <input type="button" id="leave-group" class="btn" value="取消" />
        </div>
    </form>

    <h4>To Me</h4>
    <form class="form-inline">
        <div class="input-append">
            <input type="text" id="me" placeholder="Type a message" />
            <input type="button" id="sendtome" class="btn" value="Send to me" />
        </div>
    </form>

    <h4>私信 </h4>
    <form class="form-inline">
        <div class="input-prepend input-append">
            <input type="text" name="message" id="message" placeholder="Type a message" />
            <input type="text" name="user" id="user" placeholder="Type a user or group name" />

            <input type="button" id="privatemsg" class="btn" value="Send to user" />
            <input type="button" id="groupmsg" class="btn" value="Send to group" />
        </div>
    </form>

    <button id="stopStart" class="btn btn-info btn-small" disabled="disabled"><i class="icon-stop icon-white"></i><span>Stop Connection</span></button>

    <h4>Messages</h4>
    <ul id="messages">
    </ul>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="Scripts">
    <script src="<%: ResolveUrl("~/Scripts/jquery.cookie.js") %>"></script>
    <script type="text/javascript">
        window.onload = function () {
            $.cookie("user", +(new Date()));
        };
    </script>
    <script type="text/javascript">
        $(function () {
            "use strict";

            var connection = $.connection("/raw-connection");
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

            connection.disconnected(function () {
                $("#stopStart")
                    .prop("disabled", false)
                    .find("span")
                    .text("Start Connection")
                    .end()
                    .find("i")
                    .removeClass("icon-stop")
                    .addClass("icon-play");
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
                        $("#stopStart")
                            .prop("disabled", false)
                            .find("span")
                            .text("Stop Connection")
                            .end()
                            .find("i")
                            .removeClass("icon-play")
                            .addClass("icon-stop");
                    });
            };
            start();
            //send to me
            $("#sendtome").click(function () {
                connection.send({
                    type: 0,
                    value: $("#me").val()
                });
            });
            //Broadcast
            $("#broadcast").click(function () {
                connection.send({
                    type: 1,
                    value: $("#msg").val()
                });
            });

            //Join
            $("#join").click(function () {
                connection.send({
                    type: 2,
                    value: $("#msg").val()
                });
            });
            //PrivateMessage
            $("#privatemsg").click(function () {
                connection.send({
                    type: 3,
                    value: $("#user").val() + "|" + $("#message").val()
                });
            });
            //AddToGroup
            $('#join-group').click(function () {
                connection.send({
                    type: 4,
                    value: $("#group").val()
                });
            });
            //RemoveFromGroup
            $('#leave-group').click(function () {
                connection.send({
                    type: 5,
                    value: $("#msg").val()
                });
            });

            //SendToGroup
            $("#groupmsg").click(function () {
                connection.send({
                    type: 6,
                    value: $("#user").val() + "|" + $("#message").val()
                });
            });
            //BroadcastExceptMe
            $("#broadcast-exceptme").click(function () {
                connection.send({
                    type: 7,
                    value: $("#msg").val()
                });
            });
            $("#stopStart").click(function () {
                var $el = $(this);

                $el.prop("disabled", true);

                if ($.trim($el.find("span").text()) === "Stop Connection") {
                    connection.stop();
                } else {
                    start();
                }
            });
        });
    </script>
</asp:Content>
