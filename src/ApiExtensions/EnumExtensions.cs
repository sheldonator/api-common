using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using ApiTypes;

namespace ApiExtensions
{
    public static class EnumExtensions
    {
        public static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static string Description(this Enum enumerationType)
        {
            var type = enumerationType.GetType();

            var memInfo = type.GetMember(enumerationType.ToString());

            if (memInfo.Length > 0)
            {
                var attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attributes.Any())
                {
                    return ((DescriptionAttribute)attributes.First()).Description;
                }
            }

            return enumerationType.ToString();
        }

        public static string Code(this Enum enumerationType)
        {
            var type = enumerationType.GetType();

            var memInfo = type.GetMember(enumerationType.ToString());

            if (memInfo.Length > 0)
            {
                var attributes = memInfo[0].GetCustomAttributes(typeof(CodeAttribute), false);

                if (attributes.Any())
                {
                    return ((CodeAttribute)attributes.First()).Code;
                }
            }

            return enumerationType.ToString();
        }

        public static IImmutableList<int> AllowedStatusCodes(this Enum enumerationType)
        {
            var type = enumerationType.GetType();

            var memInfo = type.GetMember(enumerationType.ToString());

            if (memInfo.Length > 0)
            {
                var attributes = memInfo[0].GetCustomAttributes(typeof(StatusCodesAttribute), false);

                if (attributes.Any())
                {
                    return ((StatusCodesAttribute)attributes.First()).AllowedStatusCodes;
                }
            }

            return new List<int>().ToImmutableList();
        }

        public static int DefaultStatusCode(this Enum enumerationType)
        {
            var type = enumerationType.GetType();

            var memInfo = type.GetMember(enumerationType.ToString());

            if (memInfo.Length > 0)
            {
                var attributes = memInfo[0].GetCustomAttributes(typeof(StatusCodesAttribute), false);

                if (attributes.Any())
                {
                    return ((StatusCodesAttribute)attributes.First()).AllowedStatusCodes.FirstOrDefault();
                }
            }

            return 0;
        }

        public static bool IsAllowedStatusCode(this Enum enumerationType, int statusCode)
        {
            var type = enumerationType.GetType();

            var memInfo = type.GetMember(enumerationType.ToString());

            if (memInfo.Length > 0)
            {
                var attributes = memInfo[0].GetCustomAttributes(typeof(StatusCodesAttribute), false);

                if (attributes.Any())
                {
                    return ((StatusCodesAttribute)attributes.First()).AllowedStatusCodes.Contains(statusCode);
                }
            }

            return true;
        }
    }
}
