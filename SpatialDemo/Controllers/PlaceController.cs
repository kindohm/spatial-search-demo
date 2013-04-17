using SpatialDemo.Data;
using SpatialDemo.Models;
using SpatialDemo.ViewModels;
using System.Collections.Generic;
using System.Data.Spatial;
using System.Dynamic;
using System.Linq;
using System.Web.Mvc;

namespace SpatialDemo.Controllers
{
    public class PlaceController : Controller
    {
        private const double MetersToMiles = 1609.34d;

        public ActionResult Search()
        {
            using (var context = new SpatialDemoContext())
            {
                dynamic viewModel = new ExpandoObject();
                viewModel.MaxDistances = GetDistanceSelectList(5000);
                viewModel.NeedsInitialization = context.Places.FirstOrDefault() == null;
                viewModel.PostalCode = string.Empty;
                return View(viewModel);
            }
        }

        public ActionResult List(string postalCode, int maxDistance)
        {
            using (var context = new SpatialDemoContext())
            {
                dynamic viewModel = new ExpandoObject();

                // set up core viewmodel stuff for the view
                viewModel.PostalCode = postalCode;
                viewModel.MaxDistance = maxDistance;
                viewModel.MaxDistances = GetDistanceSelectList(maxDistance);

                // get the origin postal code from the database.
                // we need this to get the lat/long/location of the origin postal code.
                var startPoint = context.PostalCodes
                    .Where(p => p.Code == postalCode)
                    .FirstOrDefault();

                // if no origin postal code was found, bail
                if (startPoint == null)
                {
                    viewModel.NoZipCode = true;
                }
                else
                {
                    var meters = maxDistance * MetersToMiles;
                    viewModel.NoZipCode = false;

                    // do the search for places based on the origin's lat/long location,
                    // and select into a new viewmodel object ordered by distance
                    var results = context.Places
                        .Where(p => p.Location.Distance(startPoint.Location) <= meters)
                        .Select(p => new PlaceResult()
                        {
                            Name = p.Name,
                            Distance = (p.Location.Distance(startPoint.Location) / MetersToMiles).Value
                        })
                        .OrderBy(p => p.Distance)
                        .ToList();

                    viewModel.Results = results;
                }
                return View(viewModel);
            }
        }

        public ActionResult Initialize()
        {
            using (var context = new SpatialDemoContext())
            {
                dynamic viewModel = new ExpandoObject();

                if (context.PostalCodes.FirstOrDefault() != null)
                {
                    viewModel.PostalCodesInserted = 0;
                }
                else
                {
                    // load US postal codes
                    var path = Server.MapPath("~/App_Data/US.txt");
                    var lines = System.IO.File.ReadAllLines(path);

                    foreach (var line in lines)
                    {
                        var parts = line.Split('\t');
                        var postalCode = new PostalCode()
                        {
                            Code = parts[1],
                            Latitude = double.Parse(parts[9]),
                            Longitude = double.Parse(parts[10])
                        };

                        postalCode.Location = DbGeography.FromText(
                            string.Format("POINT({1} {0})", postalCode.Latitude.ToString(), postalCode.Longitude.ToString()));
                        context.PostalCodes.Add(postalCode);
                    }

                    viewModel.PostalCodesInserted = context.SaveChanges();
                }

                if (context.Places.FirstOrDefault() != null)
                {
                    viewModel.PlacesInserted = 0;
                }
                else
                {
                    var path = Server.MapPath("~/App_Data/places.txt");
                    var lines = System.IO.File.ReadAllLines(path);

                    foreach (var line in lines)
                    {
                        var parts = line.Split('\t');
                        var place = new Place()
                        {
                            Name = parts[0],
                            Latitude = double.Parse(parts[1]),
                            Longitude = double.Parse(parts[2])
                        };
                        place.Location = DbGeography.FromText(
                            string.Format("POINT({1} {0})", place.Latitude.ToString(), place.Longitude.ToString()));
                        context.Places.Add(place);
                    }
                    viewModel.PlacesInserted = context.SaveChanges();
                }

                return View(viewModel);
            }
        }

        private SelectList GetDistanceSelectList(int selected)
        {
            var list = new List<int>(){
                5, 100, 250, 500, 1000, 2000, 5000};

            return new SelectList(list, selected);
        }
    }
}