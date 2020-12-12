
using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ReflectorServer
{
    public class InvokeDto
    {
        public string Class { get; set; }
        public string Method { get; set; }
        public JsonElement[] Args { get; set; }

    }

    public class Dispatcher
    {
        public static MethodInfo ResolveMethod(string klass, string name)
        {
            var t = Type.GetType(klass);
            var meth = t.GetMethod(name);
            return meth;
        }
        public static Task Run(HttpContext ctx) 
        {
            var text = ctx.Request.ReadFromJsonAsync<InvokeDto>();
            return Task.CompletedTask;

        }

        public static object[] PopulateArguments(MethodInfo targetMethod, JsonElement[] elements)
        {

            var populated = targetMethod.GetParameters().Select((par, i) =>
                JsonElementToObject(elements[i], par.ParameterType)).ToArray();
            return populated;

        }
        public static object JsonElementToObject(JsonElement el, Type targetType)
        {
            object res = targetType switch
            {
                _ when targetType == typeof(string) => el.GetString(),
                _ when targetType == typeof(Int32) => el.GetInt32(),
                
                _ => null // xxx no arrays etc yet
            };
            return res;

        }

    }
}