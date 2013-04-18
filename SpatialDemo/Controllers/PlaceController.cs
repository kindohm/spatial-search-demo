using SpatialDemo.Data;
using SpatialDemo.ViewModels;
using System.Collections.Generic;
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
                viewModel.PostalCode = string.Empty;
                viewModel.NeedsInitialization = GetNeedsInitialization(context);
                viewModel.SearchFormTitle = "Search Places by Postal Code";
                return View(viewModel);
            }
        }

        [HttpPost]
        public ActionResult List(string postalCode, int maxDistance)
        {
            using (var context = new SpatialDemoContext())
            {
                dynamic viewModel = new ExpandoObject();

                // set up core viewmodel stuff for the view
                viewModel.NeedsInitialization =
                viewModel.PostalCode = postalCode;
                viewModel.MaxDistance = maxDistance;
                viewModel.MaxDistances = GetDistanceSelectList(maxDistance);
                viewModel.NeedsInitialization = GetNeedsInitialization(context);
                viewModel.SearchFormTitle = "Search Again";

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
                    viewModel.NoZipCode = false;

                    var meters = maxDistance * MetersToMiles;

                    // do the search for places based on the origin's lat/long location,
                    // and select into a new viewmodel object ordered by distance
                    var results = context.Places
                        .Where(p => p.Location.Distance(startPoint.Location) <= meters)
                        .Select(p => new PlaceResult()
                        {
                            Name = p.Name,
                            Distance = (p.Location.Distance(startPoint.Location) / MetersToMiles).Value,
                            Latitude = p.Latitude,
                            Longitude = p.Longitude
                        })
                        .OrderBy(p => p.Distance)
                        .ToList();

                    viewModel.Results = results;
                }
                return View(viewModel);
            }
        }

        private bool GetNeedsInitialization(SpatialDemoContext context)
        {
            return context.Places.FirstOrDefault() == null
                || context.PostalCodes.FirstOrDefault() == null;
        }

        private SelectList GetDistanceSelectList(int selected)
        {
            var list = new List<int>(){
                5, 100, 250, 500, 1000, 2000, 5000};

            return new SelectList(list, selected);
        }
    }
}