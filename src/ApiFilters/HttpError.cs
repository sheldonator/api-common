using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ApiFilters
{
    public class HttpError
    {
        public string Code { get; set; } = "UNK";
        public string Message { get; set; } = "An Error Occured "; //TODO: pull from resource / CMS

        public IDictionary<string, IEnumerable<string>> Errors { get; set; }

        public HttpError(string errorMessage)
        {
            Message = errorMessage;
        }

        public HttpError(string key, string errorMessage)
        {
            Errors = new Dictionary<string, IEnumerable<string>>
            {
                {
                    key, new[] {errorMessage}
                }
            };
        }
        public HttpError(string key, IEnumerable<string> errorMessages)
        {
            Errors = new Dictionary<string, IEnumerable<string>>
            {
                {key, errorMessages}
            };
        }
        public HttpError(ModelStateDictionary modelState)
        {
            Code = "VAL";
            Errors = new Dictionary<string, IEnumerable<string>>();

            foreach (var item in modelState)
            {
                var itemErrors = item.Value.Errors.Select(childItem => childItem.ErrorMessage).ToList();
                Errors.Add(item.Key, itemErrors);
            }
        }
    }
}
