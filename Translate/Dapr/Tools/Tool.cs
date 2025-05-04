
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Tools;

public static class TimeUtil
{
    public static (int years, int months, int days, TimeSpan duration, int repetition, Exception? error) ParseISO8601Duration(string from)
    {
        // -1 signifies infinite repetition
        int repetition = -1;
        int years = 0, months = 0, days = 0;
        TimeSpan duration = TimeSpan.Zero;
        Exception? error = null;

        // Length must be at least 2 characters per specs
        if (from.Length < 2)
        {
            error = new Exception("Unsupported ISO8601 duration format: " + from);
            return (years, months, days, duration, repetition, error);
        }

        int i = 0;

        // Check if the first character is "R", indicating repetitions
        if (from[0] == 'R')
        {
            // Scan until the "/" character to get the repetitions
            int j = 1;
            while (j < from.Length && from[j] != '/')
            {
                j++;
            }

            if (j - 1 < 1)
            {
                error = new Exception("Unsupported ISO8601 duration format: " + from);
                return (years, months, days, duration, repetition, error);
            }

            if (!int.TryParse(from.AsSpan(1, j - 1), out repetition))
            {
                error = new Exception("Unsupported ISO8601 duration format: " + from);
                return (years, months, days, duration, repetition, error);
            }

            i = j + 1;

            // If we're already at the end of the string after getting repetitions, return
            if (i >= from.Length)
                return (years, months, days, duration, repetition, error);
        }

        // First character must be a "P"
        if (from[i] != 'P')
        {
            error = new Exception("Unsupported ISO8601 duration format: " + from);
            return (years, months, days, duration, repetition, error);
        }

        i++;
        bool isParsingTime = false;
        int start = i;

        while (i < from.Length)
        {
            int tmp;
            switch (from[i])
            {
                case 'T':
                    if (start == i)
                    {
                        error = new Exception("Unsupported ISO8601 duration format: " + from);
                        return (years, months, days, duration, repetition, error);
                    }

                    isParsingTime = true;
                    start = i + 1;
                    break;

                case 'Y':
                    if (isParsingTime || start == i)
                    {
                        error = new Exception("Unsupported ISO8601 duration format: " + from);
                        return (years, months, days, duration, repetition, error);
                    }

                    if (!int.TryParse(from.AsSpan(start, i - start), out years))
                    {
                        error = new Exception("Unsupported ISO8601 duration format: " + from);
                        return (years, months, days, duration, repetition, error);
                    }

                    start = i + 1;
                    break;

                case 'W':
                    if (isParsingTime || start == i)
                    {
                        error = new Exception("Unsupported ISO8601 duration format: " + from);
                        return (years, months, days, duration, repetition, error);
                    }

                    if (!int.TryParse(from.AsSpan(start, i - start), out tmp))
                    {
                        error = new Exception("Unsupported ISO8601 duration format: " + from);
                        return (years, months, days, duration, repetition, error);
                    }

                    days += tmp * 7;
                    start = i + 1;
                    break;

                case 'D':
                    if (isParsingTime || start == i)
                    {
                        error = new Exception("Unsupported ISO8601 duration format: " + from);
                        return (years, months, days, duration, repetition, error);
                    }

                    if (!int.TryParse(from.AsSpan(start, i - start), out tmp))
                    {
                        error = new Exception("Unsupported ISO8601 duration format: " + from);
                        return (years, months, days, duration, repetition, error);
                    }

                    days += tmp;
                    start = i + 1;
                    break;

                case 'H':
                    if (!isParsingTime || start == i)
                    {
                        error = new Exception("Unsupported ISO8601 duration format: " + from);
                        return (years, months, days, duration, repetition, error);
                    }

                    if (!int.TryParse(from.AsSpan(start, i - start), out tmp))
                    {
                        error = new Exception("Unsupported ISO8601 duration format: " + from);
                        return (years, months, days, duration, repetition, error);
                    }

                    duration += TimeSpan.FromHours(tmp);
                    start = i + 1;
                    break;

                case 'M':
                    if (start == i)
                    {
                        error = new Exception("Unsupported ISO8601 duration format: " + from);
                        return (years, months, days, duration, repetition, error);
                    }

                    if (!int.TryParse(from.AsSpan(start, i - start), out tmp))
                    {
                        error = new Exception("Unsupported ISO8601 duration format: " + from);
                        return (years, months, days, duration, repetition, error);
                    }

                    if (isParsingTime)
                    {
                        duration += TimeSpan.FromMinutes(tmp);
                    }
                    else
                    {
                        months = tmp;
                    }

                    start = i + 1;
                    break;

                case 'S':
                    if (!isParsingTime || start == i)
                    {
                        error = new Exception("Unsupported ISO8601 duration format: " + from);
                        return (years, months, days, duration, repetition, error);
                    }

                    if (!int.TryParse(from.AsSpan(start, i - start), out tmp))
                    {
                        error = new Exception("Unsupported ISO8601 duration format: " + from);
                        return (years, months, days, duration, repetition, error);
                    }

                    duration += TimeSpan.FromSeconds(tmp);
                    start = i + 1;
                    break;
            }

            i++;
        }

        return (years, months, days, duration, repetition, error);
    }

    public static (int years, int months, int days, TimeSpan duration, int repetition, Exception? error) ParseDuration(string from)
    {
        var (y, m, d, dur, r, err) = ParseISO8601Duration(from);
        if (err == null)
        {
            return (y, m, d, dur, r, err);
        }

        if (TimeSpan.TryParse(from, out var duration))
        {
            return (0, 0, 0, duration, -1, null);
        }

        return (0, 0, 0, TimeSpan.Zero, 0, new Exception("Unsupported duration format: " + from));
    }

    public static (DateTime startTime, Exception? error) ParseTime(string from, DateTime? offset)
    {
        DateTime start = offset ?? DateTime.Now;

        var (y, m, d, dur, r, err) = ParseISO8601Duration(from);
        if (err == null)
        {
            if (r != -1)
            {
                return (DateTime.MinValue, new Exception("Repetitions are not allowed"));
            }

            return (start.AddYears(y).AddMonths(m).AddDays(d).Add(dur), null);
        }

        if (TimeSpan.TryParse(from, out var duration))
        {
            return (start.Add(duration), null);
        }

        if (DateTime.TryParseExact(from, "yyyy-MM-ddTHH:mm:ssZ", null, DateTimeStyles.AssumeUniversal, out var parsedTime))
        {
            return (parsedTime, null);
        }

        return (DateTime.MinValue, new Exception("Unsupported time/duration format: " + from));
    }

    public static DateTime ParseTimeTruncateSeconds(string val, DateTime now)
    {
        var (t, err) = ParseTime(val, now);
        if (err != null)
        {
            throw err;
        }

        return t.AddTicks(-(t.Ticks % TimeSpan.TicksPerSecond));
    }
    
    public static DateTime Truncate(DateTime now)
    { 
        return now.AddTicks(-(now.Ticks % TimeSpan.TicksPerSecond));
    }
}
