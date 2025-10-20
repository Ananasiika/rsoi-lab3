using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BonusService.Models;

public class Privilege
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(80)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string Status { get; set; } = "BRONZE";

    public int Balance { get; set; } = 0;

    public virtual ICollection<PrivilegeHistory> History { get; set; } = new List<PrivilegeHistory>();
}

public static class PrivilegeStatus
{
    public const string BRONZE = "BRONZE";
    public const string SILVER = "SILVER";
    public const string GOLD = "GOLD";
}