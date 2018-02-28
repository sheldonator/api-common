using System;

namespace ApiTypes
{
    public class CodeAttribute : Attribute
    {
        public string Code { get; }

        public CodeAttribute(string code)
        {
            Code = code;
        }
    }
}
