﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Text.RegularExpressions;

namespace Microsoft.Health.Fhir.Liquid.Converter.Models
{
    public class DateTimeObject
    {
        public DateTimeObject(string input, Regex regex)
        {
            var matches = regex.Matches(input);
            if (matches.Count != 1 || !matches[0].Groups["year"].Success)
            {
                throw new ArgumentException(string.Format(Resources.InvalidDateTimeFormat, input));
            }

            var groups = matches[0].Groups;

            int year = int.Parse(groups["year"].Value);
            int month = groups["month"].Success ? int.Parse(groups["month"].Value) : 1;
            int day = groups["day"].Success ? int.Parse(groups["day"].Value) : 1;
            int hour = groups["hour"].Success ? int.Parse(groups["hour"].Value) : 0;
            int minute = groups["minute"].Success ? int.Parse(groups["minute"].Value) : 0;
            int second = groups["second"].Success ? int.Parse(groups["second"].Value) : 0;
            int millisecond = groups["millisecond"].Success ? int.Parse(groups["millisecond"].Value) : 0;

            var timeSpan = TimeSpan.FromHours(TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).Hours);
            if (groups["timeZone"].Success)
            {
                if (groups["timeZone"].Value == "Z")
                {
                    timeSpan = TimeSpan.Zero;
                }
                else
                {
                    var sign = groups["sign"].Success && groups["sign"].Value == "-" ? -1 : 1;
                    var timeZoneHour = int.Parse(groups["timeZoneHour"].Value) * sign;
                    var timeZoneMinute = int.Parse(groups["timeZoneMinute"].Value) * sign;
                    timeSpan = new TimeSpan(timeZoneHour, timeZoneMinute, 0);
                }
            }

            DateValue = new DateTimeOffset(year, month, day, hour, minute, second, millisecond, timeSpan);
            HasDay = groups["day"].Success;
            HasMonth = groups["month"].Success;
            HasYear = groups["year"].Success;
            HasTime = groups["time"].Success;
            HasHour = groups["hour"].Success;
            HasMinute = groups["minute"].Success;
            HasSecond = groups["second"].Success;
            HasMilliSecond = groups["millisecond"].Success;
            HasTimeZone = groups["timeZone"].Success;
        }

        public DateTimeOffset DateValue { get; set; }

        public bool HasTimeZone { get; set; } = true;

        public bool HasTime { get; set; } = true;

        public bool HasMilliSecond { get; set; } = true;

        public bool HasSecond { get; set; } = true;

        public bool HasMinute { get; set; } = true;

        public bool HasHour { get; set; } = true;

        public bool HasDay { get; set; } = true;

        public bool HasMonth { get; set; } = true;

        public bool HasYear { get; set; } = true;

        public void ConvertToDate()
        {
            HasTime = false;
            HasHour = false;
            HasMinute = false;
            HasSecond = false;
            HasTimeZone = false;
            HasMilliSecond = false;
        }

        public void AddSeconds(double seconds)
        {
            DateValue = DateValue.AddSeconds(seconds);
            HasYear = true;
            HasMonth = true;
            HasDay = true;
            HasTime = true;
            HasHour = true;
            HasMinute = true;
            HasSecond = true;
        }

        public string ToFhirString(string timeZoneHandling)
        {
            var resultDateTime = timeZoneHandling?.ToLower() switch
            {
                "preserve" => DateValue,
                "utc" => DateValue.ToUniversalTime(),
                "local" => DateValue.ToLocalTime(),
                _ => throw new ArgumentException(Resources.InvalidTimeZoneHandling),
            };

            if (!HasTime)
            {
                return HasYear switch
                {
                    true when HasMonth && HasDay => resultDateTime.ToString("yyyy-MM-dd"),
                    true when HasMonth => resultDateTime.ToString("yyyy-MM"),
                    true => resultDateTime.ToString("yyyy"),
                    _ => throw new ArgumentException("Invalid dateTimeObject with empty Year field.")
                };
            }

            var timeZoneSuffix = string.Empty;
            if (HasTimeZone || string.Equals(timeZoneHandling.ToLower(), "utc"))
            {
                // Using "Z" to represent zero timezone.
                timeZoneSuffix = resultDateTime.Offset == TimeSpan.Zero ? "Z" : "%K";
            }

            var dateTimeFormat = !HasMilliSecond ? "yyyy-MM-ddTHH:mm:ss" + timeZoneSuffix : "yyyy-MM-ddTHH:mm:ss.fff" + timeZoneSuffix;
            return resultDateTime.ToString(dateTimeFormat);
        }
    }
}
