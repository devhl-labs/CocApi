using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CocApiLibrary
{
    public abstract class CommonMethods
    {
        public ILogger? Logger { get; set; }

        /// <summary>
        /// Wraps an action in a try catch
        /// </summary>
        /// <param name="action"></param>
        /// <param name="logger"></param>
        public void Swallow(Action action, ILogger? logger = null)
        {
            try
            {
                logger ??= Logger;

                action();
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, $"Error was swallowed: {ex.Message}");
            }
        }

        /// <summary>
        /// Wraps a task in a try catch
        /// </summary>
        /// <param name="task"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task<Task> SwallowAsync(Task task, string message, params object[] args)
        {
            try
            {
                await task;

                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                if (Logger == null)
                {
                    Console.WriteLine($"The ILogger is null.  Exception was swallowed: {e.Message}");
                }
                else
                {
                    Logger.LogWarning(LoggingEvents.SwallowedError, e, message, args);
                }

                return Task.CompletedTask;
            }
        }
    }
}
