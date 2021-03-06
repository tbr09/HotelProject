﻿using HotelProject.Models;
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
        public double bestRemoveSt;
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


            List<SelectListItem> items = new List<SelectListItem>();
            for (int i = 0; i < hotels.Count() - 1; i++)
            {
                items.Add(new SelectListItem { Text = hotels.ElementAt(i).name, Value = i.ToString() });
            }
            ViewBag.SelectedHotel = items;

            MapViewModel vm = new MapViewModel(attractions as List<Point>, hotels as List<Point>);

            return View(vm);
        }

        [HttpPost]
        public ActionResult Index(int SelectedHotel, MapViewModel vm)
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


            List<SelectListItem> items = new List<SelectListItem>();
            for (int i = 0; i < hotels.Count() - 1; i++)
            {
                items.Add(new SelectListItem { Text = hotels.ElementAt(i).name, Value = i.ToString() });
            }
            ViewBag.SelectedHotel = items;

            Debug.WriteLine(hotels.ElementAt(SelectedHotel));

            TourResult _tour = Alg(hotels.ElementAt(SelectedHotel), vm.infoModel.days, vm.infoModel.distanceLimit, vm.infoModel.seconds);
            //TourResult _tour = Alg1(hotels.ElementAt(SelectedHotel), vm.infoModel.seconds);
            //List<Travel> tour = new List<Travel>();

            var viewModel = new MapViewModel
            {
                attractions = attractions as List<Point>,
                hotels = hotels as List<Point>,
                tourResult = _tour,
                infoModel = vm.infoModel
            };

            return View(viewModel);
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
            //return Math.Floor(Math.Sqrt(Math.Pow(lon1 - lon2, 2) + Math.Pow(lat1 - lat2, 2)));


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
            double best1 = 0, best2 = 0, distanceTravel1 = 0, distanceTravel2 = 0, bestScore;
            decimal ratingTravel1 = 0, ratingTravel2 = 0;
            int changePlace = 0, i = 0, j = 1, ind;

            foreach (Travel travel in tour)
            {
                i = 0; //change
                travel.attractionList.Insert(0, travel.sourceHotel);
                travel.attractionList.Add(travel.destinationHotel);
                foreach (Travel travel1 in tour)
                {
                    travel1.attractionList.Insert(0, travel1.sourceHotel);
                    travel1.attractionList.Add(travel1.destinationHotel);
                    if (travel != travel1)
                    {
                        while (i < 100)
                        {
                            changePlace = 0;
                            bestScore = 0;
                            j = 1;
                            ind = rand.Next(1, travel.attractionList.Count - 1);
                            while (j < travel1.attractionList.Count - 1)
                            {
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

                                if (distanceTravel1 < best1 && distanceTravel2 < best2 && distanceTravel1 < distanceLimit)
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
                    travel1.attractionList.RemoveAt(0);
                    travel1.attractionList.RemoveAt(travel1.attractionList.Count - 1);
                }
                travel.attractionList.RemoveAt(0);
                travel.attractionList.RemoveAt(travel.attractionList.Count - 1);
            }
        }

        public void LocalSearchPrototype(Travel travel, int howNeighbours, List<Item>[] distancesLI, List<Point> visited, List<Point> att, double distanceLimit)
        {
            int i = travel.attractionList.Count - 1, j = 0, ind;
            double newDistance;
            Point newPoint;
            List<Item> neighbours;

            while (travel.totalDistance < distanceLimit && i > 0)
            {
                neighbours = new List<Item>();
                j = 0;
                ind = att.IndexOf(travel.attractionList[i]);

                foreach (Point item in att)
                {
                    if (item != travel.attractionList[i]) neighbours.Add(new Item(item, DistanceBetweenPlaces(item, travel.attractionList[i])));
                }
                neighbours = neighbours.OrderBy(k => k.distance).ToList();

                newPoint = neighbours.First().direction;
                while (j < howNeighbours)
                {
                    if (!visited.Contains(newPoint))
                    {

                        newDistance = travel.totalDistance;

                        newDistance -= DistanceBetweenPlaces(travel.attractionList[i], travel.attractionList[i - 1]);
                        newDistance += DistanceBetweenPlaces(travel.attractionList[i - 1], newPoint);
                        newDistance += DistanceBetweenPlaces(newPoint, travel.attractionList[i]);

                        if (newDistance < distanceLimit)
                        {
                            travel.attractionList.Insert(i, newPoint);
                            travel.totalRating += newPoint.rating;
                            travel.totalDistance = newDistance;
                            visited.Add(newPoint);
                        }
                        else { i = 0; j = howNeighbours; }
                    }
                    j++;
                    newPoint = neighbours[j].direction;
                }
                i--;
            }
        }

        public void Remove(Travel travel, List<Point> attList, Random rand, double distanceLimit)
        {
            int j = 1, worstPlace = 0, prob;
            double newDistance = 0, worstScore = 0, distanceWorst = 0, newScore = 0;
            decimal newRating, worstRating;

            travel.attractionList.Insert(0, travel.sourceHotel);
            travel.attractionList.Add(travel.destinationHotel);
            List<Point> newList = new List<Point>();

            newDistance = travel.totalDistance;
            newDistance -= DistanceBetweenPlaces(travel.attractionList[j], travel.attractionList[j + 1]);
            newDistance -= DistanceBetweenPlaces(travel.attractionList[j - 1], travel.attractionList[j]);
            newDistance += DistanceBetweenPlaces(travel.attractionList[j - 1], travel.attractionList[j + 1]);
            newRating = travel.totalRating - travel.attractionList[j].rating;
            worstScore = (double)travel.attractionList[j].rating / newDistance;
            worstPlace = j;
            worstRating = newRating;
            distanceWorst = newDistance;
            while (j < travel.attractionList.Count - 1)
            {
                newDistance = travel.totalDistance;
                newDistance -= DistanceBetweenPlaces(travel.attractionList[j], travel.attractionList[j + 1]);
                newDistance -= DistanceBetweenPlaces(travel.attractionList[j - 1], travel.attractionList[j]);
                newDistance += DistanceBetweenPlaces(travel.attractionList[j - 1], travel.attractionList[j + 1]);
                newRating = travel.totalRating - travel.attractionList[j].rating;
                newScore = (double)travel.attractionList[j].rating / newDistance;

                if (newScore > bestRemoveSt && newScore < travel.score)
                {
                    bestRemoveSt = newScore;
                    worstScore = newScore;
                    worstPlace = j;
                    distanceWorst = newDistance;
                }
                j++;
            }
            if (/*bestScore > (double)travel.totalRating / travel.totalDistance && */ worstPlace != 0)
            {
                Debug.WriteLine("Removing " + travel.attractionList[worstPlace].name + "(" + travel.attractionList[worstPlace].rating + ") between " + travel.attractionList[worstPlace - 1].name + " and " + travel.attractionList[worstPlace + 1].name);
                //Debug.WriteLine(travel.attractionList[worstPlace].geometry.location.lat + " . " + travel.attractionList[worstPlace].geometry.location.lng);
                travel.totalRating -= travel.attractionList[worstPlace].rating;
                attList.Add(travel.attractionList[worstPlace]);
                travel.attractionList.RemoveAt(worstPlace);
                travel.totalDistance = distanceWorst;
            }

            travel.attractionList.RemoveAt(0);
            travel.attractionList.RemoveAt(travel.attractionList.Count - 1);
        }

        public void InsertSpecific(Travel travel, List<Point> attList, Random rand, double distanceLimit, Point newPoint)
        {
            int i = 0, j = 1; int bestPlace = 0;
            double newDistance = 0, bestScore = 0, distanceBest = 0;
            decimal newRating;

            travel.attractionList.Insert(0, travel.sourceHotel);
            travel.attractionList.Add(travel.destinationHotel);
            while (j < travel.attractionList.Count)
            {
                newDistance = travel.totalDistance;

                newDistance -= DistanceBetweenPlaces(travel.attractionList[j - 1], travel.attractionList[j]);
                newDistance += DistanceBetweenPlaces(travel.attractionList[j - 1], newPoint);
                newDistance += DistanceBetweenPlaces(newPoint, travel.attractionList[j]);

                newRating = travel.totalRating + newPoint.rating;
                if ((double)newRating / newDistance > bestScore)
                {
                    bestScore = (double)newRating / newDistance;
                    bestPlace = j;
                    distanceBest = newDistance;
                }
                j++;
            }

            Debug.WriteLine("Inserting " + newPoint.name + " between ");
            travel.attractionList.Insert((int)bestPlace, newPoint);
            travel.totalDistance = distanceBest;

            travel.attractionList.RemoveAt(0);
            travel.attractionList.RemoveAt(travel.attractionList.Count - 1);
            i++;
        }

        public void Insert(Travel travel, List<Point> attList, Random rand, double distanceLimit)
        {
            int j = 1, ind = 0, bestPlace = 0, bestIndex = 0;
            double newDistance = 0, bestScore = 0, distanceBest = 0;
            decimal newRating;
            Point current = null;

            travel.attractionList.Insert(0, travel.sourceHotel);
            travel.attractionList.Add(travel.destinationHotel);
            while (ind < attList.Count)
            {
                current = attList[ind];

                j = 1;
                while (j < travel.attractionList.Count)
                {
                    newDistance = travel.totalDistance;
                    newDistance -= DistanceBetweenPlaces(travel.attractionList[j - 1], travel.attractionList[j]);
                    newDistance += DistanceBetweenPlaces(travel.attractionList[j - 1], current);
                    newDistance += DistanceBetweenPlaces(current, travel.attractionList[j]);
                    newRating = travel.totalRating + current.rating;
                    if ((double)newRating / newDistance > bestScore)
                    {
                        bestScore = (double)newRating / newDistance;
                        bestPlace = j;
                        distanceBest = newDistance;
                        bestIndex = ind;
                    }
                    j++;
                }
                ind++;
            }

            if (bestPlace != 0 && distanceBest < distanceLimit)
            {
                //Debug.WriteLine("Inserting " + attList[bestIndex].name + " " + attList[bestIndex].rating + " between " + travel.attractionList[bestPlace - 1].name + " and " + travel.attractionList[bestPlace].name);
                travel.attractionList.Insert(bestPlace, attList[bestIndex]);
                travel.totalRating += attList[bestIndex].rating;
                attList.RemoveAt(bestIndex);
                travel.totalDistance = distanceBest;
            }
            travel.attractionList.RemoveAt(0);
            travel.attractionList.RemoveAt(travel.attractionList.Count - 1);

        }

        public void MoveBest(Travel travel, List<Point> visited, List<Point> attList, Random rand, double distanceLimit)
        {
            double newDistance = 0, newScore, bestDistance = 0, travelDist = 0;
            int bestPlace = 0, ind, j = 1, i = 1;
            Point newPoint = null;

            travel.attractionList.Insert(0, travel.sourceHotel);
            travel.attractionList.Insert(travel.attractionList.Count - 1, travel.destinationHotel);
            while (i < 50)
            {
                ind = rand.Next(1, travel.attractionList.Count - 1);
                newPoint = travel.attractionList[ind];
                j = 1;
                bestDistance = travel.totalDistance;
                travelDist = travel.totalDistance;
                travelDist -= DistanceBetweenPlaces(newPoint, travel.attractionList[ind - 1]);
                travelDist -= DistanceBetweenPlaces(newPoint, travel.attractionList[ind + 1]);
                travelDist += DistanceBetweenPlaces(travel.attractionList[ind - 1], travel.attractionList[ind + 1]);
                travel.attractionList.Remove(newPoint);

                while (j < travel.attractionList.Count - 1)
                {
                    newDistance = travelDist;

                    newDistance -= DistanceBetweenPlaces(travel.attractionList[j], travel.attractionList[j - 1]);
                    newDistance += DistanceBetweenPlaces(travel.attractionList[j - 1], newPoint);
                    newDistance += DistanceBetweenPlaces(travel.attractionList[j], newPoint);
                    newScore = (double)(travel.totalRating + newPoint.rating) / newDistance;
                    if (newDistance < bestDistance)
                    {
                        bestPlace = j;
                        bestDistance = newDistance;
                    }
                    j++;
                }
                if (bestDistance != 0 && bestDistance < travel.totalDistance && bestDistance < distanceLimit)
                {
                    travel.attractionList.Insert(bestPlace, newPoint);
                    travel.totalDistance = bestDistance;
                }
                else
                {

                    travel.attractionList.Insert(ind, newPoint);
                }
                i++;
            }
            travel.attractionList.RemoveAt(0);
            travel.attractionList.RemoveAt(travel.attractionList.Count - 1);
        }

        public void Replace(Travel travel, List<Point> attList, Random rand, double distanceLimit)
        {
            int j = 1, bestPlace = 0, bestChoice = 0;
            double newDistance = 0, bestScore = 0, distanceBest1 = 0, distanceBest = 0;
            decimal newRating;

            travel.attractionList.Insert(0, travel.sourceHotel);
            travel.attractionList.Add(travel.destinationHotel);
            bestScore = 0;
            while (j < travel.attractionList.Count - 1)
            {
                newDistance = travel.totalDistance;
                newDistance -= DistanceBetweenPlaces(travel.attractionList[j], travel.attractionList[j - 1]);
                newDistance -= DistanceBetweenPlaces(travel.attractionList[j], travel.attractionList[j + 1]);
                newRating = travel.totalRating - travel.attractionList[j].rating;
                if ((double)newRating / newDistance > bestScore)
                {
                    bestScore = (double)newRating / newDistance;
                    bestPlace = j;
                    distanceBest = newDistance;
                }
                j++;
            }
            bestScore = 0;
            j = 0;
            if (bestPlace != 0)
            {
                while (j < attList.Count - 1)
                {
                    newDistance = distanceBest;
                    newDistance += DistanceBetweenPlaces(attList[j], travel.attractionList[bestPlace - 1]);
                    newDistance += DistanceBetweenPlaces(attList[j], travel.attractionList[bestPlace + 1]);
                    newRating = travel.totalRating + attList[j].rating;
                    if ((double)newRating / newDistance > bestScore)
                    {
                        bestScore = (double)newRating / newDistance;
                        distanceBest1 = newDistance;
                        bestChoice = j;
                    }
                    j++;
                }
            }

            if (bestScore > travel.score)
            {
                attList.Add(travel.attractionList[bestPlace]);
                travel.totalRating -= travel.attractionList[bestPlace].rating;
                travel.attractionList.RemoveAt(bestPlace);
                travel.totalRating += attList[bestChoice].rating;
                travel.attractionList.Insert(bestPlace, attList[bestChoice]);
                attList.RemoveAt(bestChoice);
                travel.totalDistance = distanceBest1;
            }
            travel.attractionList.RemoveAt(0);
            travel.attractionList.RemoveAt(travel.attractionList.Count - 1);
        }

        public void TwoOpt(Travel travel, List<Item>[] distanceLI)
        {
            Travel tempTravel = new Travel(travel.sourceHotel, travel.destinationHotel);
            Random rand = new Random();
            int checkingNumber, k1, k2, k, i = 0;

            travel.attractionList.Insert(0, travel.sourceHotel);
            travel.attractionList.Add(travel.destinationHotel);

            while (i < travel.attractionList.Count * 30)
            {
                k = 0;
                checkingNumber = rand.Next(0, travel.attractionList.Count - 1);
                k2 = rand.Next(checkingNumber + 1, travel.attractionList.Count);
                k1 = k2 - checkingNumber;
                List<Point> reversedList = new List<Point>();
                List<Point> newList = new List<Point>();

                for (int j = 0; j < k2; j++)
                {
                    if (j > k1 - 1 && j < k2)
                    {
                        reversedList.Add(travel.attractionList[j]);
                    }
                }
                reversedList.Reverse();
                for (int j = 0; j < travel.attractionList.Count; j++)
                {
                    if (j > k1 - 1 && j < k2)
                    {
                        newList.Add(reversedList[k]); k++;
                    }
                    else newList.Add(travel.attractionList[j]);
                }

                if (reversedList.Count == 0) { i++; continue; }
                tempTravel.attractionList = newList;
                tempTravel.totalDistance = travel.totalDistance;

                tempTravel.totalDistance -= (DistanceBetweenPlaces(travel.attractionList[k1 - 1], reversedList.Last()) + DistanceBetweenPlaces(travel.attractionList[k2], reversedList.First()));
                tempTravel.totalDistance += (DistanceBetweenPlaces(travel.attractionList[k1 - 1], reversedList.First()) + DistanceBetweenPlaces(travel.attractionList[k2], reversedList.Last()));

                if (tempTravel.totalDistance < travel.totalDistance)
                {
                    travel.attractionList = tempTravel.attractionList;
                    travel.totalDistance = tempTravel.totalDistance;
                }
                i++;
            }
            travel.attractionList.RemoveAt(0);
            travel.attractionList.RemoveAt(travel.attractionList.Count - 1);
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

        public void Extract(int iterations, Travel travel, List<Point> attList, List<Item>[] distanceLI)
        {
            Travel tempTravel = new Travel(travel.sourceHotel, travel.destinationHotel);
            Random rand = new Random();
            int checkingNumber, k1, k2, i = 0, bk1 = 0, bk2 = 0;
            decimal bestRating = 0;
            Travel bestTravel = null;

            travel.attractionList.Insert(0, travel.sourceHotel);
            travel.attractionList.Add(travel.destinationHotel);

            checkingNumber = rand.Next(0, travel.attractionList.Count / 10);
            k2 = rand.Next(checkingNumber + 1, travel.attractionList.Count);
            k1 = k2 - checkingNumber;
            List<Point> reversedList = new List<Point>();
            List<Point> newList = new List<Point>();

            while (i < travel.attractionList.Count)
            {
                tempTravel.totalRating = travel.totalRating;
                newList = new List<Point>();
                for (int j = 0; j < travel.attractionList.Count - 1; j++)
                {
                    if (j > k1 - 1 && j < k2)
                    {
                        tempTravel.totalRating -= travel.attractionList[j].rating;
                    }
                    else newList.Add(travel.attractionList[j]);
                }

                if (newList.Count != 0)
                {
                    tempTravel.attractionList = newList;
                    tempTravel.totalDistance = tempTravel.CalculateDistance();

                    if (tempTravel.totalDistance < travel.totalDistance && tempTravel.totalRating > bestRating)
                    {
                        bestTravel = tempTravel;
                        bk1 = k1;
                        bk2 = k2;
                    }
                }
                i++;
            }
            if (bestTravel != null)
            {
                Debug.WriteLine("Extracting");
                travel.attractionList = bestTravel.attractionList;
                travel.totalRating = bestTravel.totalRating;
                travel.totalDistance = bestTravel.totalDistance;

                for (int j = bk1 - 1; j < bk2 - 1; j++)
                {
                    attList.Remove(travel.attractionList[j]);
                }
            }
            travel.attractionList.RemoveAt(0);
            travel.attractionList.RemoveAt(travel.attractionList.Count - 1);
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
                    else { Debug.WriteLine("Reapeating in travel " + j + item.attractionList[i].rating + " " + item.attractionList[i].name + " index:" + i); }
                }
                Debug.WriteLine("Travel(" + item.attractionList.Count + " points) " + j + " rating check - " + ratingCheck + " & travel distance check - " + distanceCheck);
                j++;
            }
        }

        //test data
        public TourResult Alg1(Point selectedHotel, int seconds)
        {
            StreamReader sr = new StreamReader(Server.MapPath("~/F.txt"));
            if (seconds == 0) seconds++;
            string s;
            string[] separators = { " ", "," };
            List<Point> attList = new List<Point>();
            List<Point> hotelList = new List<Point>();
            s = sr.ReadLine();
            string[] words = s.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            int N = int.Parse(words[0]), days = int.Parse(words[1]);
            double distanceLimit = int.Parse(words[2]), oldDistance;
            Point newPoint;

            int h = 1;
            #region 
            do
            {
                s = sr.ReadLine();
                if (s != null)
                {
                    words = s.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    newPoint = new Point();
                    newPoint.geometry.location.lat = Decimal.Parse(words[0], System.Globalization.NumberStyles.Float);
                    newPoint.geometry.location.lng = Decimal.Parse(words[1], System.Globalization.NumberStyles.Float);
                    newPoint.id = h.ToString();
                    if (words.Length == 3)
                    {
                        newPoint.rating = decimal.Parse(words[2]);
                        attList.Add(newPoint);
                        h++;
                    }
                    else hotelList.Add(newPoint);
                }
            }
            while (s != null);
            #endregion

            DateTime totalStart = DateTime.Now, start, stop;
            Random rand = new Random();
            Point last;

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
            int i = 0;

            //pseudoGreedy for travel
            #region
            //ind = hotelList.IndexOf(travel.sourceHotel);
            //Point current = hotelsLI[ind].First().direction;
            //travel.addAtt(current, distanceLI, ind);
            //visited.Add(current);
            //oldDistance = travel.totalDistance;
            //start = DateTime.Now;
            //while (travel.totalDistance < distanceLimit)
            //{
            //    j = 0;
            //    ind = attList.IndexOf(travel.attractionList.Last());
            //    newPoint = LI[ind].First().direction;
            //    while (j < howNeighbours)
            //    {
            //        if (!visited.Contains(newPoint))
            //        {
            //            travel.addAtt(newPoint, distanceLI, ind);
            //            visited.Add(newPoint);
            //        }
            //        LI[ind].RemoveAt(0);
            //        newPoint = LI[ind].First().direction;
            //        j++;
            //    }
            //    nearDestinationHotel = new List<Item>();
            //    foreach (Point item1 in travel.attractionList)
            //    {
            //        nearDestinationHotel.Add(new Item(item1, DistanceBetweenPlaces(travel.destinationHotel, item1)));
            //    }
            //    nearDestinationHotel = nearDestinationHotel.OrderBy(k => k.distance).ToList();
            //    ind = travel.attractionList.IndexOf(nearDestinationHotel.First().direction);
            //    travel.attractionList = twoOptSample(ind, travel.attractionList.Count + 1, travel.attractionList);
            //    travel.totalDistance = travel.CalculateDistance();
            //    i++;
            //}
            //stop = DateTime.Now;
            //Debug.WriteLine("\nPseudoGreedy->" + (stop - start).TotalMilliseconds + "ms");
            //Debug.WriteLine("Old Distance->" + oldDistance + "  New Distance->" + travel.totalDistance);
            //Debug.WriteLine("Score-> " + travel.totalRating);


            ////pseudoGreedy for travel1
            //i = 0;
            //ind = hotelList.IndexOf(travel1.sourceHotel);
            //current = hotelsLI[ind].First().direction;
            //while (travel.attractionList.Contains(current))
            //{
            //    current = hotelsLI[ind].First().direction;
            //    hotelsLI[ind].RemoveAt(0);
            //}
            //travel1.addAtt(current, distanceLI, ind);
            //visited.Add(current);
            //oldDistance = travel1.totalDistance;
            //start = DateTime.Now;
            //while (travel1.totalDistance < distanceLimit)
            //{
            //    j = 0;
            //    ind = attList.IndexOf(travel1.attractionList.Last());
            //    newPoint = LI[ind].First().direction;
            //    while (j < howNeighbours)
            //    {
            //        if (!visited.Contains(newPoint))
            //        {
            //            travel1.addAtt(newPoint, distanceLI, ind);
            //            visited.Add(newPoint);
            //        }
            //        LI[ind].RemoveAt(0);
            //        newPoint = LI[ind].First().direction;
            //        j++;
            //    }
            //    nearDestinationHotel = new List<Item>();
            //    foreach (Point item1 in travel1.attractionList)
            //    {
            //        nearDestinationHotel.Add(new Item(item1, DistanceBetweenPlaces(travel1.destinationHotel, item1)));
            //    }
            //    nearDestinationHotel = nearDestinationHotel.OrderBy(k => k.distance).ToList();
            //    ind = travel1.attractionList.IndexOf(nearDestinationHotel.First().direction);
            //    travel1.attractionList = twoOptSample(ind, travel1.attractionList.Count + 1, travel1.attractionList);
            //    travel1.totalDistance = travel1.CalculateDistance();
            //    i++;
            //}
            //stop = DateTime.Now;
            //Debug.WriteLine("\nPseudoGreedy->" + (stop - start).TotalMilliseconds + "ms");
            //Debug.WriteLine("Old Distance->" + oldDistance + "  New Distance->" + travel1.totalDistance);
            //Debug.WriteLine("Score-> " + travel1.totalRating);
            #endregion

            //pseudoGreedy for travel with scores (best for data)
            #region 
            foreach (Point item1 in attList)
            {
                nearDestinationHotel.Add(new Item(item1, DistanceBetweenPlaces(travel.sourceHotel, item1)));
            }
            nearDestinationHotel = nearDestinationHotel.OrderBy(k => k.score).ToList();
            newPoint = nearDestinationHotel.First().direction;
            travel.addAtt(newPoint, distanceLI);
            attList.Remove(newPoint);
            oldDistance = travel.totalDistance;
            start = DateTime.Now;
            last = travel.attractionList.Last();
            i = 0;
            while (travel.totalDistance < distanceLimit)
            {
                nearDestinationHotel = new List<Item>();
                foreach (Point item1 in attList)
                {
                    nearDestinationHotel.Add(new Item(item1, DistanceBetweenPlaces(last, item1)));
                }
                nearDestinationHotel = nearDestinationHotel.OrderBy(k => k.score).ToList();
                newPoint = nearDestinationHotel.First().direction;
                while (visited.Contains(newPoint))
                {
                    nearDestinationHotel.RemoveAt(0);
                    newPoint = nearDestinationHotel.First().direction;
                }
                travel.addAtt(newPoint, distanceLI);
                if(travel.CalculateDistance() > distanceLimit)
                {
                    travel.attractionList.Remove(newPoint);
                    travel.totalRating -= newPoint.rating;
                    break;
                }
                attList.Remove(newPoint);
                last = newPoint;
                travel.totalDistance = travel.CalculateDistance();
                i++;
            }
            stop = DateTime.Now;
            Debug.WriteLine("\nPseudoGreedy->" + (stop - start).TotalMilliseconds + "ms");
            Debug.WriteLine("Old Distance->" + oldDistance + "  New Distance->" + travel.totalDistance);
            Debug.WriteLine("Score-> " + travel.totalRating);


            //pseudoGreedy for travel1 with scores
            i = 0;
            foreach (Point item1 in attList)
            {
                nearDestinationHotel.Add(new Item(item1, DistanceBetweenPlaces(travel1.sourceHotel, item1)));
            }
            nearDestinationHotel = nearDestinationHotel.OrderBy(k => k.score).ToList();
            newPoint = nearDestinationHotel.First().direction;
            while (travel.attractionList.Contains(newPoint))
            {
                newPoint = nearDestinationHotel.First().direction;
                nearDestinationHotel.RemoveAt(0);
            }
            travel1.addAtt(newPoint, distanceLI);
            attList.Remove(newPoint);
            oldDistance = travel1.totalDistance;
            start = DateTime.Now;
            i = 0;
            last = travel.attractionList.Last();
            while (travel1.totalDistance < distanceLimit)
            {
                nearDestinationHotel = new List<Item>();
                foreach (Point item1 in attList)
                {
                    nearDestinationHotel.Add(new Item(item1, DistanceBetweenPlaces(last, item1)));
                }
                nearDestinationHotel = nearDestinationHotel.OrderBy(k => k.score).ToList();
                newPoint = nearDestinationHotel.First().direction;
                while (visited.Contains(newPoint))
                {
                    nearDestinationHotel.RemoveAt(0);
                    newPoint = nearDestinationHotel.First().direction;
                }
                travel1.addAtt(newPoint, distanceLI);
                if (travel1.CalculateDistance() > distanceLimit)
                {
                    travel1.attractionList.Remove(newPoint);
                    travel1.totalRating -= newPoint.rating;
                    break;
                }
                attList.Remove(newPoint);
                last = newPoint;
                travel1.totalDistance = travel1.CalculateDistance();
                i++;
            }
            stop = DateTime.Now;
            Debug.WriteLine("\nPseudoGreedy->" + (stop - start).TotalMilliseconds + "ms");
            Debug.WriteLine("Old Distance->" + oldDistance + "  New Distance->" + travel1.totalDistance);
            Debug.WriteLine("Score-> " + travel1.totalRating);
            #endregion

            //pseudoGreedy for travel with scores and Inserting
            #region
            //foreach (Point item1 in attList)
            //{
            //    nearDestinationHotel.Add(new Item(item1, DistanceBetweenPlaces(travel.sourceHotel, item1)));
            //}
            //nearDestinationHotel = nearDestinationHotel.OrderBy(k => k.score).ToList();
            //newPoint = nearDestinationHotel.First().direction;
            //travel.addAtt(newPoint, distanceLI);
            //// visited.Add(newPoint);

            //attList.Remove(newPoint);
            //oldDistance = travel.totalDistance;
            //start = DateTime.Now;
            //while (travel.totalDistance < distanceLimit)
            //{
            //    last = travel.attractionList.Last();
            //    nearDestinationHotel = new List<Item>();
            //    foreach (Point item1 in attList)
            //    {
            //        nearDestinationHotel.Add(new Item(item1, DistanceBetweenPlaces(last, item1)));
            //    }
            //    nearDestinationHotel = nearDestinationHotel.OrderBy(k => k.score).ToList();
            //    newPoint = nearDestinationHotel.First().direction;
            //    while (!attList.Contains(newPoint))
            //    {
            //    nearDestinationHotel.RemoveAt(0);
            //    newPoint = nearDestinationHotel.First().direction;
            //     }
            //    //if (!visited.Contains(newPoint))
            //    //{
            //    InsertSpecific(travel, attList, rand, distanceLimit, newPoint);
            //    travel.totalRating += newPoint.rating;
            //    attList.Remove(newPoint);
            //    //}
            //    travel.totalDistance = travel.CalculateDistance();
            //    //TwoOpt(iterations, travel, distanceLI);
            //}
            //stop = DateTime.Now;
            //Debug.WriteLine("\nPseudoGreedy->" + (stop - start).TotalMilliseconds + "ms");
            //Debug.WriteLine("Old Distance->" + oldDistance + "  New Distance->" + travel.totalDistance);
            //Debug.WriteLine("Score-> " + travel.totalRating);

            ////pseudoGreedy for travel1 with scores and Inserting
            //foreach (Point item1 in attList)
            //{
            //    nearDestinationHotel.Add(new Item(item1, DistanceBetweenPlaces(travel1.sourceHotel, item1)));
            //}
            //nearDestinationHotel = nearDestinationHotel.OrderBy(k => k.score).ToList();
            //newPoint = nearDestinationHotel.First().direction;
            //while (travel.attractionList.Contains(newPoint))
            //{
            //    newPoint = nearDestinationHotel.First().direction;
            //    nearDestinationHotel.RemoveAt(0);
            //}
            //travel1.addAtt(newPoint, distanceLI);
            //visited.Add(newPoint);
            //i = 0;
            //ind = hotelList.IndexOf(travel1.sourceHotel);
            //newPoint = hotelsLI[ind].First().direction;
            //while (travel.attractionList.Contains(newPoint))
            //{
            //    newPoint = hotelsLI[ind].First().direction;
            //    hotelsLI[ind].RemoveAt(0);
            //}
            //travel1.addAtt(newPoint, distanceLI);
            ////visited.Add(newPoint);
            //attList.Remove(newPoint);
            //oldDistance = travel1.totalDistance;
            //start = DateTime.Now;
            //while (travel1.totalDistance < distanceLimit)
            //{
            //    last = travel1.attractionList.Last();
            //    nearDestinationHotel = new List<Item>();
            //    foreach (Point item1 in attList)
            //    {
            //        nearDestinationHotel.Add(new Item(item1, DistanceBetweenPlaces(last, item1)));
            //    }
            //    nearDestinationHotel = nearDestinationHotel.OrderBy(k => k.score).ToList();
            //    newPoint = nearDestinationHotel.First().direction;
            //    while (!attList.Contains(newPoint))
            //    {
            //        nearDestinationHotel.RemoveAt(0);
            //        newPoint = nearDestinationHotel.First().direction;
            //    }
            //    //if (!visited.Contains(newPoint))
            //    //{
            //    InsertSpecific(travel1, attList, rand, distanceLimit, newPoint);
            //    travel1.totalRating += newPoint.rating;
            //    attList.Remove(newPoint);
            //    //}
            //    travel1.totalDistance = travel1.CalculateDistance();
            //    //TwoOpt(iterations, travel1, distanceLI);
            //}
            //stop = DateTime.Now;
            //Debug.WriteLine("\nPseudoGreedy->" + (stop - start).TotalMilliseconds + "ms");
            //Debug.WriteLine("Old Distance->" + oldDistance + "  New Distance->" + travel1.totalDistance);
            //Debug.WriteLine("Score-> " + travel1.totalRating);
            #endregion

            List<Travel> tour = new List<Travel>();
            tour.Add(travel); tour.Add(travel1);
            Checker(tour);

            //Checker(tour);
            DateTime check = DateTime.Now;
            decimal lastScore = travel.totalRating + travel1.totalRating;
            while (true)
            {
                if ((DateTime.Now - check).TotalSeconds >= seconds) break;
                foreach (Travel item in tour)
                {
                    TwoOpt(item, distanceLI);
                    Insert(item, attList, rand, distanceLimit);
                    Replace(item, attList, rand, distanceLimit);
                }
            }

            //Remove(tour[0], attList, rand, distanceLimit);
            //DateTime x = DateTime.Now;
            //TwoOpt(iterations, tour[0], distanceLI);
            //Debug.WriteLine((DateTime.Now - x).TotalMilliseconds + " 2opt time");
            //x = DateTime.Now;
            //Insert(tour[0], attList, rand, distanceLimit);
            //Debug.WriteLine((DateTime.Now - x).TotalMilliseconds + " insert time");
            //x = DateTime.Now;
            //Remove(tour[0], attList, rand, distanceLimit);
            //Debug.WriteLine((DateTime.Now - x).TotalMilliseconds + " remove time");
            //x = DateTime.Now;
            //Replace(travel, attList, rand, distanceLimit);
            //Debug.WriteLine((DateTime.Now - x).TotalMilliseconds + " replace time");
            //x = DateTime.Now;
            //TravelSwapping(tour, rand, distanceLimit);
            //Debug.WriteLine((DateTime.Now - x).TotalMilliseconds + " swap time");
            //x = DateTime.Now;
            ////Extract(iterations, travel, attList, distanceLI);
            //Debug.WriteLine((DateTime.Now - x).TotalMilliseconds + " extract time");

            //checker
            Checker(tour);

            Debug.WriteLine((14500 - (travel1.totalRating + travel.totalRating)) / 145 + "%");

            //scores
            i = 1;
            using (StreamWriter sw = new StreamWriter(Server.MapPath("~/output.txt")))
            using (StreamWriter sw1 = new StreamWriter(Server.MapPath("~/outputWPF.txt")))
            {
                sw.WriteLine(days);
                foreach (Travel item in tour)
                {
                    sw1.WriteLine(item.sourceHotel.geometry.location.lat + " " + item.sourceHotel.geometry.location.lng);
                    sw.Write(item.sourceHotel.id + " ");
                    foreach (Point x in item.attractionList)
                    {
                        sw1.WriteLine(x.geometry.location.lat + " " + x.geometry.location.lng + " " + x.rating);
                        sw.Write(x.id + " ");
                    }
                    Debug.WriteLine("Travel(" + i + ")Rating-> " + item.totalRating + "(" + item.totalDistance + ") TotalScore " + item.score);
                    i++;
                    sw1.WriteLine(item.sourceHotel.geometry.location.lat + " " + item.sourceHotel.geometry.location.lng);
                    sw.WriteLine(item.sourceHotel.id);
                }
            }
            stop = DateTime.Now;
            Debug.WriteLine(">Total time-> " + (stop - totalStart).TotalMilliseconds);


            TourResult result = new TourResult
            {
                tour = tour,
                score = (double)(travel1.totalRating + travel.totalRating)
            };


            //tryin to execute wpf window

            //string folder = Server.MapPath("") + "/../../";
            //Debug.WriteLine(folder);
            //TestVisualization.MainWindow pb = new TestVisualization.MainWindow();
            //Debug.WriteLine(@"D:\Git\HotelProject\TestVisualization\bin\Debug" + "\\TestVisualization.exe");
            ////Process.Start(@"D:\Git\HotelProject\TestVisualization\bin\Debug" + "\\TestVisualization.exe");

            //Process process = new Process();
            //process.StartInfo.FileName = @"D:\Git\HotelProject\TestVisualization\bin\Debug" + @"\TestVisualization.exe";
            //process.Start();
            //Process p = new Process();
            //p.StartInfo.FileName = @"D:\Git\HotelProject\TestVisualization\bin\Debug" + "\\TestVisualization.exe";
            //p.Start();


            return result;
        }

        /// <summary>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// </summary>
        /// <returns></returns>

        //real coords
        public TourResult Alg(Point selectedHotel, int days, int distanceLimit, int seconds)
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
            List<Item>[] distanceLI = new List<Item>[att.Count];
            List<Item>[] LI = new List<Item>[att.Count];
            List<Item>[] hotelsLI = new List<Item>[hot.Count];
            List<Item> nearDestinationHotel = new List<Item>();
            GenerateLI(distanceLI, att);
            GenerateLI(LI, att);
            GenerateHotLI(hotelsLI, att, hot);
            stop = DateTime.Now;
            Debug.WriteLine("Generating LI Time->" + (stop - start).TotalMilliseconds + "ms");

            start = DateTime.Now;
            List<Point> visited = new List<Point>();

            Debug.WriteLine(DistanceBetweenPlaces(hot.Where(k => k.name == "Hotel Branicki ****").Single(), hot.Where(k => k.name == "Apartamenty i Restauracja Janus").Single()));

            double oldDistance;
            int i = 0;

            List<Travel> tour = new List<Travel>();
            for (int j = 0; j < days; j++)
            {
                tour.Add(new Travel(selectedHotel, selectedHotel));
            }

            Point last;
            Point newPoint;
            //Point sourceHot = hot.Where(k => k.name == "Hotel Willa Port Art & Business").Single();
            //Point destinationHot = hot.Where(k => k.name == "Hotel Branicki ****").Single();
            //Point destinationHot = hot.Where(k => k.name == "Hotel Willa Port Art & Business").Single();
            //Point destinationHot = hot.Where(k => k.name == "Hotel ATENA ***").Single();

            //pseudoGreedy
            #region
            //int ind = hot.IndexOf(sourceHot);
            //Point current = hotelsLI[ind].First().direction;
            //travel.addAtt(current, distanceLI, ind);
            //visited.Add(current);
            //oldDistance = travel.totalDistance;
            //start = DateTime.Now;
            //while (travel.totalDistance < distanceLimit)
            //{
            //    j = 0;
            //    ind = att.IndexOf(travel.attractionList.Last());
            //    Point newPoint = LI[ind].First().direction;
            //    while (j < howNeighbours)
            //    {
            //        if (!visited.Contains(newPoint))
            //        {
            //            travel.addAtt(newPoint, distanceLI, ind);
            //            visited.Add(newPoint);
            //        }
            //        LI[ind].RemoveAt(0);
            //        newPoint = LI[ind].First().direction;
            //        j++;
            //    }
            //    nearDestinationHotel = new List<Item>();
            //    foreach (Point item1 in travel.attractionList)
            //    {
            //        nearDestinationHotel.Add(new Item(item1, DistanceBetweenPlaces(travel.destinationHotel, item1)));
            //    }
            //    nearDestinationHotel = nearDestinationHotel.OrderBy(k => k.distance).ToList();
            //    ind = travel.attractionList.IndexOf(nearDestinationHotel.First().direction);
            //    travel.attractionList = twoOptSample(ind, travel.attractionList.Count + 1, travel.attractionList);
            //    travel.totalDistance = travel.CalculateDistance();
            //    i++;
            //}
            //stop = DateTime.Now;
            //Debug.WriteLine("\nPseudoGreedy->" + (stop - start).TotalMilliseconds + "ms");
            //Debug.WriteLine("Old Distance->" + oldDistance + "  New Distance->" + travel.totalDistance);
            //Debug.WriteLine("Score-> " + travel.totalRating);
            #endregion

            //pseudoGreedy for travel with scores and Inserting and 2opt
            #region
            //int ind = hot.IndexOf(travel.sourceHotel);
            //Point current = hotelsLI[ind].First().direction;
            //travel.addAtt(current, distanceLI);
            //visited.Add(current);
            //oldDistance = travel.totalDistance;
            //start = DateTime.Now;

            //foreach (Travel travel in tour)
            //{
            //    while (travel.totalDistance < distanceLimit)
            //    {
            //        last = travel.attractionList.Last();
            //        nearDestinationHotel = new List<Item>();
            //        foreach (Point item1 in att)
            //        {
            //            nearDestinationHotel.Add(new Item(item1, DistanceBetweenPlaces(last, item1)));
            //        }
            //        nearDestinationHotel = nearDestinationHotel.OrderBy(k => k.score).ToList();
            //        newPoint = nearDestinationHotel.First().direction;
            //        //while (visited.Contains(newPoint))
            //        //{
            //            nearDestinationHotel.RemoveAt(0);
            //            newPoint = nearDestinationHotel.First().direction;
            //        //}
            //        //if (!visited.Contains(newPoint))
            //        //{
            //            InsertSpecific(travel, att, rand, distanceLimit, newPoint);
            //            travel.totalRating += newPoint.rating;
            //        att.Remove(newPoint);
            //        //}
            //        TwoOpt(iterations, travel, distanceLI);
            //        travel.totalDistance = travel.CalculateDistance();
            //    }
            //    stop = DateTime.Now;
            //    Debug.WriteLine("\nPseudoGreedy->" + (stop - start).TotalMilliseconds + "ms");
            //    Debug.WriteLine("Old Distance->" + oldDistance + "  New Distance->" + travel.totalDistance);
            //    Debug.WriteLine("Score-> " + travel.totalRating);
            //}
            #endregion


            //pseudoGreedy for travel with scores
            #region
            foreach (Travel travel in tour)
            {
                nearDestinationHotel = new List<Item>();
                foreach (Point item1 in att)
                {
                    nearDestinationHotel.Add(new Item(item1, DistanceBetweenPlaces(selectedHotel, item1)));
                }
                nearDestinationHotel = nearDestinationHotel.OrderBy(k => k.distance).ToList();
                newPoint = nearDestinationHotel.First().direction;
                travel.addAtt(newPoint, distanceLI);
                att.Remove(newPoint);
                oldDistance = travel.totalDistance;
                start = DateTime.Now;
                last = travel.attractionList.Last();
                i = 0;
                while (travel.totalDistance < distanceLimit && att.Count() > 0)
                {
                    nearDestinationHotel = new List<Item>();
                    foreach (Point item1 in att)
                    {
                        nearDestinationHotel.Add(new Item(item1, DistanceBetweenPlaces(last, item1)));
                    }
                    nearDestinationHotel = nearDestinationHotel.OrderBy(k => k.score).ToList();
                    newPoint = nearDestinationHotel.First().direction;
                    travel.addAtt(newPoint, distanceLI);
                    if (travel.CalculateDistance() > distanceLimit)
                    {
                        travel.attractionList.Remove(newPoint);
                        travel.totalRating -= newPoint.rating;
                        break;
                    }
                    att.Remove(newPoint);
                    last = newPoint;
                    travel.totalDistance = travel.CalculateDistance();
                    i++;
                }
                stop = DateTime.Now;
                Debug.WriteLine("\nPseudoGreedy->" + (stop - start).TotalMilliseconds + "ms");
                Debug.WriteLine("Old Distance->" + oldDistance + "  New Distance->" + travel.totalDistance);
                Debug.WriteLine("Score-> " + travel.totalRating);
            }
            #endregion

            DateTime check = DateTime.Now;
            while (true)
            {
                if ((DateTime.Now - check).TotalSeconds >= seconds) break;
                foreach (Travel item in tour)
                {
                    TwoOpt(item, distanceLI);
                    Insert(item, att, rand, distanceLimit);
                    Replace(item, att, rand, distanceLimit);
                }
                //TravelSwapping(tour, rand, distanceLimit);
            }

            //checker
            Checker(tour);

            //scores
            i = 1;
            using (StreamWriter sw = new StreamWriter(Server.MapPath("~/output.txt")))
            {
                foreach (Travel item in tour)
                {
                    sw.WriteLine(item.sourceHotel.geometry.location.lat + " " + item.sourceHotel.geometry.location.lng);
                    foreach (Point x in item.attractionList)
                    {
                        sw.WriteLine(x.geometry.location.lat + " " + x.geometry.location.lng + " " + x.rating);
                    }
                    Debug.WriteLine("Travel(" + i + ")Rating-> " + item.totalRating + "(" + item.totalDistance + ") TotalScore " + item.score);
                    i++;
                    sw.WriteLine(item.sourceHotel.geometry.location.lat + " " + item.sourceHotel.geometry.location.lng);
                }
            }
            stop = DateTime.Now;
            Debug.WriteLine("Total time-> " + (stop - totalStart).TotalMilliseconds);

            decimal score = 0;
            foreach (var x in tour)
            {
                score += x.totalRating;
            }
            //ViewModel
            TourResult result = new TourResult
            {
                tour = tour,
                score = (double)score
            };


            return result;
        }
    }
}