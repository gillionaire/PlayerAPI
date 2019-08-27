using System.ComponentModel.DataAnnotations;

namespace PlayerAPI2.Models
{
    public class Location
    {
        [Key]
        public int Id { get; set; }
        public string City { get; set; }
    }
}