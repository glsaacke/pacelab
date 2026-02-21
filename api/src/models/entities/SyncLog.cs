using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.src.models.entities;

[Table("Sync_Logs")]
public class SyncLog
{
    [Key]
    [Column("sync_log_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SyncLogId { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("sync_type")]
    public string? SyncType { get; set; }

    [Column("status")]
    public string? Status { get; set; }

    [Column("activities_synced")]
    public int? ActivitiesSynced { get; set; }

    [Column("error_message")]
    public string? ErrorMessage { get; set; }

    [Column("started_at")]
    public DateTime? StartedAt { get; set; }

    [Column("completed_at")]
    public DateTime? CompletedAt { get; set; }
}
