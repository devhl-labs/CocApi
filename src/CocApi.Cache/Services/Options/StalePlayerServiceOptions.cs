using System;
using ScheduledServices.Services.Options;

namespace CocApi.Cache.Services.Options;

public class StalePlayerServiceOptions : RecurringServiceOptions
{
	public StalePlayerServiceOptions()
	{
		DelayBeforeExecution = TimeSpan.FromMinutes(20);
		DelayBetweenExecutions = TimeSpan.FromMinutes(20);
		Enabled = true;
	}
}
