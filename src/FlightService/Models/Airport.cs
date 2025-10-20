using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FlightService.Models;

public class Airport
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string City { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Country { get; set; } = string.Empty;

    // Навигационные свойства
    [JsonIgnore]
    public virtual ICollection<Flight> DepartureFlights { get; set; }

    [JsonIgnore]
    public virtual ICollection<Flight> ArrivalFlights { get; set; }
}