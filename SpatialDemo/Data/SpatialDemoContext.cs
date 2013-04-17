using SpatialDemo.Models;
using System.Data.Entity;

namespace SpatialDemo.Data
{
    public class SpatialDemoContext : DbContext
    {
        public SpatialDemoContext()
            : base("SpatialDemo")
        {
            this.Configuration.AutoDetectChangesEnabled = false;
        }

        public DbSet<Place> Places { get; set; }

        public DbSet<PostalCode> PostalCodes { get; set; }
    }
}