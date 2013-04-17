using System.ComponentModel.DataAnnotations;
using System.Data.Spatial;

namespace SpatialDemo.Models
{
    public class PostalCode
    {
        [Required]
        [Key]
        public int Id { get; set; }

        [Required]
        public string Code { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        [Required]
        public DbGeography Location { get; set; }
    }
}