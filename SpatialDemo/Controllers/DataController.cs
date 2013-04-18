using SpatialDemo.Data;
using SpatialDemo.Models;
using System.Data.Spatial;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Web.Mvc;

namespace SpatialDemo.Controllers
{
    public class DataController : Controller
    {
        private const string UnitedStatesDataFile = "~/App_Data/US.txt";
        private const string PlacesDataFile = "~/App_Data/places.txt";
        private const char Tab = '\t';
        private const string PointStringFormat = "POINT({1} {0})";

        public ActionResult Load()
        {
            // This is just a big parsing of two flat, tab-delimited files.
            // It's just to get data into the database. Nothing fancy, but
            // take note of the use of DbGeography and the POINT string to
            // insert spatial geography types into the database.

            using (var context = new SpatialDemoContext())
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                dynamic viewModel = new ExpandoObject();

                if (context.PostalCodes.FirstOrDefault() != null)
                {
                    viewModel.PostalCodesInserted = 0;
                }
                else
                {
                    // load US postal codes
                    var path = Server.MapPath(UnitedStatesDataFile);
                    var lines = System.IO.File.ReadAllLines(path);

                    foreach (var line in lines)
                    {
                        var parts = line.Split(Tab);
                        var postalCode = new PostalCode()
                        {
                            Code = parts[1],
                            Latitude = double.Parse(parts[9]),
                            Longitude = double.Parse(parts[10])
                        };

                        // Here is where we set the SQL Geography spatial field
                        // on the Postal Code.
                        postalCode.Location = DbGeography.FromText(
                            string.Format(GetSqlPointString(postalCode.Latitude, postalCode.Longitude)));
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
                    // load places
                    var path = Server.MapPath(PlacesDataFile);
                    var lines = System.IO.File.ReadAllLines(path);

                    foreach (var line in lines)
                    {
                        var parts = line.Split(Tab);
                        var place = new Place()
                        {
                            Name = parts[0],
                            Latitude = double.Parse(parts[1]),
                            Longitude = double.Parse(parts[2])
                        };

                        // set the SQL Geography spatial field on the Place.
                        place.Location = DbGeography.FromText(
                            GetSqlPointString(place.Latitude, place.Longitude));
                        context.Places.Add(place);
                    }
                    viewModel.PlacesInserted = context.SaveChanges();
                }

                stopwatch.Stop();
                viewModel.Elapsed = stopwatch.Elapsed.TotalSeconds;
                return View(viewModel);
            }
        }

        private string GetSqlPointString(double latitude, double longitude)
        {
            return string.Format(PointStringFormat, latitude.ToString(), longitude.ToString());
        }
    }
}