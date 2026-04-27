using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace OpenHeavensChurch.Models;

[TableName(TableName)]
[PrimaryKey(nameof(Id), AutoIncrement = true)]
[ExplicitColumns]
public class ContactSubmission
{
    public const string TableName = "OHC_ContactSubmissions";

    [Column("id")]
    [PrimaryKeyColumn(AutoIncrement = true, IdentitySeed = 1)]
    public int Id { get; set; }

    [Column("submittedAt")]
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    [Column("name")]
    [Length(120)]
    public string Name { get; set; } = string.Empty;

    [Column("email")]
    [Length(254)]
    public string Email { get; set; } = string.Empty;

    [Column("phone")]
    [Length(40)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Phone { get; set; }

    [Column("topic")]
    [Length(120)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Topic { get; set; }

    [Column("message")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    public string Message { get; set; } = string.Empty;

    [Column("ipAddress")]
    [Length(64)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? IpAddress { get; set; }

    [Column("userAgent")]
    [Length(512)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? UserAgent { get; set; }

    [Column("sourceUrl")]
    [Length(2048)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? SourceUrl { get; set; }

    [Column("status")]
    [Length(20)]
    public string Status { get; set; } = "New"; // New | Read | Archived
}
