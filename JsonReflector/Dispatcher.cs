
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

    // our lookup table has these
    public class ClassEntry
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public object Instance { get; set; }
    }

    public class Dispatcher
    {
        Dictionary<string, ClassEntry> LookupTable = new();

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

        public void AddInstance<T>(T instance)
        {
            var t = typeof(T);
            LookupTable.Add(t.Name, new ClassEntry {
                Name = t.Name,
                Instance = instance, 
                Type = t,
            });
        }

        public byte[] DispatchJson(ReadOnlySpan<byte> json)
        {
            var rd = new Utf8JsonReader(json);
            rd.Read(); // startarray

            rd.Read(); 
            var className = rd.GetString();
            rd.Read();
            var methodName = rd.GetString();

            var entry = LookupTable[className];
            var methodInfo = entry.Type.GetMethod(methodName);

            var args = PopulateArguments(methodInfo, ref rd);

            object retVal;
            try
            {
                retVal = methodInfo.Invoke(entry.Instance, args);

            } catch (Exception e)
            {
                retVal = new
                {
                    Exception = e.InnerException.ToString()
                };

            }
            return JsonSerializer.SerializeToUtf8Bytes(retVal);
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