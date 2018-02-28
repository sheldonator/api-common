using System;
using System.Collections.Generic;
using System.Linq;
using ApiExtensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiMiddleware
{
    public class StringObfuscation
    {
        private const string JsonContentType = "application/json";
        private const string FormContentType = "application/x-www-form-urlencoded";

        private static readonly List<string> CommonPropertiesToRemove = new List<string>
        {
            "Password",
            "Answer",
            "QuestionAnswer",
            "SecurityAnswer",
            "AnswerCharacters",
            "Character1",
            "Character2",
            "Character3",
            "ReTypePassword",
            "LoyaltyCardNumber",
            "OldPassword",
            "NewPassword"
        };

        public static string RemovePropertyValues(string requestBody,
            string contentType,
            IEnumerable<string> propertiesToRemove = null)
        {
            if (requestBody == null)
                return requestBody;

            contentType = contentType?.StripWhiteSpace().Split(';')[0].ToLower();
            var propertyFieldNames = CommonPropertiesToRemove.Union(propertiesToRemove ?? new List<string>()).ToList();

            // return 
            var requestContainsField = propertyFieldNames.Any(n => requestBody.IndexOf(n, StringComparison.OrdinalIgnoreCase) >= 0);

            if (!string.IsNullOrEmpty(requestBody) && requestContainsField)
            {
                switch (contentType)
                {
                    case JsonContentType:
                        requestBody = ObfuscateJsonRequest(requestBody, propertyFieldNames);
                        break;
                    case FormContentType:
                        requestBody = ObfuscateFormRequest(requestBody, propertyFieldNames);
                        break;
                }
            }

            return requestBody;
        }
        private static string ObfuscateJsonRequest(string requestBody, List<string> propertyFieldNames)
        {
            dynamic obj = JsonConvert.DeserializeObject(requestBody);
            JToken jToken = obj as JToken;
            RemoveJsonPropertyValueRecursive(jToken, propertyFieldNames);
            requestBody = JsonConvert.SerializeObject(jToken, Formatting.Indented);
            return requestBody;
        }

        private static string ObfuscateFormRequest(string requestBody, List<string> propertyFieldNames)
        {
            var formDictionary = QueryHelpers.ParseQuery(requestBody);
            var modifiedDictionary = RemoveFormPropertyValuerecursive(formDictionary, propertyFieldNames);
            requestBody = QueryHelpers.AddQueryString("", modifiedDictionary);
            return requestBody;
        }

        private static Dictionary<string,string> RemoveFormPropertyValuerecursive(Dictionary<string, StringValues> formDictionary,
            List<string> propertyFieldNames)
        {
            var returnDictionary = new Dictionary<string, string>();

            foreach (KeyValuePair<string, StringValues> pair in formDictionary)
            {
                if (propertyFieldNames.Any(n => string.Equals(n, pair.Key, StringComparison.CurrentCultureIgnoreCase)))
                {
                    returnDictionary.Add(pair.Key, !string.IsNullOrEmpty(pair.Value[0]) ? "[NOT LOGGED]" : string.Empty);
                }
                else
                {
                    returnDictionary.Add(pair.Key, pair.Value);
                }
            }

            return returnDictionary;
        }

        private static void RemoveJsonPropertyValueRecursive(JToken jToken, List<string> propertyFieldNames)
        {
            if (jToken.HasValues)
            {
                foreach (JToken token in jToken.Children())
                {
                    if (token is JProperty)
                    {
                        var prop = token as JProperty;

                        if (propertyFieldNames.Any(n => string.Equals(n, prop.Name, StringComparison.CurrentCultureIgnoreCase)))
                        {
                            if (!string.IsNullOrEmpty(prop.Value?.ToString()))
                                prop.Value = "[NOT LOGGED]";
                        }

                        RemoveJsonPropertyValueRecursive(token, propertyFieldNames);
                    }

                    if (token is JObject)
                    {
                        RemoveJsonPropertyValueRecursive(token, propertyFieldNames);
                    }
                }
            }
        }
    }
}