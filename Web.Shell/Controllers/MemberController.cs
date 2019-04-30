using System.Web.Mvc;

namespace Web.Shell.Controllers
{

    /// <summary>
    /// 
    /// </summary>
    public class MemberController : Controller
    {
        public ViewResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string account, string loginPwd)
        {
         
            return Json(new { }, JsonRequestBehavior.AllowGet);
        }

       

        public ActionResult ChangePassword(string newPwd,string oldPwd,string repeatPwd)
        {
            return Json(new { });

        }


        [HttpPost]
        public ActionResult Logout()
        {
            return Json(new { });
        }
    }
}