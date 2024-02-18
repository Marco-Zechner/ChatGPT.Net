using Newtonsoft.Json.Linq;
using System.Reflection;
using static ChatGPT.Net.GptApiAttribute;

namespace ChatGPT.Net
{
    public delegate object ToolDelegate(Dictionary<string, object> args);

    public class ToolMapping(MethodInfo method, List<string> argumentNames)
    {
        public MethodInfo Method { get; } = method ?? throw new ArgumentNullException(nameof(method));
        public List<string> ArgumentNames { get; } = argumentNames ?? throw new ArgumentNullException(nameof(argumentNames));
    }

    public class GptToolHandler
    {
        private static Dictionary<string, ToolMapping> toolMappings = [];
        public static Dictionary<Type, List<string>> ClassToMethodNames { private set; get; } = [];
        public static List<GptTool> Tools { private set; get; } = [];

        static GptToolHandler() => LoadOnlyServicesByAttribute();

        public static void LoadOnlyServicesByAttribute()
        {
            HashSet<MethodInfo> apiMethods = [];

            var possibleAPIs = Assembly.GetExecutingAssembly().GetTypes();

            foreach (var possibleAPI in possibleAPIs)
            {
                var methods = possibleAPI.GetMethods(BindingFlags.Public | BindingFlags.Static);

                foreach (var method in methods)
                {
                    // Check if the method has the GptApiMethodAttribute
                    var attributes = method.GetCustomAttributes(typeof(GptApiMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        apiMethods.Add(method);
                        AddToolToClassMapping(method.DeclaringType, method.Name);
                    }
                }
            }

            PopulateFunctionMappingsAndGenerateTools(apiMethods);
        }

        private static void AddToolToClassMapping(Type classType, string methodName)
        {
            if (!ClassToMethodNames.TryGetValue(classType, out List<string>? value))
            {
                ClassToMethodNames[classType] = [methodName];
            }
            else
            {
                value.Add(methodName);
            }
        }



        private static void PopulateFunctionMappingsAndGenerateTools(HashSet<MethodInfo> methods)
        {
            toolMappings = [];
            Tools = [];

            foreach (var method in methods)
            {
                var toolFunctionAttribute = method.GetCustomAttribute<GptApiMethodAttribute>();
                if (toolFunctionAttribute != null)
                {
                    var parameters = method.GetParameters();
                    var properties = new Dictionary<string, GptPropertyDetails>();
                    var requiredParams = new List<string>();
                    var paramAttributes = method.GetCustomAttributes<GptApiParamAttribute>();

                    foreach (var param in parameters)
                    {
                        var paramAttribute = paramAttributes.FirstOrDefault(pa => pa.ParamName == param.Name);
                        var isRequired = paramAttribute == null || !paramAttribute.IsOptional;

                        var details = new GptPropertyDetails
                        {
                            Type = JsonHelper.GetJsonType(param.ParameterType),
                            Description = paramAttribute?.Description ?? "",
                            Enum = paramAttribute?.AllowedValues.ToList() ?? []
                        };

                        properties.Add(param.Name, details);

                        if (isRequired)
                        {
                            requiredParams.Add(param.Name);
                        }
                    }

                    toolMappings.Add(method.Name, new ToolMapping(method, [.. properties.Keys]));

                    var tool = new GptTool
                    {
                        Type = "function",
                        Function = new GpToolsFunction
                        {
                            Name = method.Name,
                            Description = toolFunctionAttribute.Description,
                            Parameters = new GptToolsParameters
                            {
                                Type = "object",
                                Properties = properties,
                                Required = requiredParams
                            }
                        }
                    };
                    Tools.Add(tool);
                }
            }
        }


        public static object CallFunction(string functionName, JObject functionArgs)
        {
            if (toolMappings.TryGetValue(functionName, out var functionMapping))
            {
                var method = functionMapping.Method;
                var parameterInfos = method.GetParameters();
                var parameters = new object[parameterInfos.Length];

                for (int i = 0; i < parameterInfos.Length; i++)
                {
                    var parameterInfo = parameterInfos[i];
                    var paramName = parameterInfo.Name;

                    if (!functionArgs.TryGetValue(paramName, out JToken argToken) && parameterInfo.IsOptional)
                    {
                        parameters[i] = Type.Missing;
                    }
                    else
                    {
                        parameters[i] = argToken.ToObject(parameterInfo.ParameterType);
                    }
                }

                return method.Invoke(null, parameters);
            }

            return $"Function '{functionName}' not found or arguments mismatch.";
        }

    }

}
