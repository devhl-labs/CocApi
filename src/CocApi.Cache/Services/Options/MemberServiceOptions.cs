using System;
using ScheduledServices.Services.Options;

namespace CocApi.Cache.Services.Options;

public class MemberServiceOptions : RecurringServiceOptions
{
	public MemberServiceOptions()
	{
		DelayBeforeExecution = TimeSpan.FromSeconds(5);
		DelayBetweenExecutions = TimeSpan.FromSeconds(5);
		Enabled = true;
	}
}
