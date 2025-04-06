using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
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
            ViewData["name"] = f.Name;
            return View();
        }


        public ActionResult SearchRedirect(Route rr)
        {
           var f=  ent.Routes.Where(b=>b.FromLocation.Equals(rr.FromLocation) && b.ToLocation.Equals(rr.ToLocation));
            Session["from"] = rr.FromLocation;
            Session["to"] = rr.ToLocation;
            if (rr.Mode.ToLower() == "bus")
            {
                return RedirectToAction("busbook");
            }
            else if(rr.Mode.ToLower() == "train")
            {
                return RedirectToAction("trainbook");

            }
            else if (rr.Mode.ToLower() == "flight")
            {
                return RedirectToAction("flightbook");
            }
            else
            {
                return Redirect("Userdash");
            }

        }
        public ActionResult busbook()
        {
            string from = Session["from"].ToString();
            string to = Session["to"].ToString();

            var buses = ent.Buses
                           .Include("Route") 
                           .Where(b => b.Route.FromLocation == from && b.Route.ToLocation == to)
                           .ToList();

            return View(buses);

        }

        public ActionResult SelectSeat(int BusId)
        {
            var seats = ent.Seats
        .Where(b => b.BusId == BusId && b.IsBooked == true)
        .Select(b => b.SeatNumber.ToString())
        .ToList();
            var bus = ent.Buses.Include("Route").FirstOrDefault(b=>b.BusId == BusId);
            Session["Booked"] = string.Join(",", seats);

            return View(bus);
        }

        public ActionResult ConfirmBooking()
        {
            int busid = int.Parse(Request.Params["BusId"]);
            string seatnumbers = Request.Params["SelectedSeats"];

            string[] seatnum = seatnumbers.Split(',');
            Session["busid"] = busid;
            Session["seatnum"] = seatnumbers;
            foreach (string s in seatnum) {
            
               int seatno= int.Parse(s);

                Seat st = new Seat()
                {

                    BusId = busid,
                    SeatNumber = seatno.ToString(),
                    IsBooked = true



                };

                ent.Seats.Add(st);  
                 
            
            }
            ent.SaveChanges();

            return RedirectToAction("Boardandpickup");
        }


        public ActionResult Boardandpickup()

        {
           

            return View();
        }

        public ActionResult bookinginfo()
        {
            string board = Request.Params["board"];
            string pickup = Request.Params["pickup"];

            Session["board"] = board;
            Session["pickup"] = pickup;
            Session["fare"] = 500;

            var selectedSeats = (Session["seatnum"]?.ToString() ?? "").Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

            List<ConfirmBooking> bookings = new List<ConfirmBooking>();
            foreach (var seat in selectedSeats)
            {
                bookings.Add(new ConfirmBooking
                {
                    SeatNumber = Convert.ToInt32(seat)
                });
            }

            return View(bookings); 
        }

        public ActionResult savemultiplebooking(List<ConfirmBooking> Bookings)
        {
            foreach (var booking in Bookings)
            {
                booking.BookingTime = DateTime.Now;

      
                ent.ConfirmBookings.Add(booking);
            }

            ent.SaveChanges();
            return RedirectToAction("message");
        }

        public ActionResult message()
        {
            return View();
        }
        public ActionResult printticket()
        {
            var email = Request.Params["email"];
            var f = ent.ConfirmBookings.FirstOrDefault(b => b.Email == email);

            return View(f);
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