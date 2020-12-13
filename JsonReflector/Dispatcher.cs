
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace ReflectorServer
{
    // our lookup table has these
    public class ClassEntry
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public object Instance { get; set; }

        public List<object> Describe()
        {
            List<object> all = new();
            all.Add(Name);
            all.Add(Type.FullName);
            foreach (var method in Type.GetMethods())
            {
                var paramss = method.GetParameters().Select(p => $"{p.Name} {p.ParameterType.Name}");
                all.Add(new object[] { method.Name, paramss });

            }
            return all;

        }
    }

    public class Dispatcher
    {
        Dictionary<string, ClassEntry> LookupTable = new();

        // more variants of AddInstance possibly needed, e.g. add with full
        // namespace or own name - or only new up the instance later!
        public void AddInstance<T>(T instance)
        {
            var t = typeof(T);
            LookupTable.Add(t.Name, new ClassEntry {
                Name = t.Name,
                Instance = instance, 
                Type = t,
            });
        }

        public byte[] Describe()
        {
            var all = LookupTable.Values.Select(p => p.Describe()).ToArray();
            return JsonSerializer.SerializeToUtf8Bytes(all);
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