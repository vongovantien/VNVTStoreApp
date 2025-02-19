using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FW.WAPI.Core.General
{
    public static class JsonUtilities
    {
        //public static object SerializeObjectWithFields<T>(dynamic objectToSeriralize, dynamic fields)
        //{
        //    var serializeSettings = new JsonSerializerSettings();
        //    serializeSettings.ContractResolver = new ShouldSerializeFieldContract<T>(fields);

        //    var objectSerialized = JsonConvert.SerializeObject(objectToSeriralize, Formatting.None, serializeSettings);
        //    var result = JsonConvert.DeserializeObject<object>(objectSerialized);

        //    return result;
        //}

        //public static object SerializeObjectWithFields<T>(object objectToSeriralize, List<string> fields)

        //{
        //    var serializeSettings = new JsonSerializerSettings();
        //    serializeSettings.ContractResolver = new ShouldSerializeFieldContract<T>(fields);

        //    var objectSerialized = JsonConvert.SerializeObject(objectToSeriralize, Formatting.None, serializeSettings);
        //    var result = JsonConvert.DeserializeObject<object>(objectSerialized);

        //    return result;
        //}

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool CheckJsonArray(dynamic value)
        {
            return value is JArray;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="jArray"></param>
        /// <returns></returns>
        public static string[] GetValuesOfJArray(JArray jArray)
        {
            var vals = jArray.Where(x => x.Type == JTokenType.String);
            var result = vals.Select(x => x.ToString()).ToArray();

            return result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ConvertObjectToJson(object obj)
        {
            var options = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };

            return JsonConvert.SerializeObject(obj, options);
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T ConvertJsonToObject<T>(string json)
        {
            var jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
            jsonSerializerSettings.NullValueHandling = NullValueHandling.Ignore;

            jsonSerializerSettings.Converters.Add(new FormatConverter());
            return JsonConvert.DeserializeObject<T>(json, jsonSerializerSettings);
        }

        /// <summary>
        /// Convert Arguments To Json
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static string ConvertArgumentsToJson(IDictionary<string, object> arguments)
        {
            try
            {
                if (arguments.IsNullOrEmpty())
                {
                    return "{}";
                }

                var dictionary = new Dictionary<string, object>();

                foreach (var argument in arguments)
                {
                    if (argument.Value == null)
                    {
                        dictionary[argument.Key] = null;
                    }
                    else
                    {
                        dictionary[argument.Key] = argument.Value;
                    }
                }

                return ConvertObjectToJson(dictionary);
            }
            catch
            {
                return "{}";
            }
        }

        /// <summary>
        /// Remove sensitive value
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string ReplaceSensitiveValue(string parameters)
        {
            try
            {
                if (parameters == null) return parameters;

                var auditLogObj = JObject.Parse(parameters);
                foreach (var field in AuditLogConst.SENSITIVE_FIELDS)
                {
                    IEnumerable<JToken>[] sensitiveFields = new IEnumerable<JToken>[]
                    {
                        auditLogObj.SelectTokens($"$.{field}"),
                        auditLogObj.SelectTokens($"$..{field}"),
                        auditLogObj.SelectTokens($"$...{field}")
                    };

                    foreach (IEnumerable<JToken> sensitiveField in sensitiveFields)
                    {
                        if (sensitiveField?.Any() != true) continue;
                        foreach (JToken item in sensitiveField)
                        {
                            var value = item.Value<string>();
                            if (value == null) continue;
                            var hiddenValue = new string(AuditLogConst.SENSITIVE_VALUE_ALTERNATIVE,
                                AuditLogConst.SENSITIVE_VALUE_ALTERNATIVE_LENGTH);
                            item.Replace(hiddenValue);
                        }
                    }
                }

                return auditLogObj.ToString(Formatting.None);
            }
            catch (JsonReaderException)
            {
                return parameters;
            }
            catch
            {
                return "{}";
            }
        }
    }
}