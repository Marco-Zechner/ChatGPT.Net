using Newtonsoft.Json;

namespace ChatGPT.Net
{
    public class JsonHelper
    {
        public static string ConvertObjectToString(object result)
        {
            if (result == null)
                return "null";
            
            try
            {
                return JsonConvert.SerializeObject(result);
            }
            catch (JsonSerializationException)
            {
                return "Serialization failed";
            }
        }

        public static string GetJsonType(Type type)
        {
            if (type == typeof(string))
                return "string";
            else if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte))
                return "integer";
            else if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
                return "number";
            else if (type == typeof(bool))
                return "boolean";
            else if (type.IsArray || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)))
                return "array";
            else if (type == typeof(object))
                return "object";
            else if (type.IsClass)
                return "object"; // You might want to handle nested objects differently
            else
                return "null"; // This is a fallback and might not be correct for all cases
        }
    }
}
