using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ScheduledServices;
using ScheduledServices.Services.Options;

namespace CocApi.Cache.Services;

public sealed class FireAndForgetService : RecurringService
{
    private readonly ILogger<FireAndForgetService> _logger;
    private readonly ConcurrentQueue<Task> _tasks = new();

    public FireAndForgetService(ILogger<FireAndForgetService> logger)
        : base(logger, Microsoft.Extensions.Options.Options.Create(new RecurringServiceOptions
        {
            DelayBeforeExecution = TimeSpan.FromSeconds(1),
            DelayBetweenExecutions = TimeSpan.FromMilliseconds(250),
            Enabled = true
        }))
    {
        _logger = logger;
    }

    protected override async Task ExecuteScheduledTaskAsync(CancellationToken cancellationToken)
    {
        List<Task> batch = new();

        while (_tasks.TryDequeue(out Task? task))
            batch.Add(task);

        if (batch.Count == 0)
            return;

        // Process in chunks to avoid saturating the thread pool when many events fire at once.
        const int chunkSize = 25;
        for (int i = 0; i < batch.Count; i += chunkSize)
            await Task.WhenAll(batch.Skip(i).Take(chunkSize).Select(SafeRunAsync)).ConfigureAwait(false);
    }

    private async Task SafeRunAsync(Task task)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{exceptionMessage}", e.Message);
        }
    }

    public void Append(Task task) => _tasks.Enqueue(task);
}
