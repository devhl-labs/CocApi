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
    private readonly ConcurrentQueue<Func<Task>> _tasks = new();

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
        List<Func<Task>> batch = new();

        while (_tasks.TryDequeue(out Func<Task>? factory))
            batch.Add(factory);

        if (batch.Count == 0)
            return;

        // Start tasks lazily in chunks to limit concurrent Discord API calls.
        const int chunkSize = 25;
        for (int i = 0; i < batch.Count; i += chunkSize)
            await Task.WhenAll(batch.Skip(i).Take(chunkSize).Select(f => SafeRunAsync(f()))).ConfigureAwait(false);
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

    public void Append(Func<Task> taskFactory) => _tasks.Enqueue(taskFactory);
}
