using System.Web.Mvc;

namespace SignalR.Tick.Controllers
{
    public class HubsController : Controller
    {
        public HubsController()
        {
        }
        public ViewResult Index()
        {
            return View();
        }
        public ViewResult StockTicker()
        {
            return View();
        }
    }
}