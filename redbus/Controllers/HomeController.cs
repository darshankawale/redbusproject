using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using redbus;
namespace redbus.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home

        RedbusEntities1 ent = new RedbusEntities1();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Register()
        {
            User u= new User();
            
            
            return View(u);
        }


        [HttpPost]

        public ActionResult Register(User uss)
        {
            ent.Users.Add(uss);
            ent.SaveChanges();  

            return RedirectToAction("Login");
        }


        public ActionResult Login()
        {
            User uw = new User();

            return View(uw);
        }
        [HttpPost]
        public ActionResult Login(User ue)
        {
          var a=   ent.Users.FirstOrDefault(m => m.Email.Equals(ue.Email) && m.Password.Equals(ue.Password));
            if (a != null) {

                Session["userid"] = a.UserId;


                return RedirectToAction("Userdash");
            
            }
            return View();
        }

         
        
        public ActionResult Userdash()
        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("Login");
            }
            int  UserId =(int)Session["userid"];

           var f=  ent.Users.Find(UserId);

            return View(f);
        }

        public ActionResult busbook()
        {
            // add a cooment
            return View();
        }
        public ActionResult About()
        {
            return View();
        }
        public ActionResult Contact()
        {
            return View();
        }

    }
}