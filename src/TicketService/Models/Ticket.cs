using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketService.Models;

public class Ticket
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public Guid TicketUid { get; set; }

    [Required]
    [MaxLength(80)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string FlightNumber { get; set; } = string.Empty;

    [Required]
    public int Price { get; set; }

    [Required]
    [MaxLength(20)]
    public TicketStatus Status { get; set; }
}

public enum TicketStatus
{
    PAID,
    CANCELED
}