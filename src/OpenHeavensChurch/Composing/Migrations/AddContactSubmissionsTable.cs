using Microsoft.Extensions.Logging;
using OpenHeavensChurch.Models;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OpenHeavensChurch.Composing.Migrations;

public class AddContactSubmissionsTable : MigrationBase
{
    public AddContactSubmissionsTable(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate()
    {
        Logger.LogInformation("Running migration {Migration}", nameof(AddContactSubmissionsTable));

        if (TableExists(ContactSubmission.TableName))
        {
            Logger.LogInformation("{Table} already exists, skipping create", ContactSubmission.TableName);
            return;
        }

        Create.Table<ContactSubmission>().Do();
        Create.Index($"IX_{ContactSubmission.TableName}_SubmittedAt")
            .OnTable(ContactSubmission.TableName)
            .OnColumn(nameof(ContactSubmission.SubmittedAt).ToLowerFirstChar())
            .Ascending()
            .WithOptions().NonClustered()
            .Do();
    }
}

internal static class StringExt
{
    public static string ToLowerFirstChar(this string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToLowerInvariant(s[0]) + s.Substring(1);
}
