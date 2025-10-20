using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlightService.Models;

public class Flight
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string FlightNumber { get; set; } = string.Empty;

    [Required]
    public DateTime DateTime { get; set; }

    [Required]
    public int FromAirportId { get; set; }

    [Required]
    public int ToAirportId { get; set; }

    [Required]
    public int Price { get; set; }

    // Навигационные свойства
    [ForeignKey("FromAirportId")]
    public virtual Airport FromAirport { get; set; } = null!;

    [ForeignKey("ToAirportId")]
    public virtual Airport ToAirport { get; set; } = null!;
}