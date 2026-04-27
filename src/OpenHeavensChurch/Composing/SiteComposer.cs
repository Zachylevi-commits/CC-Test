using OpenHeavensChurch.Composing.Notifications;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;

namespace OpenHeavensChurch.Composing;

public class SiteComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<UmbracoApplicationStartedNotification, RunMigrationsNotificationHandler>();
    }
}
