namespace ChatGPT.Net
{
    public class GptApiAttribute
    {
        [AttributeUsage(AttributeTargets.Method, Inherited = false)]
        public class GptApiMethodAttribute : Attribute
        {
            public string Description { get; }

            public GptApiMethodAttribute(string description)
            {
                Description = description;
            }
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        public class GptApiParamAttribute(string paramName, string description = "", bool isOptional = false, params string[] allowedValues) : Attribute
        {
            public string ParamName { get; } = paramName;
            public string Description { get; } = description;
            public bool IsOptional { get; } = isOptional;
            public string[] AllowedValues { get; } = allowedValues;
        }
    }
}
