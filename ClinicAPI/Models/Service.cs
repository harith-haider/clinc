using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicAPI.Models
{
    public class Service
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(150)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public int DurationMinutes { get; set; }

        [Required]
        public decimal Price { get; set; }
    }
}