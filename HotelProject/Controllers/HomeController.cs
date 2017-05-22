using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HotelProject.Models;
using System.IO;
using System.Diagnostics;

namespace HotelProject.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            IEnumerable<Point> hotels;
            IEnumerable<Point> attractions;
            using (StreamReader r = new StreamReader(Server.MapPath("~/hotels.json")))
            {
                string json = r.ReadToEnd();
                hotels = JsonConvert.DeserializeObject<IEnumerable<Point>>(json);
            }
            using (StreamReader r = new StreamReader(Server.MapPath("~/data.json")))
            {
                string json = r.ReadToEnd();
                attractions = JsonConvert.DeserializeObject<IEnumerable<Point>>(json);
            }
            var vm = new MainViewModel
            {
                hotels = hotels,
                attractions = attractions
            };

            List<SelectListItem> items = new List<SelectListItem>();
            for(int i =0; i<hotels.Count()-1;i++)
            {
                items.Add(new SelectListItem { Text = hotels.ElementAt(i).name, Value = i.ToString() });
            }
            ViewBag.HotelList = items;
            return View(vm);
        }

        public ActionResult Hotels()
        {
            using (StreamReader r = new StreamReader(Server.MapPath("~/hotels.json")))
            {
                string json = r.ReadToEnd();
                List<Hotel> hotels = JsonConvert.DeserializeObject<List<Hotel>>(json);
                return View(hotels);
            }
        }

        public ActionResult AddHotel()
        {
            Hotel hot = new Hotel();

            return View(hot);
        }

        [HttpPost]
        public ActionResult AddHotel(Hotel hot)
        {
            if (ModelState.IsValid)
            {
                List<Hotel> hotelList = null;
                using (StreamReader r = new StreamReader(Server.MapPath("~/hotels.json")))
                {
                    string json = r.ReadToEnd();
                    hotelList = JsonConvert.DeserializeObject<List<Hotel>>(json);
                }

                hotelList.Add(hot);

                using (StreamWriter w = new StreamWriter(Server.MapPath("~/hotels.json")))
                {
                    var jsonOut = JsonConvert.SerializeObject(hotelList);
                    w.Write(jsonOut);
                }
                return RedirectToAction("Hotels", "Home");
            }
            return View(hot);
        }

        public ActionResult AddAttraction()
        {
            Attraction att = new Attraction();

            return View(att);
        }

        [HttpPost]
        public ActionResult AddAttraction(Attraction att)
        {
            if (ModelState.IsValid)
            {
                List<Attraction> attractionList = null;
                using (StreamReader r = new StreamReader(Server.MapPath("~/data.json")))
                {
                    string json = r.ReadToEnd();
                    attractionList = JsonConvert.DeserializeObject<List<Attraction>>(json);
                }

                attractionList.Add(att);

                using (StreamWriter w = new StreamWriter(Server.MapPath("~/data.json")))
                {
                    var jsonOut = JsonConvert.SerializeObject(attractionList);
                    w.Write(jsonOut);
                }
                return RedirectToAction("Attractions", "Home");
            }
            return View(att);
        }

        public ActionResult EditAttraction(string id)
        {
            List<Attraction> attractionList = null;
            using (StreamReader r = new StreamReader(Server.MapPath("~/data.json")))
            {
                string json = r.ReadToEnd();
                attractionList = JsonConvert.DeserializeObject<List<Attraction>>(json);
            }
            var att = attractionList.Find(k => k.id == id);

            return View(att);
        }

        [HttpPost]
        public ActionResult EditAttraction(Attraction att)
        {
            if (ModelState.IsValid)
            {
                List<Attraction> attractionList = null;
                using (StreamReader r = new StreamReader(Server.MapPath("~/data.json")))
                {
                    string json = r.ReadToEnd();
                    attractionList = JsonConvert.DeserializeObject<List<Attraction>>(json);
                }

                int ind = attractionList.IndexOf(attractionList.Find(k => k.id == att.id));
                if (ind != -1) attractionList[ind] = att;

                using (StreamWriter w = new StreamWriter(Server.MapPath("~/data.json")))
                {
                    var jsonOut = JsonConvert.SerializeObject(attractionList);
                    w.Write(jsonOut);
                }
            }
            return RedirectToAction("Attractions", "Home");
        }

        public ActionResult DeleteAttraction(string id)
        {
            List<Attraction> attractionList = null;
            using (StreamReader r = new StreamReader(Server.MapPath("~/data.json")))
            {
                string json = r.ReadToEnd();
                attractionList = JsonConvert.DeserializeObject<List<Attraction>>(json);
            }

            attractionList.Remove(attractionList.Find(k => k.id == id));

            using (StreamWriter w = new StreamWriter(Server.MapPath("~/data.json")))
            {
                var jsonOut = JsonConvert.SerializeObject(attractionList);
                w.Write(jsonOut);
            }
            return RedirectToAction("Attractions", "Home");
        }

        public ActionResult DeleteHotel(string id)
        {
            List<Hotel> hotelList = null;
            using (StreamReader r = new StreamReader(Server.MapPath("~/hotels.json")))
            {
                string json = r.ReadToEnd();
                hotelList = JsonConvert.DeserializeObject<List<Hotel>>(json);
            }

            hotelList.Remove(hotelList.Find(k => k.id == id));

            using (StreamWriter w = new StreamWriter(Server.MapPath("~/hotels.json")))
            {
                var jsonOut = JsonConvert.SerializeObject(hotelList);
                w.Write(jsonOut);
            }
            return RedirectToAction("Hotels", "Home");
        }


        public ActionResult EditHotel(string id)
        {
            List<Hotel> hotelList = null;
            using (StreamReader r = new StreamReader(Server.MapPath("~/hotels.json")))
            {
                string json = r.ReadToEnd();
                hotelList = JsonConvert.DeserializeObject<List<Hotel>>(json);
            }
            var att = hotelList.Find(k => k.id == id);

            return View(att);
        }

        [HttpPost]
        public ActionResult EditHotel(Hotel hot)
        {
            if (ModelState.IsValid)
            {
                List<Hotel> hotelList = null;
                using (StreamReader r = new StreamReader(Server.MapPath("~/hotels.json")))
                {
                    string json = r.ReadToEnd();
                    hotelList = JsonConvert.DeserializeObject<List<Hotel>>(json);
                }

                int ind = hotelList.IndexOf(hotelList.Find(k => k.id == hot.id));
                if (ind != -1) hotelList[ind] = hot;

                using (StreamWriter w = new StreamWriter(Server.MapPath("~/hotels.json")))
                {
                    var jsonOut = JsonConvert.SerializeObject(hotelList);
                    w.Write(jsonOut);
                }
            }
            return RedirectToAction("Hotels", "Home");
        }

        public ActionResult Attractions()
        {
            using (StreamReader r = new StreamReader(Server.MapPath("~/data.json")))
            {
                string json = r.ReadToEnd();
                List<Attraction> atractions = JsonConvert.DeserializeObject<List<Attraction>>(json);

                /* //removing object without rating
                List<Attraction> attrList1 = new List<Attraction>();
                foreach (Attraction x in attrList)
                {
                    if (x.rating != 0) attrList1.Add(x);
                }

                using (StreamWriter w = new StreamWriter(Server.MapPath("~/data.json")))
                {
                    var jsonOut = JsonConvert.SerializeObject(attrList1);
                    w.Write(jsonOut);
                }
                */
                return View(atractions);
            }
        }

    }
}