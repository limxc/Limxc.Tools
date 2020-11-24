﻿using Limxc.Tools.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Limxc.Tools.Utils
{
    public class UnixTimestampConverter : DateTimeConverterBase
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteRawValue(UnixTimestampFromDateTime((DateTime)value).ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value == null ? new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc) : TimeFromUnixTimestamp((long)reader.Value);
        }

        private static DateTime TimeFromUnixTimestamp(long unixTimestamp) => unixTimestamp.ToDateTime();

        public static long UnixTimestampFromDateTime(DateTime date) => date.ToTimeStamp();
    }
}