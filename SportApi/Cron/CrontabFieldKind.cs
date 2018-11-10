using System;

namespace SportApi.Cron
{
	[Serializable]
	public enum CrontabFieldKind
	{
		Minute,
		Hour,
		Day,
		Month,
		DayOfWeek
	}
}