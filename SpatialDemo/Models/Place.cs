using System.ComponentModel.DataAnnotations;
using System.Data.Spatial;

namespace SpatialDemo.Models
{
    public class Place
    {
        [Required]
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        [Required]
        public DbGeography Location { get; set; }
    }
}