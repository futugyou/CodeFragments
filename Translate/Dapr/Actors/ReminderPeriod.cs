using Tools;

namespace Actors;

public class ReminderPeriod
{
    private readonly string value;
    private int years;
    private int months;
    private int days;
    private TimeSpan period;
    private int repeats = -1;

    public ReminderPeriod()
    {

    }

    public ReminderPeriod(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            this.value = value;
            ParseReminderPeriod();
        }
    }

    public ReminderPeriod(string value, int repeats)
    {
        this.value = value;
        this.repeats = repeats;
    }

    public bool HasRepeats()
    {
        return repeats != 0 && (years != 0 || months != 0 || days != 0 || period != TimeSpan.Zero);
    }

    public DateTime GetFollowing(DateTime now)
    {
        DateTime next = now.AddYears(years).AddMonths(months).AddDays(days);
        return next + period;
    }

    public override string ToString()
    {
        return value;
    }

    private void ParseReminderPeriod()
    {
        (years, months, days, period, repeats, Exception? error) = TimeUtil.ParseDuration(value);
        if (error != null)
        {
            throw error;
        }
    }
}