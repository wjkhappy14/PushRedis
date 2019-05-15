if (!String.prototype.supplant) {
    String.prototype.supplant = function (o) {
        return this.replace(/{([^{}]*)}/g,
            function (a, b) {
                var r = o[b];
                return typeof r === 'string' || typeof r === 'number' ? r : a;
            }
        );
    };
}

// A simple background color flash effect that uses jQuery Color plugin
jQuery.fn.flash = function (color, duration) {
    var current = this.css('backgroundColor');
    this.animate({ backgroundColor: 'rgb(' + color + ')' }, duration / 2)
        .animate({ backgroundColor: current }, duration / 2);
};



$(function () {
    var ticker = $.connection.stockTicker;
    var up = '↑',
        down = '↓',
        $stockTable = $('#stockTable'),
        $stockTableBody = $stockTable.find('tbody'),
        rowTemplate = '<tr data-symbol="{CommodityNo}"><td>{CommodityNo}</td> <td>{ContractNo}</td><td>{CurrentTime}</td><td>{Time}</td><td>{TimeDiff}</td><td>{HighPrice}</td><td>{LastPrice}</td> <td>{LastSize}</td><td>{LowPrice}</td> <td>{NowClosePrice}</td><td>{ClosePrice}</td> <td>{OpenPrice}</td><td>{PercentChange}</td><td>{PositionQty}</td> <td>{PrePositionQty}</td> <td>{PreSettlePrice}</td> <td>{TotalQty}</td><td>{TotalVolume}</td><td>{Volume}</td><td>{AskPrice}</td><td>{AskSize}</td><td>{BidPrice}</td><td>{BidSize}</td></tr>',
        $stockTicker = $('#stockTicker'),
        $stockTickerUl = $stockTicker.find('ul'),
        liTemplate = '<li data-symbol="{Item2}"><span>{Item2}</span></li>';

    function formatStock(stock) {
        return $.extend(stock, {
            Price: stock.BidPrice,
            CurrentTime: +(new Date()),
            TimeDiff: timestamp() - stock.Time,
            PercentChange: (stock.BidPrice * 100).toFixed(2) + '%',
            Direction: stock.BidPrice === 0 ? '' : stock.BidPrice >= 0 ? up : down,
            DirectionClass: stock.BidPrice === 0 ? 'even' : stock.BidPrice >= 0 ? 'up' : 'down'
        });
    };
    function formatSymbol(symbol) {
        return $.extend(symbol, {
            symbol: symbol,
            CurrentTime: +(new Date())
        });
    };

    function scrollTicker() {
        var w = $stockTickerUl.width();
        $stockTickerUl.css({ marginLeft: w });
        $stockTickerUl.animate({ marginLeft: -w }, 15000, 'linear', scrollTicker);
    }

    function stopTicker() {
        $stockTickerUl.stop();
    }

    function init() {
        return ticker.server.getAllStocks().done(function (stocks) {
            $stockTableBody.empty();
            $stockTickerUl.empty();
            $.each(stocks, function () {
                var stock = formatStock(this);
                $stockTableBody.append(rowTemplate.supplant(stock));
                $stockTickerUl.append(liTemplate.supplant(stock));
            });
        });
    };
    function initSymbols() {
        return ticker.server.getSymbols()
            .done(function (symbols) {
                $stockTickerUl.empty();
                $.each(symbols, function (index) {
                    var symbol = symbols[index];
                    var el = liTemplate.supplant(symbol);
                    $stockTickerUl.append(el);
                });
            });
    }

    // Add client-side hub methods that the server will call
    $.extend(ticker.client, {
        updateStockPrice: function (stock) {
            var displayStock = formatStock(stock),
                $row = $(rowTemplate.supplant(displayStock)),
                $li = $(liTemplate.supplant(displayStock)),
                bg = stock.LastChange < 0
                    ? '255,148,148' // red
                    : '154,240,117'; // green

            $stockTableBody.find('tr[data-symbol=' + stock.CommodityNo + ']')
                .replaceWith($row);
            $stockTickerUl.find('li[data-symbol=' + stock.CommodityNo + ']')
                .replaceWith($li);

            $row.flash(bg, 1000);
            $li.flash(bg, 1000);
        },

        marketOpened: function () {
            $("#open").prop("disabled", true);
            $("#close").prop("disabled", false);
            $("#reset").prop("disabled", true);
            scrollTicker();
        },

        marketClosed: function () {
            $("#open").prop("disabled", false);
            $("#close").prop("disabled", true);
            $("#reset").prop("disabled", false);
            stopTicker();
        },
        unSubscribe: function () {
            console.log(this);

        },
        subscribe: function () {
            console.log(this);
        },

        marketReset: function () {
            return init();
        }
    });

    // Start the connection
    $.connection.hub.logging = true;
    $.connection.hub.start({ transport: ['webSockets'] })
        .then(init)
        .then(initSymbols)
        .then(function () {
            return ticker.server.getMarketState();
        })
        .done(function (state) {
            if (state === 'Open') {
                ticker.client.marketOpened();
            } else {
                ticker.client.marketClosed();
            }
            // Wire up the buttons
            $("#open").click(function () {
                ticker.server.openMarket();
            });

            $("#close").click(function () {
                ticker.server.closeMarket();
            });

            $("#reset").click(function () {
                ticker.server.reset();
            });
        });
});