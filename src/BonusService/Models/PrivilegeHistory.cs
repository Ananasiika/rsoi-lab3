using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BonusService.Models;

public class PrivilegeHistory
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int PrivilegeId { get; set; }

    [Required]
    public Guid TicketUid { get; set; }

    [Required]
    public DateTime Datetime { get; set; }

    [Required]
    public int BalanceDiff { get; set; }

    [Required]
    [MaxLength(20)]
    public string OperationType { get; set; } = string.Empty;

    [ForeignKey("PrivilegeId")]
    public virtual Privilege Privilege { get; set; } = null!;
}

public static class OperationType
{
    public const string FILL_IN_BALANCE = "FILL_IN_BALANCE";
    public const string DEBIT_THE_ACCOUNT = "DEBIT_THE_ACCOUNT";
}