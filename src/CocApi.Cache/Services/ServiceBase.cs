using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ScheduledServices;

namespace CocApi.Cache.Services;

public abstract class ServiceBase : RecurringService
{
    private protected int _id = int.MinValue;
    private protected DateTime expires = DateTime.UtcNow.AddSeconds(-3);
    private protected DateTime min = DateTime.MinValue;
    private protected DateTime now = DateTime.UtcNow;


    private protected IServiceScopeFactory ScopeFactory { get; }


    public ServiceBase(
        ILogger logger,
        IServiceScopeFactory scopeFactory,
        IOptions<IRecurringServiceOptions> options
        ) : base(logger, options)
    {
        ScopeFactory = scopeFactory;
    }


    private protected void SetDateVariables()
    {
        expires = DateTime.UtcNow.AddSeconds(-3);

        now = DateTime.UtcNow;
    }
}
