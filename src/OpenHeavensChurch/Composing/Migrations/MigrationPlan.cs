using Umbraco.Cms.Infrastructure.Migrations;

namespace OpenHeavensChurch.Composing.Migrations;

public class OpenHeavensChurchMigrationPlan : MigrationPlan
{
    public OpenHeavensChurchMigrationPlan() : base("OpenHeavensChurch")
    {
        From(string.Empty)
            .To<AddContactSubmissionsTable>("contact-submissions-v1");
    }
}
