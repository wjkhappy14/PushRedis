<%@ Page Title="ASP.NET SignalR" Language="C#" MasterPageFile="~/SignalR.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="SignalR.Tick._Default" %>

<asp:Content runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="MainContent">
    <div class="hero-unit">
        <h1>SignalR</h1>
        <p>Real-time web library for ASP.NET with a simple API, broad reaching client support and incredible performance.</p>
        <p><a class="btn btn-success btn-large" href="http://www.asp.net/signalr">Learn more &raquo;</a></p>
    </div>
    <div class="page-header">
        <h2>Demos</h2>
    </div>
    <!-- Samples -->
    <div class="row">
        <div class="span4">
            <h3>Connection API</h3>
            <p>
                Demonstrates all features of the lower-level connection API including starting and stopping, sending and
               receiving messages, and managing groups.
            </p>
            <p><a class="btn" href="/Hubs/Raw/">查看  &raquo;</a></p>
        </div>
        <div class="span4">
            <h3>Simple Streaming</h3>
            <p>A simple example of a background thread that broadcasts the server time to all connected clients every two seconds.</p>
            <p><a class="btn" href="Hubs/Streaming/">查看  &raquo;</a></p>
        </div>
        <div class="span4">
            <h3>Connection Status</h3>
            <p>Demonstrates how to handle the events that are raised when connections connect, reconnect and disconnect from the Hub API.</p>
            <p><a class="btn" href="Hubs/ConnectDisconnect/">查看  &raquo;</a></p>
        </div>
    </div>

    <div class="row">
        <div class="span4">
            <h3>Demo Hub</h3>
            <p>A contrived example that exploits every feature of the Hub API.</p>
            <p><a class="btn" href="Hubs/DemoHub/">查看  &raquo;</a></p>
        </div>
        <div class="span4">
            <h3>Hub Authorization</h3>
            <p>Demonstrates how to use the authorization features of the Hub API to restrict certain Hubs and methods to specific users.</p>
            <p><a class="btn" href="Hubs/Auth/">查看  &raquo;</a></p>
        </div>
        <div class="span4">
            <h3>Chat</h3>
            <p>The canonical real-time web example: a chat application. Features user management, multiple rooms and rich content integration.</p>
            <p><a class="btn" href="Hubs/Chat/">查看  &raquo;</a></p>
        </div>
    </div>

    <div class="row">
        <div class="span4">
            <h3>Mouse Tracking</h3>
            <p>An example that displays a live cursor on all users' screens for all other users' mouse movements.</p>
            <p><a class="btn" href="Hubs/MouseTracking/">查看  &raquo;</a></p>
        </div>
        <div class="span4">
            <h3>Drawing Pad</h3>
            <p>An example of a collaborative drawing pad using the HTML5 canvas API.</p>
            <p><a class="btn" href="Hubs/DrawingPad/">查看  &raquo;</a></p>
        </div>
        <div class="span4">
            <h3>Shape Share</h3>
            <p>Demonstrates a shared canvas onto which users can place various shapes and move them around in real-time.</p>
            <p><a class="btn" href="Hubs/ShapeShare/">查看  &raquo;</a></p>
        </div>
    </div>

    <div class="row">
        <div class="span4">
            <h3>Hub Ping</h3>
            <p>A sample that's used for measuring the full roundtrip time for invocations of Hub methods from client to server and back again.</p>
            <p><a class="btn" href="Hubs/Benchmark/">查看 &raquo;</a></p>
        </div>
        <div class="span4">
            <h3>Real-time Broadcast</h3>
            <p>A sample that uses a high-frequency timer to deliver updates at a high rate (e.g. 25 Hz) as might be typical in a real-time, multi-user HTML5 game.</p>
            <p><a class="btn" href="Hubs/RealtimeBroadcast/">查看 &raquo;</a></p>
        </div>
        <div class="span4">
            <h3>Redis Hub Connection API</h3>
            <p>Redis Hub Connection API including starting and stopping, sending and receiving messages, and managing groups.</p>
            <p><a class="btn" href="Hubs/HubConnectionAPI/">查看 &raquo;</a></p>
        </div>
    </div>

    <div class="row">
        <div class="span4">
            <h3>Message Loops</h3>
            <p>A sample that demonstrates message loops where client calls the server method to broadcast message after the client successfully calls the server method once start, and shows missing and dup messages if happens.</p>
            <p><a class="btn" href="Hubs/MessageLoops/">View sample &raquo;</a></p>
        </div>
        <div class="span4">
            <h3>Simultaneous Connections</h3>
            <p>A sample that demonstrates simultaneous connections. You can add new connections via the button or default the number of active connections via the queryString parameters hubConnections and persistentConnections.</p>
            <p><a class="btn" href="Hubs/SimultaneousConnections/">View sample &raquo;</a></p>
        </div>
    </div>
    <div class="clear"></div>
</asp:Content>
