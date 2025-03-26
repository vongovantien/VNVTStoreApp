using Newtonsoft.Json;
using System;

namespace vnvt_back_end.Core.General
{
    internal class FormatConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(int);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (objectType == typeof(int))
            {
                return reader.Value == null ? 0 : Convert.ToInt32(reader.Value.ToString().Replace(",", string.Empty));
            }

            return reader.Value;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}