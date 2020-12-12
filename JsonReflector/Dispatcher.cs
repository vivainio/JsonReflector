
using System;
using System.Collections.Generic;
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
        public static object[] PopulateArguments(MethodInfo targetMethod, ref Utf8JsonReader rd)
        {

            var objects = new List<object>();
            foreach (var par in targetMethod.GetParameters())
            {
                rd.Read();
                var extracted = JsonSerializer.Deserialize(ref rd, par.ParameterType);
                objects.Add(extracted);
            }
            return objects.ToArray();

        }

    }
}