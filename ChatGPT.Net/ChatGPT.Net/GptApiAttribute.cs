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
        public class GptApiParamAttribute : Attribute
        {
            public string ParamName { get; }
            public string Description { get; }
            public bool IsOptional { get; }
            public string[] AllowedValues { get; }

            public GptApiParamAttribute(string paramName, string description = "", bool isOptional = false, params string[] allowedValues)
            {
                ParamName = paramName;
                Description = description;
                IsOptional = isOptional;
                AllowedValues = allowedValues;
            }
        }
    }
}
