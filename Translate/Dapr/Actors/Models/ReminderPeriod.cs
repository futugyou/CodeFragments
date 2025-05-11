using Tools;

namespace Actors.Models;

[JsonConverter(typeof(JsonConverter))]
public class ReminderPeriod
{
    private string value;
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

    public int Repeats
    {
        get => repeats;
        internal set => repeats = value;
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
    public sealed class JsonConverter : JsonConverter<ReminderPeriod>
    {
        public override ReminderPeriod Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var stringValue = reader.GetString() ?? "";
            var reminder = new ReminderPeriod { value = stringValue };
            if (string.IsNullOrEmpty(reminder.value) || reminder.value == "null" || reminder.value == "{}" || reminder.value == "\"\"" || reminder.value == "[]")
            {
                reminder.value = "";
                return reminder;
            }

            if (reminder.value.Length >= 2 && reminder.value[0] == '"' && reminder.value[^1] == '"')
            {
                reminder.value = reminder.value[1..^1];
            }

            reminder.ParseReminderPeriod();
            return reminder;
        }

        public override void Write(Utf8JsonWriter writer, ReminderPeriod value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.value);
        }
    }
}
