using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiExtensions
{
    public static class ObjectExtensions
    {
        public static string ToQueryString(this object obj)
        {
            var propertyValues = obj.ToPropertyValueDictionary();
            var builder = new QueryBuilder();

            foreach (var prop in propertyValues.Keys)
            {
                if (propertyValues[prop] != null)
                {
                    builder.Add(prop,
                        propertyValues[prop].GetType().Name == typeof(DateTime).Name
                            ? ((DateTime) propertyValues[prop]).ToString("u")
                            : propertyValues[prop].ToString());
                }
            }

            return builder.ToQueryString().Value;
        }

        public static Dictionary<string, object> ToPropertyValueDictionary(this object obj)
        {
            var objString = JsonConvert.SerializeObject(obj);
            if (JsonConvert.DeserializeObject(objString) is JObject attributesAsJObject)
                return attributesAsJObject.ToObject<Dictionary<string, object>>();

            return new Dictionary<string, object>();
        }
    }
}
