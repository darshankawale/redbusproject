using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SqlTypes;
using System.Diagnostics.Contracts;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml.Linq;
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

            Session["tdate"] = Request.Params["Date"];
           var f=  ent.Routes.Where(b=>b.FromLocation.Equals(rr.FromLocation) && b.ToLocation.Equals(rr.ToLocation));
            Session["from"] = rr.FromLocation;
            Session["to"] = rr.ToLocation;
            Session["rti"] = rr.RouteId;
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
            string tdate= Session["tdate"].ToString() ;
            

         DateTime travelDate = DateTime.Parse(tdate);

            var buses = ent.Buses
                           .Include("Route") 
                           .Where(b => b.Route.FromLocation == from && b.Route.ToLocation == to && DbFunctions.TruncateTime(b.DepartureTime) == travelDate.Date)
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

            int routeId = Convert.ToInt32(Session["rti"]);

          
            var boardingPoints = ent.BoardingPoints
                .Where(b => b.RouteId == routeId)
                .ToList();
            ViewData["bbs"] = new SelectList(boardingPoints, "BoardingPointId", "BoardingPointName");

           
            var pickupPoints = ent.PickupPoints
                .Where(p => p.RouteId == routeId)
                .ToList();
            ViewData["pps"] = new SelectList(pickupPoints, "PickupPointId", "PickupPointName");


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
            if(f == null)
            {
                return RedirectToAction("Userdash");
            }
            return View(f);
        }

        public ActionResult cancel()
        {
            return View();
        }
        public ActionResult cancelticket()
        {
            var email = Request.Params["email"];
            var f = ent.ConfirmBookings.FirstOrDefault(b => b.Email == email);
            decimal cfare = f.Fare - 100;
            CancelBooking cs = new CancelBooking()
            {
                BusId = f.BusId,
                SeatNumber = f.SeatNumber,
                Name = f.Name,
                Email = f.Email,
                Mobile= f.Mobile,
                Age= f.Age, 
                cutFare= cfare,
                cancelTime = DateTime.Now

            };
            ent.CancelBookings.Add(cs);
            ent.SaveChanges();  
            ent.ConfirmBookings.Remove(f);
            ent.SaveChanges();
            Seat st = new Seat()
            {

                BusId = f.BusId,
                SeatNumber = f.SeatNumber.ToString(),
                IsBooked = false



            };

            ent.Seats.Add(st);
            ent.SaveChanges();  
            return RedirectToAction("Userdash");
        }
        public ActionResult About()
        {
            return View();
        }

        public ActionResult AdLogin()
        {

            return View();
        }
        [HttpPost]
        public ActionResult AdLogind()
        {
            string email = Request.Params["Email"];
            string pass = Request.Params["Password"];
            if (email.Equals("Admin") && pass.Equals("123"))
            {
                return RedirectToAction("Admindash");
            }

            return View();
        }

        public ActionResult Admindash()
        {
            return View();
        }


        public ActionResult AddRoutes()
        {
            Route rr= new Route();
            return View(rr);
        }

        [HttpPost]
        public ActionResult AddRoutes( Route re)
        {

                var rou = new Route()
                {
                    FromLocation = re.FromLocation,
                    ToLocation = re.ToLocation,
                    Mode = re.Mode,
                };

                ent.Routes.Add(rou);
            
            ent.SaveChanges();  
            return RedirectToAction("manageroutes");
        }

        public ActionResult manageroutes()
        {

            var f= ent.Routes.ToList(); 
            return View(f);
        }

        public ActionResult Editroutes(int id)

        {
            var f = ent.Routes.Find(id);

            return View(f);
        }
        [HttpPost]
        public ActionResult Editroutes(Route rz)

        {
            var f = ent.Routes.Find(rz.RouteId);

            f.ToLocation= rz.ToLocation;
            f.FromLocation= rz.FromLocation;
            f.Mode= rz.Mode;

            ent.Routes.AddOrUpdate(f);
            ent.SaveChanges();
            return RedirectToAction("manageroutes");
        }

       





        public ActionResult deleteroute(int id)
        {
            var f= ent.Routes.Find(id);
            ent.Routes.Remove(f);
            ent.SaveChanges();
            return RedirectToAction("manageroutes");

        }


        public ActionResult Addpickup()
        {
            var routes = ent.Routes.ToList();

            var routeList = routes.Select(r => new
            {
                RouteId = r.RouteId,
                DisplayText = r.FromLocation + " → " + r.ToLocation
            }).ToList();

            ViewBag.Routes = new SelectList(routeList, "RouteId", "DisplayText");

            return View();
        }
        [HttpPost]
        public ActionResult Addpickupp   ()
        {
            int routeid = int.Parse(Request.Params["RouteId"]);
            string boardg = Request.Params["BoardingPoint"];
            string pickg = Request.Params["PickupPoint"];

            var board = new BoardingPoint
            {
                RouteId = routeid,
                PointName = boardg
            };
            ent.BoardingPoints.Add(board);

            var pick = new PickupPoint
            {
                RouteId = routeid,
                PointName = pickg
            };
            ent.PickupPoints.Add(pick);

            ent.SaveChanges();
            return RedirectToAction("manageroutes");
        }

        public ActionResult Addbuses()
        {
            var routes = ent.Routes.ToList();

            var routeList = routes.Select(r => new
            {
                RouteId = r.RouteId,
                DisplayText = r.FromLocation + " → " + r.ToLocation
            }).ToList();

            ViewBag.Routes = new SelectList(routeList, "RouteId", "DisplayText");
            Bus bus = new Bus();
            return View(bus);
        }
        [HttpPost]
        public ActionResult Addbuses( Bus bd)
        {
            string fname= Path.GetFileNameWithoutExtension(bd.FileBase.FileName);   

            string ex= Path.GetExtension(bd.FileBase.FileName);

            fname = fname + ex;
            bd.filename= "~/busimages/"+fname;
            fname = Path.Combine(Server.MapPath("~/busimages/") + fname);
            bd.FileBase.SaveAs(fname);  

           ent.Buses.Add(bd);
            ent.SaveChanges();
            return RedirectToAction("managesbuses");
        }

        public ActionResult managebuses()
        {
            var f= ent.Buses.ToList();  
            return View(f);
        }
        public ActionResult Editbuses(int id)

        {
            var f = ent.Buses.Find(id);
            ViewBag.RouteId = new SelectList(ent.Routes.ToList(), "RouteId", "FromLocation", f.RouteId);


            return View(f);
        }
        [HttpPost]
        public ActionResult Editbuses(Bus rz , HttpPostedFileBase FileBase)

        {
            var f = ent.Buses.Find(rz.BusId);

           

            
                string fname = Path.GetFileNameWithoutExtension(FileBase.FileName);
                string ext = Path.GetExtension(FileBase.FileName);
                string uniqueName = fname + "_" + Guid.NewGuid().ToString().Substring(0, 8) + ext;

                string relativePath = "~/busimages/" + uniqueName;
                string fullPath = Server.MapPath(relativePath);

                FileBase.SaveAs(fullPath);
                f.filename = relativePath;
          

        
            f.BusName = rz.BusName;
            f.ArrivalTime = rz.ArrivalTime;
            f.DepartureTime = rz.DepartureTime;
            f.BusType = rz.BusType;
            f.RouteId = rz.RouteId;
            f.TotalSeats = rz.TotalSeats;

        
            ent.Entry(f).State = System.Data.Entity.EntityState.Modified;
            ent.SaveChanges();

            return RedirectToAction("Managebuses");


        }

        public ActionResult deletebuses(int id)
        {
            var f = ent.Buses.Find(id);
            ent.Buses.Remove(f);
            ent.SaveChanges();
            return RedirectToAction("managebuses");

        }


        public ActionResult bookingdetails()
        {
            var f= ent.ConfirmBookings.ToList();
            return View(f);
        }

        public ActionResult canceldetails()
        {
            var f = ent.CancelBookings.ToList();
            return View(f);
        }




        //public ActionResult extenddays()
        //{
        //    var today= DateTime.Today;
        //    var f= ent.Routes.Where(r=>r.TravelDate<today).ToList();    
        //    foreach(var r in f)
        //    {
        //        r.TravelDate = today.AddDays(5);

        //    }
        //    ent.SaveChanges (); 
        //    return View();
        //}

        public ActionResult Contact()
        {
            return View();
        }

    }
}