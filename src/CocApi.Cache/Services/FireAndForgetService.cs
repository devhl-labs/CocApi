using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CocApi.Cache.Services;

public sealed class FireAndForgetService : BackgroundService
{
    private readonly ILogger<FireAndForgetService> _logger;
    private readonly Channel<Func<Task>> _channel = Channel.CreateUnbounded<Func<Task>>();
    private readonly SemaphoreSlim _limit = new(25);

    public FireAndForgetService(ILogger<FireAndForgetService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        List<Task> inflight = new();

        await foreach (Func<Task> factory in _channel.Reader.ReadAllAsync(cancellationToken))
            inflight.Add(RunWithLimitAsync(factory));

        await Task.WhenAll(inflight).ConfigureAwait(false);
    }

    private async Task RunWithLimitAsync(Func<Task> factory)
    {
        await _limit.WaitAsync();
        try { await SafeRunAsync(factory()).ConfigureAwait(false); }
        finally { _limit.Release(); }
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

    public void Append(Func<Task> taskFactory) => _channel.Writer.TryWrite(taskFactory);

    public void Append(Task task) => _channel.Writer.TryWrite(() => task);
}
