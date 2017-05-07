using HotelProject.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            return View(Alg());

        }

        public double Distance(Attraction a1, Attraction a2)
        {
            double x1 = (double)a1.geometry.location.lat;
            double x2 = (double)a2.geometry.location.lat;
            double y1 = (double)a1.geometry.location.lng;
            double y2 = (double)a2.geometry.location.lng;

            double x = Math.Sqrt(Math.Pow(Math.Pow((x2 - x1), 2) + (Math.Cos((x1 * Math.PI) / 180) * (y2 - y1)), 2)) * (40075.704 / 360.0);

            return x;
        }

        const double RADIUS = 6378.16;

        public static double Radians(double x)
        {
            return x * Math.PI / 180;
        }

        public static double DistanceBetweenPlaces(Point a1, Point a2)
        {
            //for testing data (Euclidean)

            //double lat1 = (double)a1.geometry.location.lat;
            //double lat2 = (double)a2.geometry.location.lat;
            //double lon1 = (double)a1.geometry.location.lng;
            //double lon2 = (double)a2.geometry.location.lng;
            //return Math.Sqrt(Math.Pow(lon1 - lon2, 2) + Math.Pow(lat1 - lat2, 2));


            //for real coords

            double lat1 = (double)a1.geometry.location.lat;
            double lat2 = (double)a2.geometry.location.lat;
            double lon1 = (double)a1.geometry.location.lng;
            double lon2 = (double)a2.geometry.location.lng;
            double dlon = Radians(lon2 - lon1);
            double dlat = Radians(lat2 - lat1);
            double a = (Math.Sin(dlat / 2) * Math.Sin(dlat / 2)) + Math.Cos(Radians(lat1)) * Math.Cos(Radians(lat2)) * (Math.Sin(dlon / 2) * Math.Sin(dlon / 2));
            double angle = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return angle * RADIUS;
        }

        public void GenerateHotLI(List<Item>[] LI, List<Point> att, List<Point> hot)
        {
            int i = 0;
            foreach (Point item in hot)
            {
                LI[i] = new List<Item>();

                foreach (Point item1 in att)
                {
                    LI[i].Add(new Item(item1, DistanceBetweenPlaces(item, item1)));
                }
                foreach (Point item1 in hot)
                {
                    if (item1 != item) LI[i].Add(new Item(item1, DistanceBetweenPlaces(item, item1)));
                }
                LI[i] = LI[i].OrderBy(k => k.distance).ToList();

                i++;
            }
        }
        public void GenerateLI(List<Item>[] LI, List<Point> att)
        {
            int i = 0;
            foreach (Point item in att)
            {
                LI[i] = new List<Item>();
                foreach (Point item1 in att)
                {
                    if (item1 != item) LI[i].Add(new Item(item1, DistanceBetweenPlaces(item, item1)));
                }
                LI[i] = LI[i].OrderBy(k => k.distance).ToList();
                i++;
            }
        }

        public void TravelSwapping(List<Travel> tour, Random rand, double distanceLimit)
        {
            double best1 = 0, best2 = 0;
            double distanceTravel1 = 0, distanceTravel2 = 0;
            decimal ratingTravel1 = 0, ratingTravel2 = 0;
            double bestScore;
            int changePlace = 0, i = 0, j = 1, ind;

            foreach (Travel travel in tour)
            {
                foreach (Travel travel1 in tour)
                {
                    if (travel != travel1)
                    {

                        while (i < 500)
                        {
                            changePlace = 0;
                            bestScore = 0;
                            j = 1;
                            ind = rand.Next(1, travel.attractionList.Count - 1);
                            while (j < travel1.attractionList.Count - 1)
                            {
                                //Travel tempTravel1 = new Travel(travel.sourceHotel, travel.destinationHotel);
                                //tempTravel1.attractionList = travel.attractionList;
                                //tempTravel1.totalDistance = travel.totalDistance;

                                distanceTravel1 = travel.totalDistance;
                                ratingTravel1 = travel.totalRating;
                                distanceTravel1 -= DistanceBetweenPlaces(travel.attractionList[ind], travel.attractionList[ind - 1]);
                                distanceTravel1 -= DistanceBetweenPlaces(travel.attractionList[ind], travel.attractionList[ind + 1]);
                                distanceTravel1 += DistanceBetweenPlaces(travel.attractionList[ind - 1], travel.attractionList[ind + 1]);
                                ratingTravel1 -= travel.attractionList[ind].rating;

                                distanceTravel2 = travel1.totalDistance;
                                ratingTravel2 = travel1.totalRating;
                                distanceTravel2 -= DistanceBetweenPlaces(travel1.attractionList[j - 1], travel1.attractionList[j]);
                                distanceTravel2 += DistanceBetweenPlaces(travel1.attractionList[j - 1], travel.attractionList[ind]);
                                distanceTravel2 += DistanceBetweenPlaces(travel.attractionList[ind], travel1.attractionList[j]);
                                ratingTravel2 += travel.attractionList[ind].rating;

                                if ((double)ratingTravel2 / distanceTravel2 > travel1.score && (double)ratingTravel1 / distanceTravel1 > travel.score && (double)ratingTravel2 / distanceTravel2 > bestScore && distanceTravel1 < distanceLimit && distanceTravel2 < distanceLimit)
                                {
                                    bestScore = (double)ratingTravel2 / distanceTravel2;
                                    changePlace = j;
                                    best1 = distanceTravel1;
                                    best2 = distanceTravel2;
                                }
                                j++;
                            }
                            if (changePlace != 0)
                            {
                                travel1.totalDistance = best2;
                                travel.totalDistance = best1;

                                travel1.totalRating = ratingTravel2;
                                travel.totalRating = ratingTravel1;

                                travel1.attractionList.Insert(changePlace, travel.attractionList[ind]);
                                travel.attractionList.RemoveAt(ind);
                            }
                            i++;
                        }
                    }
                }
            }
        }

        public void LocalSearchPrototype(Travel travel, int howNeighbours, List<Item>[] distancesLI, List<Point> visited, List<Point> att, double distanceLimit)
        {
            int i = travel.attractionList.Count - 1;
            int j = 0;
            int ind;
            //while(i<travel.attractionList.Count-1)
            //while (i > 0)
            while (travel.totalDistance < distanceLimit && i > 0)
            {
                List<Item> neighbours = new List<Item>();

                j = 0;
                ind = att.IndexOf(travel.attractionList[i]);
                foreach (Point item in att)
                {
                    if (item != travel.attractionList[i]) neighbours.Add(new Item(item, DistanceBetweenPlaces(item, travel.attractionList[i])));
                }
                neighbours = neighbours.OrderBy(k => k.distance).ToList();

                Point newPoint = neighbours.First().direction;
                while (j < howNeighbours)
                {
                    if (!visited.Contains(newPoint))
                    {

                        travel.totalRating += newPoint.rating;
                        //travel.totalDistance = travel.CalculateDistance();

                        travel.totalDistance -= DistanceBetweenPlaces(travel.attractionList[i], travel.attractionList[i - 1]);
                        travel.attractionList.Insert(i, newPoint);
                        travel.totalDistance += DistanceBetweenPlaces(travel.attractionList[i - 1], travel.attractionList[i]);
                        travel.totalDistance += DistanceBetweenPlaces(travel.attractionList[i], travel.attractionList[i + 1]);
                        //travel.addAtt(newPoint);
                        visited.Add(newPoint);
                    }
                    j++;
                    newPoint = neighbours[j].direction;
                }
                //i++;
                i--;
            }
        }

        public void Remove(Travel travel, List<Point> visited, List<Point> attList, Random rand, double distanceLimit)
        {
            int i = 0, j, ind;
            Point current;
            double newDistance = 0;
            decimal newRating;
            double worstScore = 0;
            int worstPlace = 0;
            double distanceWorst = 0;
            //while (i < attList.Count && travel.totalDistance < distanceLimit)
            //{
            j = 1;

            newDistance -= DistanceBetweenPlaces(travel.attractionList[j], travel.attractionList[j + 1]);
            newDistance -= DistanceBetweenPlaces(travel.attractionList[j - 1], travel.attractionList[j]);
            newDistance += DistanceBetweenPlaces(travel.attractionList[j - 1], travel.attractionList[j + 1]);
            newRating = travel.attractionList[j].rating;
            worstScore = (double)newRating / newDistance;
            worstPlace = j;
            distanceWorst = newDistance;
            while (j < travel.attractionList.Count - 1)
            {
                newDistance = 0;
                newDistance -= DistanceBetweenPlaces(travel.attractionList[j], travel.attractionList[j + 1]);
                newDistance -= DistanceBetweenPlaces(travel.attractionList[j - 1], travel.attractionList[j]);
                newDistance += DistanceBetweenPlaces(travel.attractionList[j - 1], travel.attractionList[j + 1]);
                newRating = travel.attractionList[j].rating;
                //if ((double)newRating / newDistance < worstScore)
                if (newDistance > distanceWorst)
                {
                    worstScore = (double)newRating / newDistance;
                    worstPlace = j;
                    distanceWorst = newDistance;
                }
                j++;
            }
            if (/*bestScore > (double)travel.totalRating / travel.totalDistance && */ worstPlace != 0)
            {
                Debug.WriteLine("Removing " + travel.attractionList[worstPlace].name + " between " + travel.attractionList[worstPlace - 1].name + " and " + travel.attractionList[worstPlace + 1].name);

                travel.totalRating -= travel.attractionList[worstPlace].rating;
                visited.Remove(travel.attractionList[worstPlace]);
                travel.attractionList.RemoveAt(worstPlace);
                travel.totalDistance -= distanceWorst;
                //travel.totalDistance = travel.CalculateDistance();
                //travel.totalDistance += DistanceBetweenPlaces(travel.attractionList[worstPlace - 1], travel.attractionList[worstPlace + 1]);
            }

            i++;
            //}
        }

        public void Insert(Travel travel, List<Point> visited, List<Point> attList, Random rand, double distanceLimit)
        {
            int i = 0, j, ind;
            Point current;
            double newDistance = 0;
            decimal newRating;
            double bestScore = 0;
            int bestPlace = 0;
            double distanceBest = 0;
            //while (i < attList.Count && travel.totalDistance < distanceLimit)
            //{
            j = 1;
            ind = rand.Next(0, attList.Count - 1);
            current = attList[ind];
            if (!visited.Contains(current))
            {
                bestPlace = 0;
                distanceBest = 0;
                bestScore = 0;
                while (j < travel.attractionList.Count - 1)
                {
                    newDistance = travel.totalDistance;
                    newDistance -= DistanceBetweenPlaces(travel.attractionList[j - 1], travel.attractionList[j]);
                    newDistance += DistanceBetweenPlaces(travel.attractionList[j - 1], attList[ind]);
                    newDistance += DistanceBetweenPlaces(attList[ind], travel.attractionList[j]);
                    newRating = travel.totalRating + current.rating;
                    if ((double)newRating / newDistance > bestScore)
                    {
                        bestScore = (double)newRating / newDistance;
                        bestPlace = j;
                        distanceBest = newDistance;
                    }
                    j++;
                }
                if (/*bestScore > (double)travel.totalRating / travel.totalDistance && */ bestPlace != 0 && distanceBest < distanceLimit)
                {
                    Debug.WriteLine("Inserting " + current.name + " between " + travel.attractionList[bestPlace - 1].name + " and " + travel.attractionList[bestPlace].name);
                    travel.attractionList.Insert(bestPlace, current);
                    travel.totalRating += current.rating;
                    visited.Add(current);
                    //travel.totalDistance = travel.CalculateDistance();
                    travel.totalDistance = distanceBest;
                }
            }
            i++;
            //}
        }

        public void TwoOpt(int iterations, Travel travel, List<Item>[] distanceLI)
        {
            Random rand = new Random();
            Travel tempTravel = new Travel(travel.sourceHotel, travel.destinationHotel);
            int checkingNumber, k1, k2, k, i = 0;

            while (i < iterations)
            {
                k = 0;
                checkingNumber = rand.Next(0, travel.attractionList.Count);
                //k1 = rand.Next(0, travel.attractionList.Count - checkingNumber);
                //k2 = k1 + rand.Next(0, checkingNumber);
                k2 = rand.Next(0, travel.attractionList.Count);
                k1 = k2 - checkingNumber;
                List<Point> reversedList = new List<Point>();
                List<Point> newList = new List<Point>();
                for (int j = 0; j < k2; j++)
                {
                    if (j > k1 - 1 && j < k2 - 1)
                    {
                        reversedList.Add(travel.attractionList[j]);
                    }
                }
                reversedList.Reverse();
                for (int j = 0; j < travel.attractionList.Count; j++)
                {
                    if (j > k1 - 1 && j < k2 - 1)
                    {
                        newList.Add(reversedList[k]); k++;
                    }
                    else newList.Add(travel.attractionList[j]);
                }

                if (reversedList.Count == 0 || k1 < 1) continue;
                tempTravel.attractionList = newList;
                tempTravel.totalDistance = travel.totalDistance;
                tempTravel.totalDistance -= (DistanceBetweenPlaces(travel.attractionList[k1 - 1], reversedList.Last()) + DistanceBetweenPlaces(travel.attractionList[k2 - 1], reversedList.First()));
                tempTravel.totalDistance += (DistanceBetweenPlaces(travel.attractionList[k1 - 1], reversedList.First()) + DistanceBetweenPlaces(travel.attractionList[k2 - 1], reversedList.Last()));

                if (tempTravel.totalDistance < travel.totalDistance)
                {
                    travel.attractionList = tempTravel.attractionList;
                    travel.totalDistance = tempTravel.totalDistance;
                }
                i++;
            }
        }

        public List<Point> twoOptSample(int x, int y, List<Point> pointList)
        {
            int k = 0;
            List<Point> reversedList = new List<Point>();
            List<Point> newList = new List<Point>();
            for (int i = 0; i < y; i++)
            {
                if (i > x - 1 && i < y - 1)
                {
                    reversedList.Add(pointList[i]);
                }
            }
            reversedList.Reverse();
            for (int i = 0; i < pointList.Count; i++)
            {
                if (i > x - 1 && i < y - 1)
                {
                    newList.Add(reversedList[k]); k++;
                }
                else newList.Add(pointList[i]);
            }
            return newList;
        }

        public void Checker(List<Travel> tour)
        {
            int j = 1;
            decimal ratingCheck = 0;
            double distanceCheck = 0;
            List<Point> travelCheck = new List<Point>();

            foreach (Travel item in tour)
            {
                ratingCheck = 0; distanceCheck = 0;
                distanceCheck += DistanceBetweenPlaces(item.sourceHotel, item.attractionList[0]);
                distanceCheck += DistanceBetweenPlaces(item.destinationHotel, item.attractionList[item.attractionList.Count - 1]);
                for (int i = 0; i < item.attractionList.Count; i++)
                {
                    ratingCheck += item.attractionList[i].rating;
                    if (!travelCheck.Contains(item.attractionList[i]))
                    {
                        if (i < item.attractionList.Count - 1)
                        {
                            distanceCheck += DistanceBetweenPlaces(item.attractionList[i], item.attractionList[i + 1]);
                        }
                        travelCheck.Add(item.attractionList[i]);
                    }
                    else { Debug.WriteLine("Reapeating in travel " + j); }
                }
                Debug.WriteLine("Travel(" + item.attractionList.Count + " points) " + j + " rating check - " + ratingCheck + " & travel distance check - " + distanceCheck);
                j++;
            }
        }

        //test data
        public MapViewModel Alg1()
        {
            StreamReader sr = new StreamReader(Server.MapPath("~/F.txt"));

            string s;
            string[] separators = { " ", "," };
            List<Point> attList = new List<Point>();
            List<Point> hotelList = new List<Point>();
            s = sr.ReadLine();
            string[] words = s.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            int N = int.Parse(words[0]);
            int days = int.Parse(words[1]);
            double distanceLimit = int.Parse(words[2]);
            double oldDistance;

            #region 
            do
            {
                s = sr.ReadLine();
                if (s != null)
                {
                    words = s.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    Point newPoint = new Point();
                    newPoint.geometry.location.lat = Decimal.Parse(words[0], System.Globalization.NumberStyles.Float);
                    newPoint.geometry.location.lng = Decimal.Parse(words[1], System.Globalization.NumberStyles.Float);
                    if (words.Length == 3)
                    {
                        newPoint.rating = decimal.Parse(words[2]);
                        attList.Add(newPoint);
                    }
                    else hotelList.Add(newPoint);
                }
            }
            while (s != null);
            #endregion

            DateTime totalStart = DateTime.Now;
            DateTime start;
            DateTime stop;
            Random rand = new Random();

            start = DateTime.Now;
            List<Item> nearDestinationHotel = new List<Item>();
            List<Item>[] distanceLI = new List<Item>[400];
            List<Item>[] LI = new List<Item>[400];
            List<Item>[] hotelsLI = new List<Item>[20];
            List<Point> visited = new List<Point>();
            GenerateLI(distanceLI, attList);
            GenerateLI(LI, attList);
            GenerateHotLI(hotelsLI, attList, hotelList);
            stop = DateTime.Now;
            Debug.WriteLine("Generating LI Time->" + (stop - start).TotalMilliseconds + "ms");

            Travel travel = new Travel(hotelList[0], hotelList[0]);
            Travel travel1 = new Travel(hotelList[0], hotelList[0]);
            int howNeighbours = 6;
            int i = 0, ind, j = 0;
            int iterations = 20000;

            //pseudoGreedy for travel
            ind = hotelList.IndexOf(travel.sourceHotel);
            Point current = hotelsLI[ind].First().direction;
            travel.addAtt(current, distanceLI, ind);
            visited.Add(current);
            oldDistance = travel.totalDistance;
            start = DateTime.Now;
            while (travel.totalDistance < distanceLimit)
            {
                j = 0;
                ind = attList.IndexOf(travel.attractionList.Last());
                Point newPoint = LI[ind].First().direction;
                while (j < howNeighbours)
                {
                    if (!visited.Contains(newPoint))
                    {
                        travel.addAtt(newPoint, distanceLI, ind);
                        visited.Add(newPoint);
                    }
                    LI[ind].RemoveAt(0);
                    newPoint = LI[ind].First().direction;
                    j++;
                }
                nearDestinationHotel = new List<Item>();
                foreach (Point item1 in travel.attractionList)
                {
                    nearDestinationHotel.Add(new Item(item1, DistanceBetweenPlaces(travel.destinationHotel, item1)));
                }
                nearDestinationHotel = nearDestinationHotel.OrderBy(k => k.distance).ToList();
                ind = travel.attractionList.IndexOf(nearDestinationHotel.First().direction);
                travel.attractionList = twoOptSample(ind, travel.attractionList.Count + 1, travel.attractionList);
                travel.totalDistance = travel.CalculateDistance();
                i++;
            }
            stop = DateTime.Now;
            Debug.WriteLine("\nPseudoGreedy->" + (stop - start).TotalMilliseconds + "ms");
            Debug.WriteLine("Old Distance->" + oldDistance + "  New Distance->" + travel.totalDistance);
            Debug.WriteLine("Score-> " + travel.totalRating);


            //pseudoGreedy for travel1
            i = 0;
            ind = hotelList.IndexOf(travel1.sourceHotel);
            current = hotelsLI[ind].First().direction;
            while (travel.attractionList.Contains(current))
            {
                current = hotelsLI[ind].First().direction;
                hotelsLI[ind].RemoveAt(0);
            }
            travel1.addAtt(current, distanceLI, ind);
            visited.Add(current);
            oldDistance = travel1.totalDistance;
            start = DateTime.Now;
            while (travel1.totalDistance < distanceLimit)
            {
                j = 0;
                ind = attList.IndexOf(travel1.attractionList.Last());
                Point newPoint = LI[ind].First().direction;
                while (j < howNeighbours)
                {
                    if (!visited.Contains(newPoint))
                    {
                        travel1.addAtt(newPoint, distanceLI, ind);
                        visited.Add(newPoint);
                    }
                    LI[ind].RemoveAt(0);
                    newPoint = LI[ind].First().direction;
                    j++;
                }
                nearDestinationHotel = new List<Item>();
                foreach (Point item1 in travel1.attractionList)
                {
                    nearDestinationHotel.Add(new Item(item1, DistanceBetweenPlaces(travel1.destinationHotel, item1)));
                }
                nearDestinationHotel = nearDestinationHotel.OrderBy(k => k.distance).ToList();
                ind = travel1.attractionList.IndexOf(nearDestinationHotel.First().direction);
                travel1.attractionList = twoOptSample(ind, travel1.attractionList.Count + 1, travel1.attractionList);
                travel1.totalDistance = travel1.CalculateDistance();
                i++;
            }
            stop = DateTime.Now;
            Debug.WriteLine("\nPseudoGreedy->" + (stop - start).TotalMilliseconds + "ms");
            Debug.WriteLine("Old Distance->" + oldDistance + "  New Distance->" + travel1.totalDistance);
            Debug.WriteLine("Score-> " + travel1.totalRating);


            List<Travel> tour = new List<Travel>();
            tour.Add(travel); tour.Add(travel1);
            int generations = 15;
            while (i < generations)
            {
                foreach (Travel item in tour)
                {
                    TwoOpt(iterations, item, distanceLI);
                    LocalSearchPrototype(item, howNeighbours, distanceLI, visited, attList, distanceLimit);
                    //item.totalDistance = item.CalculateDistance();\
                    TravelSwapping(tour, rand, distanceLimit);
                    //Insert(item, visited, attList, rand, distanceLimit);
                    
                }
                i++;
            }

            Remove(tour[0], visited, attList, rand, distanceLimit);

            //checker
            Checker(tour);

            //scores
            i = 1;
            foreach (Travel item in tour)
            {
                Debug.WriteLine("Travel(" + i + ")Score-> " + item.totalRating + "(" + item.totalDistance + ")");
                i++;
            }
            stop = DateTime.Now;
            Debug.WriteLine(">Total time-> " + (stop - totalStart).TotalMilliseconds);


            var vm = new MapViewModel
            {
                hotels = hotelList,
                attractions = attList,
                tour = tour
            };

            return vm;
        }

        /// <summary>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// </summary>
        /// <returns></returns>

        //real coords
        public MapViewModel Alg()
        {
            List<Point> att = new List<Point>();
            using (StreamReader r = new StreamReader(Server.MapPath("~/data.json")))
            {
                string json = r.ReadToEnd();
                att = JsonConvert.DeserializeObject<List<Point>>(json);
            }

            List<Point> hot = new List<Point>();
            using (StreamReader r = new StreamReader(Server.MapPath("~/hotels.json")))
            {
                string json = r.ReadToEnd();
                hot = JsonConvert.DeserializeObject<List<Point>>(json);
            }

            DateTime totalStart = DateTime.Now;
            DateTime start, stop;
            Random rand = new Random();

            start = DateTime.Now;
            List<Item>[] distanceLI = new List<Item>[400];
            List<Item>[] LI = new List<Item>[400];
            List<Item>[] hotelsLI = new List<Item>[20];
            List<Item> nearDestinationHotel = new List<Item>();
            GenerateLI(distanceLI, att);
            GenerateLI(LI, att);
            GenerateHotLI(hotelsLI, att, hot);
            stop = DateTime.Now;
            Debug.WriteLine("Generating LI Time->" + (stop - start).TotalMilliseconds + "ms");

            start = DateTime.Now;
            List<Point> visited = new List<Point>();

            double oldDistance, distanceLimit = 900;
            int iterations, howNeighbours = 5, i = 0, j = 0;

            Point sourceHot = hot.Where(k => k.name == "Hotel Willa Port Art & Business").Single();
            Point destinationHot = hot.Where(k => k.name == "Hotel Branicki ****").Single();
            //Point destinationHot = hot.Where(k => k.name == "Hotel ATENA ***").Single();
            Travel travel = new Travel(sourceHot, destinationHot);

            //replacing points (sucks)
            //start = DateTime.Now;
            //i = 0;
            //oldDistance = travel.totalDistance;
            //while (i < 100)
            //{
            //    k1 = rand.Next(0, 400);
            //    k2 = 0;
            //    while (k2 < travel.attractionList.Count - 1)
            //    {
            //        while (visited.Contains(att[k1]))
            //        {
            //            k1 = rand.Next(0, 400);
            //        }
            //        Point temPoint = travel.attractionList[k2];
            //        travel.attractionList[k2] = att[k1];
            //        if (travel.CalculateDistance() < travel.totalDistance)
            //        {
            //            travel.totalRating -= temPoint.rating;
            //            travel.totalRating += att[k1].rating;
            //            visited.Remove(temPoint);
            //            visited.Add(att[k1]);
            //            travel.totalDistance = travel.CalculateDistance();
            //        }
            //        else
            //        {
            //            travel.attractionList[k2] = temPoint;
            //        }
            //        k2++;
            //    }

            //    i++;
            //}
            //stop = DateTime.Now;
            //Debug.WriteLine("\nTryReplace Time(" + iterations + "-iterations)->" + (stop - start).TotalMilliseconds + "ms");
            //Debug.WriteLine("Old Distance->" + oldDistance + "  New Distance->" + travel.totalDistance);
            //Debug.WriteLine("Score-> " + travel.totalRating);

            //pseudoGreedy
            int ind = hot.IndexOf(sourceHot);
            Point current = hotelsLI[ind].First().direction;
            travel.addAtt(current, distanceLI, ind);
            visited.Add(current);
            oldDistance = travel.totalDistance;
            start = DateTime.Now;
            while (travel.totalDistance < distanceLimit)
            {
                j = 0;
                ind = att.IndexOf(travel.attractionList.Last());
                Point newPoint = LI[ind].First().direction;
                while (j < howNeighbours)
                {
                    if (!visited.Contains(newPoint))
                    {
                        travel.addAtt(newPoint, distanceLI, ind);
                        visited.Add(newPoint);
                    }
                    LI[ind].RemoveAt(0);
                    newPoint = LI[ind].First().direction;
                    j++;
                }
                nearDestinationHotel = new List<Item>();
                foreach (Point item1 in travel.attractionList)
                {
                    nearDestinationHotel.Add(new Item(item1, DistanceBetweenPlaces(travel.destinationHotel, item1)));
                }
                nearDestinationHotel = nearDestinationHotel.OrderBy(k => k.distance).ToList();
                ind = travel.attractionList.IndexOf(nearDestinationHotel.First().direction);
                travel.attractionList = twoOptSample(ind, travel.attractionList.Count + 1, travel.attractionList);
                travel.totalDistance = travel.CalculateDistance();
                i++;
            }
            stop = DateTime.Now;
            Debug.WriteLine("\nPseudoGreedy->" + (stop - start).TotalMilliseconds + "ms");
            Debug.WriteLine("Old Distance->" + oldDistance + "  New Distance->" + travel.totalDistance);
            Debug.WriteLine("Score-> " + travel.totalRating);


            List<Travel> tour = new List<Travel>();
            tour.Add(travel);
            iterations = 20000;
            int generations = 30;
            i = 0;
            while (i < generations)
            {
                foreach (Travel item in tour)
                {
                    TwoOpt(iterations, item, distanceLI);
                    //Insert(item, visited, att, rand, distanceLimit);
                    LocalSearchPrototype(item, howNeighbours, distanceLI, visited, att, distanceLimit);
                    //Remove(item, visited, att, rand, distanceLimit);
                    TravelSwapping(tour, rand, distanceLimit);
                }
                i++;
            }
            //Insert(tour[0], visited, att, rand, distanceLimit);
            Debug.WriteLine(tour[0].score);
            oldDistance = tour[0].totalDistance;
            Remove(tour[0], visited, att, rand, distanceLimit);
            Debug.WriteLine("Remove - Old Distance->" + oldDistance + "  New Distance->" + travel.totalDistance);
            Debug.WriteLine(tour[0].score);
            //checker
            Checker(tour);

            //scores
            i = 1;
            foreach (Travel item in tour)
            {
                Debug.WriteLine("Travel(" + i + ")Rating-> " + item.totalRating + "(" + item.totalDistance + ") TotalScore " + item.score);
                i++;
            }
            stop = DateTime.Now;
            Debug.WriteLine(">Total time-> " + (stop - totalStart).TotalMilliseconds);



            var vm = new MapViewModel
            {
                hotels = hot,
                attractions = att,
                tour = tour
            };

            return vm;
        }
    }
}