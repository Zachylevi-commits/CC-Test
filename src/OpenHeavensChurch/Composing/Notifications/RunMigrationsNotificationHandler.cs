using Microsoft.Extensions.Logging;
using OpenHeavensChurch.Composing.Migrations;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;

namespace OpenHeavensChurch.Composing.Notifications;

public class RunMigrationsNotificationHandler : INotificationHandler<UmbracoApplicationStartedNotification>
{
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IMigrationPlanExecutor _executor;
    private readonly IKeyValueService _keyValueService;
    private readonly IRuntimeState _runtimeState;
    private readonly ILogger<RunMigrationsNotificationHandler> _logger;

    public RunMigrationsNotificationHandler(
        ICoreScopeProvider scopeProvider,
        IMigrationPlanExecutor executor,
        IKeyValueService keyValueService,
        IRuntimeState runtimeState,
        ILogger<RunMigrationsNotificationHandler> logger)
    {
        _scopeProvider = scopeProvider;
        _executor = executor;
        _keyValueService = keyValueService;
        _runtimeState = runtimeState;
        _logger = logger;
    }

    public void Handle(UmbracoApplicationStartedNotification notification)
    {
        if (_runtimeState.Level != RuntimeLevel.Run)
        {
            return;
        }

        try
        {
            var upgrader = new Upgrader(new OpenHeavensChurchMigrationPlan());
            upgrader.Execute(_executor, _scopeProvider, _keyValueService);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run OpenHeavensChurch migrations");
        }
    }
}
