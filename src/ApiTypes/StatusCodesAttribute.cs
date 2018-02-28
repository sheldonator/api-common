using System;
using System.Collections.Immutable;

namespace ApiTypes
{
    public class StatusCodesAttribute : Attribute
    {
        public IImmutableList<int> AllowedStatusCodes { get; }

        public StatusCodesAttribute(params int[] allowedStatusCodes)
        {
            AllowedStatusCodes = allowedStatusCodes.ToImmutableList();
        }
    }
}
