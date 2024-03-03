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

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);

                    foreach (var method in methods)
                    {
                        var attributes = method.GetCustomAttributes(typeof(GptApiMethodAttribute), inherit: false);
                        if (attributes.Length > 0)
                        {
                            apiMethods.Add(method);
                            string fullMethodName = method.DeclaringType.Name + "-" + method.Name;
                            AddToolToClassMapping(method.DeclaringType, fullMethodName);
                        }
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

                    string fullMethodName = method.DeclaringType.Name + "-" + method.Name;


                    toolMappings.Add(fullMethodName, new ToolMapping(method, [.. properties.Keys]));

                    var tool = new GptTool
                    {
                        Type = "function",
                        Function = new GpToolsFunction
                        {
                            Name = fullMethodName,
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


        public static async Task<object> CallFunctionAuto(string functionName, JObject functionArgs)
        {
            if (toolMappings.TryGetValue(functionName, out var functionMapping))
            {
                var method = functionMapping.Method;

                // Check if the method is asynchronous (returns Task or Task<T>)
                if (typeof(Task).IsAssignableFrom(method.ReturnType))
                {
                    // Asynchronous method, call the async handler
                    return await CallFunctionAsync(functionName, functionArgs);
                }
                else
                {
                    // Synchronous method, call the sync handler
                    return CallFunctionSync(functionName, functionArgs);
                }
            }

            return $"Function '{functionName}' not found or arguments mismatch.";
        }

        public static object CallFunctionSync(string functionName, JObject functionArgs)
        {
            if (toolMappings.TryGetValue(functionName, out var functionMapping))
            {
                var method = functionMapping.Method;
                if (typeof(Task).IsAssignableFrom(method.ReturnType))
                {
                    return ($"ERROR: {functionName} is an asynchronous method and should be called using CallFunctionAsync.");
                }

                var parameters = PrepareParameters(method, functionArgs);
                return method.Invoke(null, parameters);
            }

            return $"Function '{functionName}' not found or arguments mismatch.";
        }

        public static async Task<object> CallFunctionAsync(string functionName, JObject functionArgs)
        {
            if (toolMappings.TryGetValue(functionName, out var functionMapping))
            {
                var method = functionMapping.Method;
                if (!typeof(Task).IsAssignableFrom(method.ReturnType))
                {
                    return ($"ERROR: {functionName} is not an asynchronous method and should be called using CallFunctionSync.");
                }

                var parameters = PrepareParameters(method, functionArgs);

                var task = (Task)method.Invoke(null, parameters);
                await task.ConfigureAwait(false);

                if (task.GetType().IsGenericType)
                {
                    return ((dynamic)task).Result;
                }

                return null; // For Task without result
            }

            return $"Function '{functionName}' not found or arguments mismatch.";
        }

        private static object[] PrepareParameters(MethodInfo method, JObject functionArgs)
        {
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

            return parameters;
        }


    }

}
