using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CocApi.Cache.Services
{
    public abstract class PerpetualService<T> : PerpetualExecutionBackgroundService<T> where T : class
    {
        private protected int _id = int.MinValue;
        private protected DateTime expires = DateTime.UtcNow.AddSeconds(-3);
        private protected DateTime min = DateTime.MinValue;
        private protected DateTime now = DateTime.UtcNow;


        private protected IServiceScopeFactory ScopeFactory { get; }


        public PerpetualService(
            ILogger<T> logger, 
            IServiceScopeFactory scopeFactory,
            TimeSpan delayBeforeExecution,
            TimeSpan delayBetweenExecutions) : base(logger, delayBeforeExecution, delayBetweenExecutions)
        {
            ScopeFactory = scopeFactory;
        }


        private protected void SetDateVariables()
        {
            expires = DateTime.UtcNow.AddSeconds(-3);

            now = DateTime.UtcNow;
        }
    }
}
