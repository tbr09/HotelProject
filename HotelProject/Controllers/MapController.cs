using HotelProject.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HotelProject.Controllers
{
    public class MapController : Controller
    {
        // GET: Map
        public ActionResult Index()
        {
            StreamReader r = new StreamReader(Server.MapPath("~/data.json"));
            StreamReader x = new StreamReader(Server.MapPath("~/hotels.json"));
            string jsonAttractions = r.ReadToEnd();
            string jsonHotels = x.ReadToEnd();
            var vm = new MapViewModel
            {
                atractions = JsonConvert.DeserializeObject<List<Attraction>>(jsonAttractions),
                hotels = JsonConvert.DeserializeObject<List<Hotel>>(jsonHotels)
            };
            return View(vm);

        }
    }
}