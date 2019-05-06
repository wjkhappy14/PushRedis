using System.Web.Mvc;

namespace SignalR.Tick.Controllers
{
    public class AccountController : Controller
    {
        public AccountController()
        {

        }

        [HttpGet]
        public JsonResult Create(string userName)
        {
            //accountViewModel.UserName = "Angkor";
            //accountViewModel.UserPass = "123456";
            //accountViewModel.ParentId = 10;
            //accountViewModel.MemberLevel = 5;
            return Json(new { }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult GetAccount(int memberId = 0)
        {
            return Json(new { }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetMemberSummary(int memberLevel = 5)
        {

           
            return Json(new { }, JsonRequestBehavior.AllowGet);

        }

    }
}