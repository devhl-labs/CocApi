using System;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Extensions.Logging;

namespace devhl.CocApi
{
    public abstract class SwallowDelegates
    {
        [NotMapped]
        public ILogger? Logger { get; set; }

        /// <summary>
        /// Wraps an action in a try catch
        /// </summary>
        /// <param name="action"></param>
        public void Swallow(Action action, string methodName)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                if (Logger == null)
                {
                    Console.WriteLine($"{methodName} {e.Message}");
                }
                else
                {
                    Logger?.LogWarning(LoggingEvents.SwallowedError, "{methodName} {exception}", methodName, e.Message);
                }

            }
        }

        /// <summary>
        /// Wraps a task in a try catch
        /// </summary>
        /// <param name="task"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public async Task<Task> SwallowAsync(Task task, string methodName)
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
                    Console.WriteLine($"{e.Message}");
                }
                else
                {
                    Logger.LogWarning(LoggingEvents.SwallowedError, e, methodName);
                }

                return Task.CompletedTask;
            }
        }
    }
}
