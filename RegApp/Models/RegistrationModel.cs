using System.ComponentModel.DataAnnotations;

namespace RegApp.Models
{
    public class RegistrationModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Range(0, 15)]
        public int FridayAdults { get; set; }

        [Required]
        [Range(0, 15, ErrorMessage = "Please enter a valid number!")]
        public int FridayChildren { get; set; }

        [Required]
        [Range(1, 15, ErrorMessage = "Please enter a valid number!")]
        public int Vehicles { get; set; }

        [Required]
        [Range(0, 15, ErrorMessage = "Please enter a valid number!")]
        public int SaturdayAdults { get; set; }

        [Required]
        [Range(0, 15, ErrorMessage = "Please enter a valid number!")]
        public int SaturdayChildren { get; set; }
    }
}